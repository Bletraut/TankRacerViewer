using Microsoft.Xna.Framework;

namespace ComposableUi
{
    public sealed class WindowLayout : ContainerElement
    {
        private readonly ContainerElement _windowContainer;
        private readonly WindowElement _tempWindow;
        private readonly TabElement _tempTab;
        private readonly HolderElement _overlayWindowHolder;

        private Element _tempTabPlaceHolder;

        private bool _isInsertPreviewShown;
        private bool _isSplitPreviewShown;

        private WindowElement _currentFocusedWindow;

        public WindowLayout()
        {
            _windowContainer = new ContainerElement();
            AddChild(new ExpandedElement(_windowContainer));

            _tempWindow = WindowElement.CreateNonInteractiveWindow();
            _tempWindow.IsEnabled = false;
            AddChild(_tempWindow);

            _tempTab = new TabElement
            {
                IsEnabled = false,
                IsInteractable = false,
                BlockInput = false
            };
            AddChild(_tempTab);

            _overlayWindowHolder = new HolderElement()
            {
                IsEnabled = false
            };
            AddChild(_overlayWindowHolder);
        }

        public void AddWindow(WindowElement window)
        {
            window.TabPointerDown += OnTabPointerDown;
            window.TabPointerUp += OnTabPointerUp;
            window.TabPointerDrag += OnTabPointerDrag;
            window.TabPreviewShown += OnTabPreviewShown;
            window.TabPreviewHidden += OnTabPreviewHidden;
            window.SplitPreviewShown += OnSplitPreviewShown;
            window.SplitPreviewHidden += OnSplitPreviewHidden;
            window.Focused += OnFocused;

            _windowContainer.AddChild(window);
        }

        private void PrepareTempWindow(WindowElement source)
        {
            var oldSize = source.Size;
            var newSize = Vector2.Max(source.MinSize, oldSize);

            _tempWindow.InnerElement.Size = newSize;
            _tempWindow.Pivot = source.Pivot;
            _tempWindow.Tab.CopyHeaderFrom(source.Tab);
            _tempWindow.Position = source.Position + (newSize - oldSize) * source.Pivot
                + source.CalculateTabOffset();
        }

        private void PrepareTempTab(TabElement source)
        {
            _tempTab.InnerElement.Size = source.InnerElement.Size;
            _tempTab.SetState(TabState.Focused);
            _tempTab.CopyHeaderFrom(source);
            _tempTab.Position = source.Position;
        }

        private void ShowTempWindow()
        {
            _tempWindow.IsEnabled = true;
        }

        private void HideTempWindow()
        {
            _tempWindow.IsEnabled = false;
        }

        private void ShowTempTab()
        {
            _tempTab.IsEnabled = true;
        }

        private void HideTempTab()
        {
            _tempTab.IsEnabled = false;
        }

        private void OnTabPointerDown(WindowElement window, PointerEvent pointerEvent)
        {
            PrepareTempWindow(window);
            PrepareTempTab(window.Tab);

            ShowTempTab();
        }

        private void OnTabPointerUp(WindowElement window, PointerEvent pointerEvent)
        {
            HideTempWindow();
            HideTempTab();
        }

        private void OnTabPointerDrag(WindowElement window, PointerDragEvent pointerEvent)
        {
            var deltaVector = pointerEvent.Delta.ToVector2();

            _tempWindow.Position += deltaVector;
            _tempTab.Position += deltaVector;

            if (_tempTabPlaceHolder is not null)
            {
                _tempTab.Position = _tempTabPlaceHolder.Position with { X = _tempTab.Position.X };
            }
        }

        private void OnTabPreviewShown(WindowElement sender, Element placeHolder)
        {
            _isInsertPreviewShown = true;
            _tempTabPlaceHolder = placeHolder;

            HideTempWindow();
            ShowTempTab();
        }

        private void OnTabPreviewHidden(WindowElement sender)
        {
            _isInsertPreviewShown = false;
            _tempTabPlaceHolder = null;

            HideTempTab();

            if (!_isSplitPreviewShown)
                ShowTempWindow();
        }

        private void OnSplitPreviewShown(WindowElement window)
        {
            _isSplitPreviewShown = true;

            HideTempWindow();
            HideTempTab();
        }

        private void OnSplitPreviewHidden(WindowElement window)
        {
            _isSplitPreviewShown = false;

            if (!_isInsertPreviewShown)
                ShowTempWindow();
        }

        private void OnFocused(WindowElement window)
        {
            if (_currentFocusedWindow == window)
                return;

            if (_currentFocusedWindow is not null)
            {
                if (_currentFocusedWindow.Tab.CurrentState is TabState.Focused)
                    _currentFocusedWindow.Tab.SetState(TabState.Active);
            }
            _currentFocusedWindow = window;
        }
    }
}
