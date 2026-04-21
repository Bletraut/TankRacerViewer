using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FastFileUnpacker;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace TankRacerViewer.Core
{
    public class MainWindow : Game
    {
        private const string RecentPathsDataKey = "save";
        private const int MaxRecentPathCount = 5;

        private const float ViewDistance = 1500;

        private const string AdvancedModeHitMessage = "Press Esc or Ctrl+F to exit Advanced Mode";
        private const float AdvancedModeHitDurationSeconds = 3f;

        public IFileDialogProvider FileDialogProvider { get; }
        public IPlatformUrlOpener UrlOpener { get; }

        private bool IsRenderingToGameWindow => _renderer.RenderContext == _gameWindowRenderContext;

        private readonly GraphicsDeviceManager _graphics;
        private readonly PersistentDataService _persistentDataService;
        private SpriteBatch _spriteBatch;

        private SpriteFont _mainFont;

        private UiComponent _uiComponent;

        private GameWindowRenderContext _gameWindowRenderContext;
        private IRenderContext _currentRenderContext;
        private WorldRenderer _renderer;

        private Camera _camera;
        private CameraController _cameraController;
        private Vector3 _cameraDefaultPosition = new(0, 5, 30);
        private Vector3 _cameraDefaultRotation = new(0, 0, 0);

        private AssetView _selectedAssetView;

        private readonly List<(string Path, AssetViewContainer AssetViewContainer)> _loadedAssetViewContainers = [];
        private readonly Dictionary<string, AssetViewContainer> _assetViewContainers = [];
        private AssetViewContainer _dataAssetViewContainer;
        private AssetViewContainer _commonAssetViewContainer;

        private readonly Action<LevelObject> _levelObjectSelectedAction;

        private readonly StringBuilder _info = new();

        private float _advancedModeHintCountdown;

        private List<string> _recentPaths = [];

        public MainWindow(IPlatformStorage platformStorage,
            IPlatformUrlOpener urlOpener,
            IFileDialogProvider fileDialogProvider)
        {
            _persistentDataService = new PersistentDataService(platformStorage);

            UrlOpener = urlOpener;
            FileDialogProvider = fileDialogProvider;

            _graphics = new GraphicsDeviceManager(this)
            {
                GraphicsProfile = GraphicsProfile.HiDef
            };

            Content.RootDirectory = "Content";
            IsFixedTimeStep = false;
            IsMouseVisible = true;

            Window.AllowUserResizing = true;

            _levelObjectSelectedAction = OnLevelObjectSelected;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            IconCollection.Initialize(Content);

            _mainFont = Content.Load<SpriteFont>("Fonts\\MainFont");

            _uiComponent = new UiComponent(this, _spriteBatch);
            _uiComponent.ViewerWindow.HideViewer();
            Components.Add(_uiComponent);

            _uiComponent.ExplorerWindow.AssetViewSelected += OnAssetViewSelected;

            _gameWindowRenderContext = new GameWindowRenderContext(Window);
            _currentRenderContext = _uiComponent.ViewerWindow.RenderContext;

            _renderer = new WorldRenderer(GraphicsDevice, _spriteBatch, Content);
            _renderer.ApplyRenderContext(_currentRenderContext);

            _camera = new Camera(GraphicsDevice)
            {
                NearPlane = 0.15f,
                FarPlane = 1_000
            };
            _camera.Position = _cameraDefaultPosition;
            _camera.ApplyRenderContext(_currentRenderContext);

            _cameraController = new CameraController(_camera);
            _cameraController.EulerAngles = _cameraDefaultRotation;

            var data = Task.Run(() => _persistentDataService.LoadAsync<List<string>>(RecentPathsDataKey)).Result;
            _recentPaths = data ?? _recentPaths;

            _uiComponent.RecentPaths = _recentPaths;
            _uiComponent.RefreshRecentPaths();
        }

        public void OpenGameFolder(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                _uiComponent.ConsoleWindow.LogMessage(MessageType.Error,
                    $"Directory not found: '{folderPath}'.");
                return;
            }

            var filePaths = Directory.GetFiles(folderPath, "*.dat", SearchOption.AllDirectories);

            _loadedAssetViewContainers.Clear();
            foreach (var filePath in filePaths)
                LoadFile(filePath);

            AddPathToRecentAndRefresh(folderPath);
            ProcessLoadedAssetViewContainers();
        }

        public void OpenFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                _uiComponent.ConsoleWindow.LogMessage(MessageType.Error,
                    $"File not found: '{filePath}'.");
                return;
            }

            _loadedAssetViewContainers.Clear();
            LoadFile(filePath);

            AddPathToRecentAndRefresh(filePath);
            ProcessLoadedAssetViewContainers();
        }

        public void ClearRecentPaths()
        {
            _recentPaths.Clear();
            _uiComponent.RefreshRecentPaths();

            SaveRecentPaths();
        }

        public void RecreateAllExtraAssetViewsIfPossible()
        {
            if (TryCreateExtraAssetViewsIfPossible(_assetViewContainers.Values))
                _uiComponent.ExplorerWindow.RefreshExtraAssetViewNodes();
        }

        public void ToggleRenderContext()
        {
            if (IsRenderingToGameWindow)
            {
                _advancedModeHintCountdown = 0;

                _uiComponent.Enabled = true;
                _uiComponent.Visible = true;
                _renderer.ApplyRenderContext(_uiComponent.ViewerWindow.RenderContext);
            }
            else
            {
                _advancedModeHintCountdown = AdvancedModeHitDurationSeconds;

                _uiComponent.Enabled = false;
                _uiComponent.Visible = false;
                _renderer.ApplyRenderContext(_gameWindowRenderContext);
            }
            _camera.ApplyRenderContext(_renderer.RenderContext);
        }

        private void AddPathToRecentAndRefresh(string path)
        {
            while (_recentPaths.Count > MaxRecentPathCount)
                _recentPaths.RemoveAt(_recentPaths.Count - 1);

            _recentPaths.Remove(path);
            _recentPaths.Insert(0, path);

            _uiComponent.RefreshRecentPaths();

            SaveRecentPaths();
        }

        private void SaveRecentPaths()
        {
            Task.Run(async () =>
            {
                try
                {
                    await _persistentDataService.SaveAsync(RecentPathsDataKey, _recentPaths);
                }
                catch (Exception exception)
                {
                    _uiComponent.ConsoleWindow.LogMessage(MessageType.Error, exception.Message);
                }
            });
        }

        private void ProcessLoadedAssetViewContainers()
        {
            _uiComponent.ConsoleWindow.LogMessage(MessageType.Info,
                $"FastFiles loaded: {_loadedAssetViewContainers.Count}.");

            if (_loadedAssetViewContainers.Count <= 0)
                return;

            TryCreateExtraAssetViewsIfPossible(_loadedAssetViewContainers.Select(data => data.AssetViewContainer));
            _uiComponent.ExplorerWindow.AddFastFiles(_loadedAssetViewContainers);
        }

        private bool TryCreateExtraAssetViewsIfPossible(IEnumerable<AssetViewContainer> assetViewContainers)
        {
            _dataAssetViewContainer = _assetViewContainers.Values
                .FirstOrDefault(container => container.DataAssetViews.ContainsKey("tank1"));
            _commonAssetViewContainer = _assetViewContainers.Values
                .FirstOrDefault(container => container.ModelAssetViews.ContainsKey("camera"));
            var canCreateExtraAssetViews = _dataAssetViewContainer is not null
                && _commonAssetViewContainer is not null;

            if (!canCreateExtraAssetViews)
            {
                _uiComponent.ConsoleWindow.LogMessage(MessageType.Error,
                    "Cannot create additional asset views: FastFiles 'Data' and 'Ingame' were not found.");

                return false;
            }

            var levelViewCount = 0;
            var tankViewCount = 0;

            foreach (var assetViewContainer in assetViewContainers)
            {
                var mapAsset = assetViewContainer.FastFile.Assets.FirstOrDefault(asset => asset is MapAsset);
                var canCreateMapAsset = mapAsset is not null
                    && !assetViewContainer.ExtraAssetViews.ContainsKey(mapAsset.FullName);
                if (canCreateMapAsset)
                {
                    levelViewCount++;

                    var levelView = new LevelView(mapAsset.FullName,
                        _commonAssetViewContainer, assetViewContainer);
                    assetViewContainer.ExtraAssetViews.Add(mapAsset.FullName, levelView);
                }

                foreach (var dataAssetView in _dataAssetViewContainer.DataAssetViews.Values)
                {
                    if (!TankView.IsTankData(dataAssetView))
                        continue;

                    if (!assetViewContainer.ModelAssetViews.TryGetValue($"{dataAssetView.Name.ToLower()}t",
                        out var tankTurretModel))
                        continue;

                    if (assetViewContainer.ExtraAssetViews.ContainsKey(dataAssetView.Name))
                        continue;

                    tankViewCount++;

                    var tankView = new TankView(dataAssetView.Name, dataAssetView,
                        _commonAssetViewContainer, assetViewContainer);
                    assetViewContainer.ExtraAssetViews.Add(dataAssetView.Name, tankView);
                }
            }

            var createdExtraAssetCount = levelViewCount + tankViewCount;
            if (createdExtraAssetCount > 0)
            {
                _uiComponent.ConsoleWindow.LogMessage(MessageType.Info,
                    $"Additional assets created: TankViews={tankViewCount}, LevelViews={levelViewCount}.");

                return true;
            }

            _uiComponent.ConsoleWindow.LogMessage(MessageType.Info,
                "No extra assets created (already exist).");

            return false;
        }

        private void LoadFile(string filePath)
        {
            if (_assetViewContainers.ContainsKey(filePath))
                return;

            try
            {
                using var fileStream = File.OpenRead(filePath);
                if (FastFile.FromStream(fileStream, out var fastFile))
                {
                    var assetViewContainer = new AssetViewContainer(GraphicsDevice, fastFile);
                    _assetViewContainers.Add(filePath, assetViewContainer);
                    _loadedAssetViewContainers.Add((filePath, assetViewContainer));
                }
            }
            catch (Exception exception)
            {
                _uiComponent.ConsoleWindow.LogMessage(MessageType.Error,
                    $"Can't load file '{filePath}'. {exception.Message}");
#if DEBUG
                throw;
#endif
            }
        }

        private void ResetCameraToDefaults()
        {
            _camera.Position = _cameraDefaultPosition;
            _cameraController.EulerAngles = _cameraDefaultRotation;
        }

        protected override void Update(GameTime gameTime)
        {
            // TODO: Add your update logic here
            Input.Update();

            if (Input.IsKeyPressed(Keys.LeftControl) && Input.IsKeyDown(Keys.F))
                ToggleRenderContext();

            if (IsRenderingToGameWindow && Input.IsKeyDown(Keys.Escape))
                ToggleRenderContext();

            if (Input.IsKeyDown(Keys.R))
                ResetCameraToDefaults();

            _advancedModeHintCountdown -= (float)gameTime.ElapsedGameTime.TotalSeconds;

            var canSelectNode = !IsRenderingToGameWindow
                && _uiComponent.ExplorerWindow.IsSelected;
            if (canSelectNode)
            {
                var isUpPressed = Input.IsKeyDown(Keys.Up)
                    || Input.IsKeyDown(Keys.W)
                    || Input.IsKeyDown(Keys.NumPad8);
                var isDownPressed = Input.IsKeyDown(Keys.Down)
                    || Input.IsKeyDown(Keys.S)
                    || Input.IsKeyDown(Keys.NumPad2);

                if (isUpPressed)
                {
                    _uiComponent.ExplorerWindow.SelectPreventNode();
                }
                else if (isDownPressed)
                {
                    _uiComponent.ExplorerWindow.SelectNextNode();
                }
            }

            base.Update(gameTime);

            if (_renderer.RenderContext == _gameWindowRenderContext)
            {
                if (!_uiComponent.UiManager.IsAnyElementPressed)
                    _cameraController.Update(gameTime);
            }
            else
            {
                if (_uiComponent.ViewerWindow.IsInputAvailable)
                    _cameraController.Update(gameTime);
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            // TODO: Add your drawing code here

            if (_selectedAssetView is not null)
            {
                if (_selectedAssetView is ModelAssetView modelAssetView)
                {
                    _renderer.Begin(Color.CornflowerBlue);
                    _renderer.Draw(modelAssetView, Matrix.Identity, _camera);
                    _renderer.End();
                }
                else if (_selectedAssetView is BackgroundAssetView backgroundAssetView)
                {
                    _renderer.Begin(Color.CornflowerBlue);
                    _renderer.Draw(backgroundAssetView, _camera);
                    _renderer.End();
                }
                else if (_selectedAssetView is LevelView levelView)
                {
                    levelView.Draw(_renderer, _camera);
                }
                else if (_selectedAssetView is TankView tankView)
                {
                    tankView.Draw(_renderer, _camera);
                }
            }

            base.Draw(gameTime);

            _info.Clear();
            _info.AppendLine($"Draw Count: {_renderer.DrawCount}");
            _info.AppendLine($"Triangle Count: {_renderer.TriangleCount}");
            _info.Append($"Fps: {1 / gameTime.ElapsedGameTime.TotalSeconds:00.0}");
            _uiComponent.ViewerWindow.RenderInfo.Text = _info.ToString();

            if (_advancedModeHintCountdown > 0)
            {
                var infoSize = _mainFont.MeasureString(AdvancedModeHitMessage);
                var infoPosition = new Vector2()
                {
                    X = (GraphicsDevice.Viewport.Width - infoSize.X) / 2,
                    Y = infoSize.Y * 2
                };

                _spriteBatch.Begin();

                _spriteBatch.Draw(WorldRenderer.WhitePixelTexture, infoPosition - infoSize / 2, null,
                    new Color(Color.Black, 0.85f), 0, Vector2.Zero, infoSize * 2, SpriteEffects.None, 0);
                _spriteBatch.DrawString(_mainFont, AdvancedModeHitMessage, infoPosition, Color.White);

                _spriteBatch.End();
            }
        }

        private void OnAssetViewSelected(AssetView assetView)
        {
            if (_selectedAssetView == assetView)
                return;

            _selectedAssetView = assetView;
            if (_selectedAssetView is not null)
            {
                if (_selectedAssetView is ModelAssetView modelAssetView)
                {
                    _uiComponent.InspectorWindow.ShowModelInspector(modelAssetView);
                    _uiComponent.ViewerWindow.Show3DViewer();
                }
                else if (_selectedAssetView is BackgroundAssetView backgroundAssetView)
                {
                    _uiComponent.InspectorWindow.ShowBackgroundInspector(backgroundAssetView);
                    _uiComponent.ViewerWindow.Show3DViewer();
                }
                else if (_selectedAssetView is TextureAssetView textureAssetView)
                {
                    _uiComponent.InspectorWindow.ShowTextureInspector(textureAssetView);
                    _uiComponent.ViewerWindow.ShowTextureViewer(textureAssetView.Texture);
                }
                else if (_selectedAssetView is DataAssetView dataAssetView)
                {
                    _uiComponent.InspectorWindow.HideInspector();
                    _uiComponent.ViewerWindow.ShowTextViewer(dataAssetView.Text);
                }
                else if (_selectedAssetView is UnsupportedAssetView unsupportedAssetView)
                {
                    _uiComponent.InspectorWindow.HideInspector();
                    _uiComponent.ViewerWindow.ShowTextViewer(unsupportedAssetView.Description);
                }
                else if (_selectedAssetView is LevelView levelView)
                {
                    _uiComponent.InspectorWindow.ShowLevelInspector(levelView,
                        _levelObjectSelectedAction);
                    _uiComponent.ViewerWindow.Show3DViewer();
                }
                else if (_selectedAssetView is TankView tankView)
                {
                    _uiComponent.InspectorWindow.ShowTankInspector(tankView,
                        _levelObjectSelectedAction);
                    _uiComponent.ViewerWindow.Show3DViewer();
                }
            }
            else
            {
                _uiComponent.InspectorWindow.HideInspector();
                _uiComponent.ViewerWindow.HideViewer();
            }

            ResetCameraToDefaults();
        }

        private void OnLevelObjectSelected(LevelObject levelObject)
        {
            var boundingSphere = BoundingSphere.CreateFromBoundingBox(levelObject.ModelAssetView.BoundingBox);
            var objectPosition = Vector3.Transform(boundingSphere.Center, levelObject.ModelMatrix);

            var viewDirection = Vector3.One;
            if (!levelObject.IsWayPoint && _selectedAssetView is LevelView levelView)
            {
                var minDistance = float.MaxValue;
                LevelObject closestWayPoint = null;

                foreach (var wayPoint in levelView.CurrentLevelObjectContainer.LevelObjects)
                {
                    if (!wayPoint.IsWayPoint)
                        continue;

                    var distance = Vector3.DistanceSquared(wayPoint.Position, objectPosition);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        closestWayPoint = wayPoint;
                    }
                }

                if (closestWayPoint is not null)
                {
                    viewDirection = Vector3.Normalize(closestWayPoint.Position - objectPosition);
                    viewDirection = Vector3.Normalize(viewDirection with { Y = 0.65f });
                }
            }
            var viewPosition = objectPosition + viewDirection * MathF.Max(ViewDistance, boundingSphere.Radius) * 1.5f;

            var worldObjectPosition = Vector3.Transform(objectPosition, _renderer.WorldScaleMatrix);
            var worldViewPosition = Vector3.Transform(viewPosition, _renderer.WorldScaleMatrix);

            _cameraController.Position = worldViewPosition;
            _cameraController.LookAt(worldObjectPosition);
        }
    }
}
