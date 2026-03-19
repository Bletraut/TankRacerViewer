using System.Diagnostics;

using ComposableUi;

using Microsoft.Xna.Framework;

namespace TankRacerViewer.Core
{
    public sealed partial class UiComponent
    {
        public ViewerWindow ViewerWindow { get; private set; }
        public ExplorerWindow ExplorerWindow { get; private set; }
        public InspectorWindow InspectorWindow { get; private set; }

        private WindowLayout _windowLayout;

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
    }
}
