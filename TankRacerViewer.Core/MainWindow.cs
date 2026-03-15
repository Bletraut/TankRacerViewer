using System.IO;
using System.Linq;
using System.Text;

using FastFileUnpacker;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace TankRacerViewer.Core
{
    public class MainWindow : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private SpriteFont _mainFont;

        private UiComponent _uiComponent;

        private GameWindowRenderContext _gameWindowRenderContext;
        private IRenderContext _currentRenderContext;
        private WorldRenderer _renderer;

        private FastFile _dataFastFile;
        private AssetViewContainer _tanksAssetViewContainer;
        private AssetViewContainer _commonAssetViewContainer;
        private AssetViewContainer _levelAssetViewContainer;

        private LevelView _levelView;
        private int _currentContainerIndex;

        private TankView _tankView;

        private Camera _camera;
        private CameraController _cameraController;
        private Vector3 _cameraDefaultPosition = new(0, 5, 30);
        private Vector3 _cameraDefaultRotation = new(0, 0, 0);

        private readonly StringBuilder _info = new();

        public MainWindow()
        {
            _graphics = new GraphicsDeviceManager(this)
            {
                GraphicsProfile = GraphicsProfile.HiDef
            };

            Content.RootDirectory = "Content";
            IsFixedTimeStep = false;
            IsMouseVisible = true;

            Window.AllowUserResizing = true;
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

            _gameWindowRenderContext = new GameWindowRenderContext(Window);
            _currentRenderContext = _uiComponent.ViewerWindow.RenderContext;

            _renderer = new WorldRenderer(GraphicsDevice, _spriteBatch, Content);
            _renderer.ApplyRenderContext(_currentRenderContext);

            using var dataFileStream = File.OpenRead("Content\\FastFiles\\DATA.DAT");
            _dataFastFile = new FastFile(dataFileStream);

            using var tanksFileStream = File.OpenRead("Content\\FastFiles\\TANKS.DAT");
            var tanksFastFile = new FastFile(tanksFileStream);
            _tanksAssetViewContainer = new AssetViewContainer(GraphicsDevice, tanksFastFile);

            using var commonFileStream = File.OpenRead("Content\\FastFiles\\INGAME.DAT");
            var commonFastFile = new FastFile(commonFileStream);
            _commonAssetViewContainer = new AssetViewContainer(GraphicsDevice, commonFastFile);

            using var levelFileStream = File.OpenRead("Content\\FastFiles\\ENGLAND.DAT");
            //using var levelFileStream = File.OpenRead("Content\\FastFiles\\MEXICO.DAT");
            //using var levelFileStream = File.OpenRead("Content\\FastFiles\\THEME.DAT");
            var levelFastFile = new FastFile(levelFileStream);
            _levelAssetViewContainer = new AssetViewContainer(GraphicsDevice, levelFastFile);

            var levelViewName = levelFastFile.Assets.FirstOrDefault(asset => asset is MapAsset)?.FullName ?? string.Empty;
            _levelView = new LevelView(levelViewName, _commonAssetViewContainer, _levelAssetViewContainer);

            _tankView = new TankView("Tank1", _dataFastFile, _commonAssetViewContainer, [_tanksAssetViewContainer]);

            _camera = new Camera(GraphicsDevice);
            _camera.Position = _cameraDefaultPosition;
            _camera.ApplyRenderContext(_currentRenderContext);

            _cameraController = new CameraController(_camera);
            _cameraController.EulerAngles = _cameraDefaultRotation;
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here
            Input.Update();

            if (Input.IsKeyDown(Keys.R))
            {
                _camera.Position = _cameraDefaultPosition;
                _cameraController.EulerAngles = _cameraDefaultRotation;
            }

            if (Input.IsKeyDown(Keys.NumPad8))
            {
                _currentContainerIndex++;
                if (_currentContainerIndex >= _levelView.LevelObjectContainers.Count)
                    _currentContainerIndex = 0;

                _levelView.CurrentLevelObjectContainer = _levelView.LevelObjectContainers[_currentContainerIndex];
            }
            else if (Input.IsKeyDown(Keys.NumPad5))
            {
                _currentContainerIndex--;
                if (_currentContainerIndex < 0)
                    _currentContainerIndex = 0;

                _levelView.CurrentLevelObjectContainer = _levelView.LevelObjectContainers[_currentContainerIndex];
            }

            if (Input.IsKeyDown(Keys.NumPad6))
            {
                _levelView.CurrentLap++;
                if (_levelView.CurrentLap >= 11)
                    _levelView.CurrentLap = 1;
            }
            else if (Input.IsKeyDown(Keys.NumPad4))
            {
                _levelView.CurrentLap--;
                if (_levelView.CurrentLap < 1)
                    _levelView.CurrentLap = 10;
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
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            _info.Clear();

            if (_levelView is not null)
            {
                _levelView.Draw(_renderer, _camera);

                _info.AppendLine($"Current Container: {_levelView.CurrentLevelObjectContainer.FullName}");
                _info.AppendLine($"Current Lap: {_levelView.CurrentLap}");
            }
            //if (_tankView is not null)
            //{
            //    _tankView.Draw(_renderer, _camera);
            //}

            _info.AppendLine($"Draw Calls: {GraphicsDevice.Metrics.DrawCount}");
            _info.Append($"Fps: {1 / gameTime.ElapsedGameTime.TotalSeconds:00.0}");

            var infoString = _info.ToString();
            var infoSize = _mainFont.MeasureString(infoString);
            var infoPosition = new Vector2(0, GraphicsDevice.Viewport.Height - infoSize.Y);

            _spriteBatch.Begin();
            _spriteBatch.DrawString(_mainFont, infoString, infoPosition, Color.White);
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
