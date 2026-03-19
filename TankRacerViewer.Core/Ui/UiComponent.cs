using ComposableUi;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using TankRacerViewer.Core.Ui;

namespace TankRacerViewer.Core
{
    public sealed partial class UiComponent : DrawableGameComponent
    {
        public UiManager UiManager { get; }

        private readonly IFileDialogService _fileDialogService;

        private readonly CanvasElement _canvas;
        private readonly ContainerElement _mainLayer;
        private readonly ContainerElement _overlayLayer;

        public UiComponent(Game game, IFileDialogService fileDialogService,
            SpriteBatch spriteBatch) : base(game)
        {
            _fileDialogService = fileDialogService;

            UiManager = new UiManager(game.GraphicsDevice, game.Content, spriteBatch);

            _canvas = new CanvasElement(game.Window);
            UiManager.Root.AddChild(_canvas);

            _mainLayer = new ContainerElement();
            _canvas.AddChild(new ExpandedElement(_mainLayer));

            _overlayLayer = new ContainerElement();
            _canvas.AddChild(new ExpandedElement(_overlayLayer));

            CreateWindows();
            CreateMenuBar();
        }

        public override void Update(GameTime gameTime)
        {
            UiManager.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            UiManager.Draw(gameTime);
        }
    }
}
