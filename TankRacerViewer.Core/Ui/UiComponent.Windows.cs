using ComposableUi;

using Microsoft.Xna.Framework;

namespace TankRacerViewer.Core
{
    public sealed partial class UiComponent
    {
        public ViewerWindow ViewerWindow { get; private set; }
        public ExplorerWindow ExplorerWindow { get; private set; }
        public InspectorWindow InspectorWindow { get; private set; }
        public ConsoleWindow ConsoleWindow { get; private set; }
        public AboutWindow AboutWindow { get; private set; }

        private WindowLayout _windowLayout;

        private ExpandedElement _overlayInputInterceptorParent;
        private PointerInputHandlerElement _overlayInputInterceptor;

        private void CreateWindows()
        {
            _windowLayout = new WindowLayout();
            _mainLayer.AddChild(new ExpandedElement(
                topPadding: MenuBarElement.DefaultHeight,
                innerElement: _windowLayout
            ));

            ViewerWindow = new ViewerWindow(GraphicsDevice);
            _windowLayout.EmbedWindow(ViewerWindow);

            ExplorerWindow = new ExplorerWindow();
            _windowLayout.AddFloatWindow(ExplorerWindow);
            WindowElement.Dock(ExplorerWindow, ViewerWindow, Edge.Left);

            InspectorWindow = new InspectorWindow();
            _windowLayout.AddFloatWindow(InspectorWindow);
            WindowElement.Dock(InspectorWindow, ViewerWindow, Edge.Right);

            ConsoleWindow = new ConsoleWindow();
            _windowLayout.AddFloatWindow(ConsoleWindow);
            WindowElement.Dock(ConsoleWindow, ViewerWindow.Container, Edge.Bottom);

            AboutWindow = new AboutWindow(_mainWindow.UrlOpener)
            {
                IsEnabled = false
            };
            AboutWindow.Tab.BlockInput = false;
            AboutWindow.Tab.IsInteractable = false;
            AboutWindow.MaximizeButton.IsEnabled = false;

            AboutWindow.Closed += OnAboutWindowClosed;

            _overlayInputInterceptor = new PointerInputHandlerElement(
                innerElement: new SpriteElement(
                    skin: StandardSkin.WhitePixel,
                    color: new Color(Color.Black, 0.25f)
                )
            );
            _overlayInputInterceptorParent = new ExpandedElement(_overlayInputInterceptor);
        }

        private void ShowWindow(WindowElement window)
        {
            if (!window.IsEnabled)
            {
                _windowLayout.AddFloatWindow(window);

                window.InnerElement.Size = WindowElement.DefaultSize;
                window.LocalPosition = Vector2.Zero;
                window.IsEnabled = true;
            }

            window.Select();
        }

        private void ShowAboutWindow()
        {
            UiManager.Root.ShowInOverlay(_overlayInputInterceptorParent,
                Vector2.Zero, Vector2.Zero);
            UiManager.Root.ShowInOverlay(AboutWindow,
                _windowLayout.Position, Vector2.Zero);

            AboutWindow.InnerElement.Size = WindowElement.DefaultSize;
            AboutWindow.IsEnabled = true;

            AboutWindow.Select();
        }

        private void OnAboutWindowClosed(WindowElement sender)
        {
            _overlayInputInterceptorParent.IsEnabled = false;
        }
    }
}
