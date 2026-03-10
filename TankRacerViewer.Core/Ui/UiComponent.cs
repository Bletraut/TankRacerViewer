using ComposableUi;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TankRacerViewer.Core
{
    public sealed class UiComponent : DrawableGameComponent
    {
        private readonly UiManager _uiManager;

        private readonly CanvasElement _canvas;

        private MenuBar _menuBar;

        public UiComponent(Game game, SpriteBatch spriteBatch) : base(game)
        {
            _uiManager = new UiManager(game.GraphicsDevice, game.Content, spriteBatch);

            _canvas = new CanvasElement(game.Window);
            _uiManager.Root.AddChild(_canvas);

            CreateMenuBar();
        }

        private void CreateMenuBar()
        {
            _menuBar = new MenuBar();
            _canvas.AddChild(new ExpandedElement(
                expandHeight: false,
                innerElement: new AlignmentElement(
                    alignmentFactor: Alignment.TopCenter,
                    pivot: Alignment.TopCenter,
                    innerElement: _menuBar
                )
            ));
        }

        public override void Update(GameTime gameTime)
        {
            _uiManager.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            _uiManager.Draw(gameTime);
        }
    }
}
