using ComposableUi;

using FastFileUnpacker;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

using TankRacerViewer.Core.Views;

namespace TankRacerViewer.Core
{
    public class MainWindow : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private SpriteFont _mainFont;

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

        private UiManager _uiManager;

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

            _uiManager = new UiManager(GraphicsDevice, Content, _spriteBatch);
            _uiManager.Root.AddChild(new CanvasElement(Window,
                children: [
                    new PanelElement(new Vector2(200, 50), Color.Orange)
                    {
                        Position = new Vector2(100, 200),
                        Pivot = Alignment.MiddleLeft
                    },
                    new ExpandedElement(
                        innerElement: new AlignmentElement(new PanelElement(new Vector2(100, 100), Color.Wheat))
                        {
                            AlignmentFactor = Alignment.MiddleRight,
                            Offset = new Vector2(-20, 0),
                            Pivot = Alignment.MiddleRight
                        },
                        leftPadding: 5,
                        topPadding: 20,
                        bottomPadding: 20,
                        expandWidth: false,
                        expandHeight: true
                    ),
                    new AlignmentElement(new PanelElement(new Vector2(100, 100), Color.Gold))
                    {
                        AlignmentFactor = Alignment.MiddleRight,
                        Offset = new Vector2(-20, 0),
                        Pivot = Alignment.MiddleRight
                    },
                ]));

            var row = new RowLayout(
                alignmentFactor: Alignment.Center,
                spacing: 10
                //expandChildrenMainAxisSize: true
                //expandChildrenCrossAxisSize: true
                );
            row.Size = new Vector2(200, 80);
            //row.Position = new Vector2(300, 50);
            row.Pivot = new Vector2(0.2f);
            for (var i = 0; i < 10; i++)
            {
                var panel = new PanelElement(new Vector2(Random.Shared.Next(10, 100), Random.Shared.Next(10, 100)),
                    new Color(Random.Shared.NextSingle(), Random.Shared.NextSingle(), Random.Shared.NextSingle()));
                panel.Pivot = new Vector2(Random.Shared.NextSingle(), Random.Shared.NextSingle());
                row.AddChild(panel);
            }
            _uiManager.Root.AddChild(new ClipMaskElement(row)
            {
                Position = new Vector2(300, 50),
                Pivot = new Vector2(0.2f)
            });

            var column = new ColumnLayout(
                alignmentFactor: Alignment.TopCenter,
                spacing: 5
                );
            column.Size = new Vector2(80, 500);
            column.Position = new Vector2(0, 50);
            column.Pivot = Alignment.TopLeft;
            for (var i = 0; i < 10; i++)
            {
                var panel = new PanelElement(new Vector2(Random.Shared.Next(10, 100), Random.Shared.Next(10, 100)),
                    new Color(Random.Shared.NextSingle(), Random.Shared.NextSingle(), Random.Shared.NextSingle()));
                panel.Pivot = new Vector2(Random.Shared.NextSingle(), Random.Shared.NextSingle());
                column.AddChild(panel);
            }
            _uiManager.Root.AddChild(column);

            _renderer = new WorldRenderer(GraphicsDevice, _spriteBatch, Content);

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

            _cameraController = new CameraController(_camera);
            _cameraController.EulerAngles = _cameraDefaultRotation;
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here
            Input.Update();
            _uiManager.Update(gameTime);

            _cameraController.Update(gameTime);

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
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            _info.Clear();

            if (_levelView != null)
            {
                _levelView.Draw(_renderer, _camera);

                _info.AppendLine($"Current Container: {_levelView.CurrentLevelObjectContainer.FullName}");
                _info.AppendLine($"Current Lap: {_levelView.CurrentLap}");
            }
            //if (_tankView != null)
            //{
            //    _tankView.Draw(_renderer, _camera);
            //}

            _info.AppendLine($"Draw Calls: {GraphicsDevice.Metrics.DrawCount}");
            _info.Append($"Fps: {1 / gameTime.ElapsedGameTime.TotalSeconds:00.0}");

            var infoString = _info.ToString();
            var infoSize = _mainFont.MeasureString(infoString);
            var infoPosition = new Vector2(0, GraphicsDevice.Viewport.Height - infoSize.Y);

            _spriteBatch.Begin();
            _spriteBatch.DrawString(_mainFont, infoString, infoPosition, Color.Black);
            _spriteBatch.End();

            _uiManager.Draw(gameTime);

            base.Draw(gameTime);
        }
    }
}
