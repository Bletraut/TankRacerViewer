using System.Collections.Generic;

using Microsoft.Xna.Framework;

namespace ComposableUi
{
    using Item = WindowNodeElement<WindowContainerElement>;

    public sealed class WindowLayout : ContainerElement
    {
        private const float DefaultEmbedPreviewAreaSize = 100;
        private const int DefaultEmbedPreviewIconPadding = 20;

        private readonly Color DefaultEmbedPreviewBackgroundColor = new(Color.DarkSlateBlue, 0.8f);
        private readonly Color DefaultEmbedPreviewIconColor = new(Color.DarkBlue, 0.8f);

        public bool HadEmbeddedWindow
        {
            get
            {
                if (_embeddedLayout.ChildCount <= 0)
                    return false;

                for (var i = 0; i < _embeddedLayout.ChildCount; i++)
                {
                    var child = _embeddedLayout.GetChildAt(i);
                    if (child is Item)
                        return true;
                }

                return false;
            }
        }

        private readonly ContainerElement _embeddedLayout;
        private readonly ContainerElement _floatLayout;

        private readonly SpriteElement _embedPreviewBackground;
        private readonly SpriteElement _embedPreviewIcon;
        private readonly PointerInputHandlerElement _embedPreviewInputArea;

        private readonly WindowElement _floatPreviewWindow;
        private readonly WindowElement _embeddedPreviewWindow;
        private readonly TabElement _floatPreviewTab;

        private readonly HashSet<WindowElement> _windows = [];

        private Element _lastShownTabPreview;

        private bool _isTabPreviewShown;
        private bool _isSplitPreviewShown;
        private bool _isEmbeddedPreviewShown;

        private WindowElement _currentWindow;
        private WindowElement _currentSelectedWindow;

        public WindowLayout()
        {
            _embeddedPreviewWindow = WindowElement.CreateNonInteractiveWindow();
            _embeddedPreviewWindow.IsEnabled = false;
            AddChild(new ExpandedElement(_embeddedPreviewWindow));

            _embeddedLayout = new ContainerElement();
            AddChild(new ExpandedElement(
                propagateToInnerElementChildren: true,
                innerElement: _embeddedLayout
            ));

            _floatLayout = new ContainerElement();
            AddChild(new ExpandedElement(_floatLayout));

            _floatPreviewWindow = WindowElement.CreateNonInteractiveWindow();
            _floatPreviewWindow.IsEnabled = false;
            AddChild(_floatPreviewWindow);

            _floatPreviewTab = new TabElement
            {
                IsEnabled = false,
                IsInteractable = false,
                BlockInput = false
            };
            AddChild(_floatPreviewTab);

            _embedPreviewBackground = new SpriteElement(
                skin: StandardSkin.WhitePixel,
                color: DefaultEmbedPreviewBackgroundColor
            );
            _embedPreviewIcon = new SpriteElement(
                skin: StandardSkin.MaximizeWindowIcon,
                color: DefaultEmbedPreviewIconColor
            );
            _embedPreviewInputArea = new PointerInputHandlerElement(
                innerElement: new ContainerElement(
                    size: new Vector2(DefaultEmbedPreviewAreaSize),
                    children: [
                        new ExpandedElement(_embedPreviewBackground),
                        new ExpandedElement(
                            leftPadding: DefaultEmbedPreviewIconPadding,
                            rightPadding: DefaultEmbedPreviewIconPadding,
                            topPadding: DefaultEmbedPreviewIconPadding,
                            bottomPadding: DefaultEmbedPreviewIconPadding,
                            innerElement: _embedPreviewIcon
                        )
                    ]
                )
            );
            AddChild(_embedPreviewInputArea);
            HideEmbedPreviewArea();

            _embedPreviewInputArea.PointerEnter += OnEmbedPreviewInputAreaPointerEnter;
            _embedPreviewInputArea.PointerLeave += OnEmbedPreviewInputAreaPointerLeave;
        }

        public void AddFloatWindow(WindowElement window) 
            => AddWindow(window, _floatLayout);

        public void EmbedWindow(WindowElement window)
        {
            if (HadEmbeddedWindow)
                return;

            AddWindow(window, _embeddedLayout);
        }

