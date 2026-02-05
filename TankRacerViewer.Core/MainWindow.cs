using ComposableUi;
using ComposableUi.Elements;

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
        private ContextMenuElement _contextMenu;

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

            var column = new ColumnLayout(
                spacing: 10,
                sizeMainAxisToContent: true,
                sizeCrossAxisToContent: true
                );

            // FOR TEST
            var innerColumn = new ColumnLayout(
                spacing: 5,
                sizeMainAxisToContent: true,
                sizeCrossAxisToContent: true);
            for (var i = 0; i < 30; i++)
            {
                var color = new Color(Random.Shared.NextSingle(),
                    Random.Shared.NextSingle(), Random.Shared.NextSingle());
                var button = new SpriteElement(
                    size: new Vector2(Random.Shared.Next(50, 400), Random.Shared.Next(50, 200)),
                    skin: StandardSkin.RectangleButton,
                    drawMode: DrawMode.Simple,
                    color: color);
                innerColumn.AddChild(button);
            }
            var innerScroll = new ScrollViewElement(
                size: new Vector2(300, 500),
                content: innerColumn);
            innerScroll.Background.Skin = StandardSkin.WindowHeader;
            //column.AddChild(new SpriteElement(
            //    size: new Vector2(Random.Shared.Next(500, 800), Random.Shared.Next(50, 200)),
            //    skin: StandardSkin.RectangleButton,
            //    drawMode: DrawMode.Simple));
            // end;

            _uiManager = new UiManager(GraphicsDevice, Content, _spriteBatch);

            _contextMenu = new ContextMenuElement()
            {
                Pivot = Alignment.TopLeft,
                IsEnabled = false
            };
            _contextMenu.AddItem(new ContextMenuItemElement(key: "Who is\\Baka\\Gaygin?", name: "MAAAAAX"));
            _contextMenu.AddItem(new ContextMenuItemElement(key: "Who is\\Baka\\Virgin?", name: "MAAAAAX"));
            _contextMenu.AddItem(new ContextMenuItemElement(key: "Who is\\Baka\\Ebaka?", name: "MAX"));
            _contextMenu.AddItem(new ContextMenuItemElement(key: "Who is\\Baka\\Ebaka?", name: "MAAX"));
            _contextMenu.AddItem(new ContextMenuItemElement(key: "Who is\\Baka\\Ebaka?", name: "MAAAX"));
            _contextMenu.AddItem(new ContextMenuItemElement(key: "Gay", name: "Ilusha"));
            _contextMenu.AddItem(new ContextMenuItemElement(key: "Gay", name: "Tolya", keyBindings: "Alt+Gay"));
            _contextMenu.AddItem(new ContextMenuItemElement(key: "Gay", name: "Hemul", keyBindings: "Alt+Alt"));
            _contextMenu.AddItem(new ContextMenuItemElement(name: "Epstein Files", isInteractable: false));
            _contextMenu.AddItem(new ContextMenuItemElement(name: "Item 228", keyBindings: "Alt+F4"));
            _contextMenu.AddItem(new ContextMenuItemElement(key: "There are only two genders", name: "Male"));
            _contextMenu.AddItem(new ContextMenuItemElement(key: "There are only two genders", name: "Female"));

            _uiManager.Root.AddChild(new CanvasElement(Window,
                children: [
                    //new ButtonElement(new Vector2(200, 50))
                    //{
                    //    Position = new Vector2(100, 200),
                    //    Pivot = Alignment.MiddleLeft
                    //},
                    //new TextElement(
                    //    //spriteFont: _mainFont,
                    //    text: "Text Element SUPER BIG..gq.?",
                    //    textAlignmentFactor: Alignment.Center,
                    //    pivot: Alignment.TopLeft,
                    //    color: Color.Yellow
                    //)
                    //{
                    //    Position = new Vector2(100, 100)
                    //},
                    new ExpandedElement(
                        innerElement: new AlignmentElement(new ButtonElement(new Vector2(100, 100)))
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
                    new AlignmentElement(new ButtonElement(new Vector2(100, 100)))
                    {
                        AlignmentFactor = Alignment.MiddleRight,
                        Offset = new Vector2(-20, 0),
                        Pivot = Alignment.MiddleRight
                    },
                    new WindowElement(
                        content: new ExpandedElement(
                            innerElement: new ScrollViewElement(
                                //expandContentWidth: true,
                                content: column
                            )
                        )
                    )
                    {
                        Position = new Vector2(200, 300)
                    },
                    _contextMenu,
                ]));

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

            var col = 0;
            foreach (var (key, textureAssetView) in _levelAssetViewContainer.TextureAssetViews)
            {
                var sprite = new SpriteElement(
                    sprite: new Sprite()
                    {
                        Texture = textureAssetView.Texture,
                        SourceRectangle = new Rectangle(0, 0, textureAssetView.Texture.Width, textureAssetView.Texture.Height)
                    },
                    drawMode: DrawMode.Simple,
                    sizeToSource: true);
                column.AddChild(sprite);

                col++;
                if (col == 16)
                    column.AddChild(innerScroll);
            }

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

            if (Input.IsMouseButtonUp(MouseButton.Left))
                _contextMenu.Hide();
            if (Input.IsMouseButtonDown(MouseButton.Right))
                _contextMenu.Show(Input.MousePosition.ToVector2());

            _uiManager.Update(gameTime);

            if (!_uiManager.IsAnyElementPressed)
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

            _uiManager.Draw(gameTime);

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
