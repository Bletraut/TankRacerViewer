using Microsoft.Xna.Framework;

namespace ComposableUi
{
    public sealed class Window2Layout : ContainerElement
    {
        private readonly ContainerElement _windowContainer;
        private readonly Window2Element _tempWindow;
        private readonly HolderElement _overlayWindowHolder;

        public Window2Layout()
        {
            _windowContainer = new ContainerElement();
            AddChild(new ExpandedElement(_windowContainer));

            _tempWindow = Window2Element.CreateNonInteractiveWindow();
            _tempWindow.IsEnabled = false;
            AddChild(_tempWindow);

            _overlayWindowHolder = new HolderElement()
            {
                IsEnabled = false
            };
            AddChild(_overlayWindowHolder);
        }

        public void AddWindow(Window2Element window)
        {
            window.TabPointerDown += OnTabPointerDown;
            window.TabPointerUp += OnTabPointerUp;
            window.TabPointerDrag += OnTabPointerDrag;
            window.SplitPreviewShown += OnSplitPreviewShown;
            window.SplitPreviewHidden += OnSplitPreviewHidden;

            _windowContainer.AddChild(window);
        }

        private void ShowTempWindow(Window2Element source, Vector2 position)
        {
            _tempWindow.IsEnabled = true;
            _tempWindow.InnerElement.Size = source.Size;
            _tempWindow.Pivot = source.Pivot;
            _tempWindow.Tab.CopyHeaderFrom(source.Tab);
            _tempWindow.Position = source.Position;
        }

        private void HideTempTab()
        {
            _tempWindow.IsEnabled = false;
        }

        private void OnTabPointerDown(Window2Element window, Point position)
        {
            ShowTempWindow(window, position.ToVector2());
        }

        private void OnTabPointerUp(Window2Element window, Point position)
        {
            HideTempTab();
        }

        private void OnTabPointerDrag(Window2Element window, Point delta)
        {
            _tempWindow.Position += delta.ToVector2();
        }

        private void OnSplitPreviewShown(Window2Element window)
        {
            HideTempTab();
        }

        private void OnSplitPreviewHidden(Window2Element window)
        {
            _tempWindow.IsEnabled = true;
        }
    }
}