        public void RemoveWindow(WindowElement window)
        {
            if (_windows.Remove(window))
            {
                window.TabPointerDown -= OnTabPointerDown;
                window.TabPointerUp -= OnTabPointerUp;
                window.TabPointerDrag -= OnTabPointerDrag;
                window.TabPreviewShown -= OnTabPreviewShown;
                window.TabPreviewHidden -= OnTabPreviewHidden;
                window.SplitPreviewShown -= OnSplitPreviewShown;
                window.SplitPreviewHidden -= OnSplitPreviewHidden;
                window.MovedByTab -= OnMovedByTab;
                window.Undocked -= OnUndocked;
                window.Selected -= OnSelected;
                window.Closed -= OnClosed;

                WindowElement.Undock(window, Vector2.Zero);
                window.Parent?.RemoveChild(window);
            }
        }

        private void AddWindow(WindowElement window, ContainerElement layout)
        {
            RemoveWindow(window);

            if (_windows.Add(window))
            {
                window.TabPointerDown += OnTabPointerDown;
                window.TabPointerUp += OnTabPointerUp;
                window.TabPointerDrag += OnTabPointerDrag;
                window.TabPreviewShown += OnTabPreviewShown;
                window.TabPreviewHidden += OnTabPreviewHidden;
                window.SplitPreviewShown += OnSplitPreviewShown;
                window.SplitPreviewHidden += OnSplitPreviewHidden;
                window.MovedByTab += OnMovedByTab;
                window.Undocked += OnUndocked;
                window.Selected += OnSelected;
                window.Closed += OnClosed;

                layout.AddChild(window);
            }
        }

        private void PrepareFloatPreviewWindow(WindowElement source)
        {
            var oldSize = source.Size;
            var newSize = Vector2.Max(source.MinSize, oldSize);

            _floatPreviewWindow.SetSize(newSize);
            _floatPreviewWindow.Pivot = source.Pivot;
            _floatPreviewWindow.Tab.CopyHeaderFrom(source.Tab);
            _floatPreviewWindow.Position = source.Position + (newSize - oldSize) * source.Pivot
                + source.CalculateTabOffset();
        }

        private void PrepareEmbeddedPreviewWindow(WindowElement source)
        {
            _embeddedPreviewWindow.Tab.CopyHeaderFrom(source.Tab);
        }

        private void PrepareFloatPreviewTab(TabElement source)
        {
            _floatPreviewTab.InnerElement.Size = source.InnerElement.Size;
            _floatPreviewTab.SetState(TabState.Selected);
            _floatPreviewTab.CopyHeaderFrom(source);
            _floatPreviewTab.Position = source.Position;
        }

        private void ShowFloatPreviewWindow()
        {
            _floatPreviewWindow.IsEnabled = true;
        }

        private void HideFloatPreviewWindow()
        {
            _floatPreviewWindow.IsEnabled = false;
        }

        private void ShowEmbeddedPreviewWindow()
        {
            _embeddedPreviewWindow.IsEnabled = true;
        }

        private void HideEmbeddedPreviewWindow()
        {
            _embeddedPreviewWindow.IsEnabled = false;
        }

        private void ShowFloatPreviewTab()
        {
            _floatPreviewTab.IsEnabled = true;
        }

        private void HideFloatPreviewTab()
        {
            _floatPreviewTab.IsEnabled = false;
        }

        private void RefreshFloatPreviewTabPosition()
        {
            if (_lastShownTabPreview is null)
                return;

            _floatPreviewTab.Position = _lastShownTabPreview.Position with { X = _floatPreviewTab.Position.X };
        }

        private void ShowEmbedPreviewAreaIfPossible()
        {
            if (_embedPreviewInputArea.IsEnabled)
                return;

            if (HadEmbeddedWindow)
                return;

            _embedPreviewInputArea.IsEnabled = true;
        }

        private void HideEmbedPreviewArea()
        {
            _isEmbeddedPreviewShown = false;
            _embedPreviewInputArea.IsEnabled = false;
        }

