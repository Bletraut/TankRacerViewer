using System.Diagnostics;

using Microsoft.Xna.Framework;

namespace ComposableUi
{
    public sealed class Window2Layout : ContainerElement
    {
        private readonly ContainerElement _windowContainer;
        private readonly Window2Element _tempWindow;
        private readonly TabElement _tempTab;
        private readonly HolderElement _overlayWindowHolder;

        private Element _tempTabPlaceHolder;

        private bool _isInsertPreviewShown;
        private bool _isSplitPreviewShown;

        public Window2Layout()
        {
            _windowContainer = new ContainerElement();
            AddChild(new ExpandedElement(_windowContainer));

            _tempWindow = Window2Element.CreateNonInteractiveWindow();
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

        public void AddWindow(Window2Element window)
        {
            window.TabPointerDown += OnTabPointerDown;
            window.TabPointerUp += OnTabPointerUp;
            window.TabPointerDrag += OnTabPointerDrag;
            window.InsertPreviewShown += OnInsertPreviewShown;
            window.InsertPreviewHidden += OnInsertPreviewHidden;
            window.SplitPreviewShown += OnSplitPreviewShown;
            window.SplitPreviewHidden += OnSplitPreviewHidden;

            _windowContainer.AddChild(window);
        }

        private void PrepareTempWindow(Window2Element source)
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

        private void OnTabPointerDown(Window2Element window, PointerEvent pointerEvent)
        {
            PrepareTempWindow(window);
            PrepareTempTab(window.Tab);

            ShowTempTab();
        }

        private void OnTabPointerUp(Window2Element window, PointerEvent pointerEvent)
        {
            HideTempWindow();
            HideTempTab();
        }

        private void OnTabPointerDrag(Window2Element window, PointerDragEvent pointerEvent)
        {
            var deltaVector = pointerEvent.Delta.ToVector2();

            _tempWindow.Position += deltaVector;
            _tempTab.Position += deltaVector;

            if (_tempTabPlaceHolder is not null)
            {
                _tempTab.Position = _tempTabPlaceHolder.Position with { X = _tempTab.Position.X };
            }
        }

        private void OnInsertPreviewShown(Window2Element sender, Element placeHolder)
        {
            _isInsertPreviewShown = true;
            _tempTabPlaceHolder = placeHolder;

            HideTempWindow();
            ShowTempTab();
        }

        private void OnInsertPreviewHidden(Window2Element sender)
        {
            _isInsertPreviewShown = false;
            _tempTabPlaceHolder = null;

            HideTempTab();

            if (!_isSplitPreviewShown)
                ShowTempWindow();
        }

        private void OnSplitPreviewShown(Window2Element window)
        {
            _isSplitPreviewShown = true;

            HideTempWindow();
            HideTempTab();
        }

        private void OnSplitPreviewHidden(Window2Element window)
        {
            _isSplitPreviewShown = false;

            if (!_isInsertPreviewShown)
                ShowTempWindow();
        }
    }
}
