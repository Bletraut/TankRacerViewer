using System;

using ComposableUi;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace TankRacerViewer.Core
{
    public sealed partial class UiComponent : DrawableGameComponent
    {
        public UiManager UiManager { get; }

        private readonly MainWindow _mainWindow;

        private readonly ContainerElement _mainLayer;
        private readonly ContainerElement _overlayLayer;

        public UiComponent(MainWindow mainWindow, SpriteBatch spriteBatch) : base(mainWindow)
        {
            _mainWindow = mainWindow;
            _mainWindow.Window.ClientSizeChanged += OnClientSizeChanged;

            UiManager = new UiManager(mainWindow.GraphicsDevice, mainWindow.Content, spriteBatch);
            RefreshUiRootSize();

            _mainLayer = new ContainerElement();
            UiManager.Root.AddChild(new ExpandedElement(_mainLayer));

            _overlayLayer = new ContainerElement();
            UiManager.Root.AddChild(new ExpandedElement(_overlayLayer));

            CreateWindows();
            CreateMenuBar();
        }

        private void RefreshUiRootSize()
        {
            UiManager.Root.Size = _mainWindow.Window.ClientBounds.Size.ToVector2();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (_mainWindow.Window is not null)
                _mainWindow.Window.ClientSizeChanged -= OnClientSizeChanged;
        }

        public override void Update(GameTime gameTime)
        {
            UiManager.Update(gameTime);

            var isControlPressed = Input.IsKeyPressed(Keys.LeftControl)
                || Input.IsKeyPressed(Keys.RightControl);
            if (isControlPressed && Input.IsKeyDown(Keys.O))
                OpenGameFolder();
        }

        public override void Draw(GameTime gameTime)
        {
            UiManager.Draw(gameTime);
        }

        private void OnClientSizeChanged(object sender, EventArgs arguments)
        {
            RefreshUiRootSize();
        }
    }
}