        private void MoveToFloatLayoutIfPossible(WindowElement window)
        {
            if (window.Parent == _embeddedLayout)
                _floatLayout.AddChild(window);
        }

        private void OnTabPointerDown(WindowElement window, PointerEvent pointerEvent)
        {
            _currentWindow = window;

            PrepareFloatPreviewWindow(_currentWindow);
            PrepareEmbeddedPreviewWindow(_currentWindow);
            PrepareFloatPreviewTab(_currentWindow.Tab);

            ShowFloatPreviewTab();
        }

        private void OnTabPointerUp(WindowElement window, PointerEvent pointerEvent)
        {
            HideFloatPreviewWindow();
            HideEmbeddedPreviewWindow();
            HideFloatPreviewTab();

            if (_isEmbeddedPreviewShown)
                EmbedWindow(_currentWindow);
            HideEmbedPreviewArea();

            _currentWindow = null;
        }

        private void OnTabPointerDrag(WindowElement window, PointerDragEvent pointerEvent)
        {
            var deltaVector = pointerEvent.Delta.ToVector2();

            _floatPreviewWindow.Position += deltaVector;
            _floatPreviewTab.Position += deltaVector;

            RefreshFloatPreviewTabPosition();

            ShowEmbedPreviewAreaIfPossible();
        }

        private void OnTabPreviewShown(WindowElement window)
        {
            _isTabPreviewShown = true;
            _lastShownTabPreview = window.Tab;

            HideFloatPreviewWindow();
            HideEmbeddedPreviewWindow();

            RefreshFloatPreviewTabPosition();
            ShowFloatPreviewTab();
        }

        private void OnTabPreviewHidden(WindowElement window)
        {
            if (_lastShownTabPreview != window.Tab)
                return;

            _isTabPreviewShown = false;
            _lastShownTabPreview = null;

            HideFloatPreviewTab();

            var canShowFloatPreviewWindow = !_isSplitPreviewShown
                && !_isEmbeddedPreviewShown;
            if (canShowFloatPreviewWindow)
                ShowFloatPreviewWindow();
        }

        private void OnSplitPreviewShown(WindowElement window)
        {
            _isSplitPreviewShown = true;

            HideFloatPreviewWindow();
            HideEmbeddedPreviewWindow();
            HideFloatPreviewTab();
        }

        private void OnSplitPreviewHidden(WindowElement window)
        {
            _isSplitPreviewShown = false;

            var canShowFloatPreviewWindow = !_isTabPreviewShown
                && !_isEmbeddedPreviewShown;
            if (canShowFloatPreviewWindow)
                ShowFloatPreviewWindow();
        }

        private void OnSelected(WindowElement window)
        {
            if (_currentSelectedWindow == window)
                return;

            if (_currentSelectedWindow is not null)
            {
                if (_currentSelectedWindow.Tab.CurrentState is TabState.Selected)
                    _currentSelectedWindow.Tab.SetState(TabState.Normal);
            }
            _currentSelectedWindow = window;
        }

        private void OnClosed(WindowElement window)
        {
            RemoveWindow(window);
        }

        private void OnMovedByTab(WindowElement window)
        {
            MoveToFloatLayoutIfPossible(window);
        }

        private void OnUndocked(WindowElement window)
        {
            MoveToFloatLayoutIfPossible(window);
        }

        private void OnEmbedPreviewInputAreaPointerEnter(PointerInputHandlerElement sender,
            PointerEvent pointerEvent)
        {
            _isEmbeddedPreviewShown = true;

            HideFloatPreviewWindow();
            HideFloatPreviewTab();
            ShowEmbeddedPreviewWindow();
        }

        private void OnEmbedPreviewInputAreaPointerLeave(PointerInputHandlerElement sender,
            PointerEvent pointerEvent)
        {
            _isEmbeddedPreviewShown = false;

            HideEmbeddedPreviewWindow();

            var canShowFloatPreviewWindow = _embedPreviewInputArea.IsEnabled
                && !_isTabPreviewShown
                && !_isEmbeddedPreviewShown;
            if (canShowFloatPreviewWindow)
                ShowFloatPreviewWindow();
        }
    }
}
