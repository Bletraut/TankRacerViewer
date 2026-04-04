using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

using FastFileUnpacker;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using TankRacerViewer.Core.Ui;

namespace TankRacerViewer.Core
{
    public class MainWindow : Game
    {
        private const float ViewDistance = 1500;

        public IFileDialogProvider FileDialogProvider { get; }

        private GraphicsDeviceManager _graphics;
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

        private readonly Dictionary<string, AssetViewContainer> _assetViewContainers = [];
        private AssetViewContainer _dataAssetViewContainer;
        private AssetViewContainer _commonAssetViewContainer;

        private readonly Action<LevelObject> _levelObjectSelectedAction;

        private readonly StringBuilder _info = new();

        public MainWindow(IFileDialogProvider fileDialogProvider)
        {
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
            _mainFont = Content.Load<SpriteFont>("Fonts\\MainFont");

            _uiComponent = new UiComponent(this, _spriteBatch);
            Components.Add(_uiComponent);

            _uiComponent.ExplorerWindow.AssetViewSelected += OnAssetViewSelected;

            _gameWindowRenderContext = new GameWindowRenderContext(Window);
            _currentRenderContext = _uiComponent.ViewerWindow.RenderContext;

            _renderer = new WorldRenderer(GraphicsDevice, _spriteBatch, Content);
            _renderer.ApplyRenderContext(_currentRenderContext);

            _camera = new Camera(GraphicsDevice);
            _camera.Position = _cameraDefaultPosition;
            _camera.ApplyRenderContext(_currentRenderContext);

            _cameraController = new CameraController(_camera);
            _cameraController.EulerAngles = _cameraDefaultRotation;
        }

        public void OpenGameFolder(string[] filePaths)
        {
            foreach (var filePath in filePaths)
                LoadFile(filePath);

            _dataAssetViewContainer = _assetViewContainers.Values
                .FirstOrDefault(container => container.DataAssetViews.ContainsKey("tank1"));
            _commonAssetViewContainer = _assetViewContainers.Values
                .FirstOrDefault(container => container.ModelAssetViews.ContainsKey("camera"));
            var canCreateExtraAssetViews = _dataAssetViewContainer is not null
                && _commonAssetViewContainer is not null;

            foreach (var (path, assetViewContainer) in _assetViewContainers)
            {
                if (canCreateExtraAssetViews)
                {
                    var mapAsset = assetViewContainer.FastFile.Assets.FirstOrDefault(asset => asset is MapAsset);
                    if (mapAsset is not null)
                    {
                        var levelView = new LevelView(mapAsset.FullName,
                            _commonAssetViewContainer, assetViewContainer);
                        assetViewContainer.ExtraAssetViews.Add(mapAsset.FullName, levelView);
                    }

                    foreach (var dataAssetView in _dataAssetViewContainer.DataAssetViews.Values)
                    {
                        if (!TankView.IsTankData(dataAssetView))
                            continue;

                        if (assetViewContainer.ModelAssetViews.TryGetValue($"{dataAssetView.Name.ToLower()}t",
                            out var tankTurretModel))
                        {
                            var tankView = new TankView(dataAssetView.Name, dataAssetView,
                                _commonAssetViewContainer, assetViewContainer);
                            assetViewContainer.ExtraAssetViews.Add(dataAssetView.Name, tankView);
                        }
                    }
                }

                _uiComponent.ExplorerWindow.AddFile(path, assetViewContainer);
            }
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
                }
            }
            catch (Exception exception)
            {
                Debug.WriteLine($"Can't load file '{filePath}'. {exception.Message}");
            }
        }

        private void ResetCameraToDefaults()
        {
            _camera.Position = _cameraDefaultPosition;
            _cameraController.EulerAngles = _cameraDefaultRotation;
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here
            Input.Update();

            if (Input.IsKeyDown(Keys.R))
                ResetCameraToDefaults();

            if (_uiComponent.ExplorerWindow.IsSelected)
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
            _info.Clear();

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

            _info.AppendLine($"Draw Calls: {GraphicsDevice.Metrics.DrawCount}");
            _info.Append($"Fps: {1 / gameTime.ElapsedGameTime.TotalSeconds:00.0}");

            var infoString = _info.ToString();
            var infoSize = _mainFont.MeasureString(infoString);
            var infoPosition = new Vector2(0, GraphicsDevice.Viewport.Height - infoSize.Y);

            _spriteBatch.Begin();
            _spriteBatch.DrawString(_mainFont, infoString, infoPosition, Color.White);
            _spriteBatch.End();
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

                foreach (var levelObjectA in levelView.CurrentLevelObjectContainer.LevelObjects)
                {
                    if (!levelObjectA.IsWayPoint)
                        continue;

                    var distance = Vector3.DistanceSquared(levelObjectA.Position, objectPosition);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        closestWayPoint = levelObjectA;
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
