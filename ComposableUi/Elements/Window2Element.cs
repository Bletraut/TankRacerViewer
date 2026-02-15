using System.Collections.Generic;
using System.Diagnostics;

using ComposableUi.Utilities;

using Microsoft.Xna.Framework;

namespace ComposableUi
{
    public class Window2Element : ResizableElement
    {
        public const int DefaultHeaderHeight = 30;

        public static readonly Vector2 DefaultSize = new(250, 300);
        public static readonly Vector2 DefaultMinSize = new(80, 100);
        public static readonly Vector2 DefaultContentPadding = new(10, 10);

        // Static.
        private static readonly Stack<Window2Element> _windowPool = new();

        private static readonly Window2Element _splitPreviewWindow = CreateNonInteractiveWindow();

        internal static LineLayout GetRow()
        {
            var row = new RowLayout(
                expandChildrenMainAxis: true,
                expandChildrenCrossAxis: true
            );

            return row;
        }

        internal static LineLayout GetColumn()
        {
            var column = new ColumnLayout(
                expandChildrenMainAxis: true,
                expandChildrenCrossAxis: true
            );

            return column;
        }

        internal static Window2Element GetWindow()
        {
            if (!_windowPool.TryPop(out var window))
                window = new Window2Element();
            window.IsEnabled = true;

            return window;
        }

        internal static void ReleaseWindow(Window2Element window)
        {
            window.IsEnabled = false;
            _windowPool.Push(window);
        }

        internal static Window2Element CreateNonInteractiveWindow()
        {
            var window =  new Window2Element()
            {
                IsInteractable = false,
                BlockInput = false
            };
            window.DragHandle.IsInteractable = false;
            window.DragHandle.BlockInput = false;
            window.Tab.IsInteractable = false;
            window.Tab.BlockInput = false;
            window._splitArea.IsEnabled = false;

            return window;
        }

        // Class.
        public Element Header { get; }
        public PointerInputHandlerElement DragHandle { get; }
        public TabElement Tab { get; }

        public event ElementEventHandler<Window2Element, Point> TabPointerDown;
        public event ElementEventHandler<Window2Element, Point> TabPointerUp;
        public event ElementEventHandler<Window2Element, Point> TabPointerDrag;
        public event ElementEventHandler<Window2Element> SplitPreviewShown;
        public event ElementEventHandler<Window2Element> SplitPreviewHidden;

        private readonly ExpandedElement _view;
        private readonly ExpandedElement _splitLayoutExpanded;

        private readonly RowLayout _tabsRow;
        private readonly ContainerElement _contentContainer;

        private readonly PointerInputHandlerElement _splitArea;

        private readonly ExpandedElement _splitPreviewExpanded;
        private readonly AlignmentElement _splitPreviewAlignment;

        private Window2Element _parentWindow;
        private Window2Element _rootWindow;

        private Vector2 _dragDeltaAccumulator;
        private Vector2 _currentSplitPreviewEdgeNormal;

        private ComposableWindowsSolver _composableWindowsSolver;

        public Window2Element(string titleText = default,
            Element content = default,
            Vector2? size = default,
            Vector2? minSize = default)
        {
            MinSize = minSize ?? DefaultMinSize;

            var background = new ExpandedElement(
                innerElement: new SpriteElement(
                    skin: StandardSkin.TabBody,
                    color: new Color(Color.White, 0.8f)
                )
            );

            DragHandle = new PointerInputHandlerElement(
                innerElement: new SpriteElement(skin: StandardSkin.TabInactiveHeader)
            );
            DragHandle.PointerFixedDrag += OnDragHandlePointerFixedDrag;

            _tabsRow = new RowLayout(
                alignmentFactor: Alignment.TopLeft,
                expandChildrenCrossAxis: true
            );

            Tab = new TabElement(
                titleText: titleText
            );
            _tabsRow.AddChild(Tab);

            Tab.PointerDown += OnTabButtonPointerDown;
            Tab.PointerUp += OnTabButtonPointerUp;
            Tab.PointerDrag += OnTabButtonPointerDrag;

            _contentContainer = new ContainerElement();
            var contentContainerParent = new ExpandedElement(
                leftPadding: DefaultContentPadding.X,
                rightPadding: DefaultContentPadding.X,
                topPadding: DefaultContentPadding.Y + DefaultHeaderHeight,
                bottomPadding: DefaultContentPadding.Y,
                innerElement: _contentContainer
            );

            Header = new ClipMaskElement(
                innerElement: new ContainerElement(
                    size: new Vector2(DefaultHeaderHeight),
                    children: [
                        new ExpandedElement(DragHandle),
                        new ExpandedElement(_tabsRow)
                    ]
                )
            );
            var headerParent = new ExpandedElement(
                expandHeight: false,
                innerElement: new AlignmentElement(
                    alignmentFactor: Alignment.TopCenter,
                    pivot: Alignment.TopCenter,
                    innerElement: Header
                )
            );

            _splitPreviewAlignment = new AlignmentElement();
            _splitPreviewExpanded = new ExpandedElement(_splitPreviewAlignment)
            {
                IsEnabled = false
            };

            _splitArea = new PointerInputHandlerElement(
                blockInput: false
            );
            var splitAreaParent = new ExpandedElement(
                topPadding: DefaultHeaderHeight,
                innerElement: _splitArea
            );
            _splitArea.PointerMove += OnSplitAreaPointerMove;
            _splitArea.PointerLeave += OnSplitAreaPointerLeave;

            _view = new ExpandedElement(
                new ContainerElement(
                    children: [
                        background,
                        contentContainerParent,
                        headerParent,
                        _splitPreviewExpanded,
                        splitAreaParent,
                    ]
                )
            );

            _splitLayoutExpanded = new ExpandedElement
            {
                IsEnabled = false
            };

            InnerElement = new ContainerElement(
                size: size ?? DefaultSize,
                children: [_view, _splitLayoutExpanded]
            );
        }

        internal void Attach(Window2Element window)
        {
            _view.IsEnabled = false;

            _splitLayoutExpanded.IsEnabled = true;

            var splitLayout = _currentSplitPreviewEdgeNormal.X != 0
                ? GetRow()
                : GetColumn();
            _splitLayoutExpanded.InnerElement = splitLayout;

            var newWindow = GetWindow();
            newWindow._rootWindow = _rootWindow ?? this;
            newWindow.IsInteractable = false;
            newWindow.BlockInput = false;
            newWindow.Tab.CopyHeaderFrom(Tab);
            splitLayout.AddChild(newWindow);

            window._rootWindow = _rootWindow ?? this;
            window.IsInteractable = false;
            window.BlockInput = false;
            splitLayout.AddChild(window);
        }

        internal void AttachSolver(ComposableWindowsSolver solver)
        {
            _composableWindowsSolver = solver;
        }

        private bool TryShowSplitPreview(Window2Element window, Point position)
        {
            var thickness = (_splitArea.Size * 0.3f).ToPoint();
            var normal = _splitArea.InteractionRectangle.GetEdgeNormal(thickness, position);
            normal.Y = normal.X != 0 ? 0 : normal.Y;

            if (normal == Vector2.Zero)
            {
                HideSplitPreviewIfPossible();
                return false;
            }

            if (_currentSplitPreviewEdgeNormal == normal)
                return true;

            _currentSplitPreviewEdgeNormal = normal;
            ShowSplitPreview(window, _currentSplitPreviewEdgeNormal);

            SplitPreviewShown?.Invoke(this);

            return true;
        }

        private void ShowSplitPreview(Window2Element window, Vector2 edgeNormal)
        {
            var alignmentFactor = Vector2.Max(Vector2.Zero, edgeNormal);

            _splitPreviewExpanded.IsEnabled = true;
            _splitPreviewExpanded.ExpandWidth = edgeNormal.Y != 0;
            _splitPreviewExpanded.ExpandHeight = edgeNormal.X != 0;

            _splitPreviewAlignment.InnerElement = _splitPreviewWindow;
            _splitPreviewAlignment.AlignmentFactor = alignmentFactor;
            _splitPreviewAlignment.Pivot = alignmentFactor;

            _splitPreviewWindow.InnerElement.Size = Size * 0.3f;
            _splitPreviewWindow.Tab.CopyHeaderFrom(window.Tab);
        }

        private void HideSplitPreviewIfPossible()
        {
            if (_currentSplitPreviewEdgeNormal == Vector2.Zero)
                return;

            _currentSplitPreviewEdgeNormal = Vector2.Zero;

            _splitPreviewExpanded.IsEnabled = false;

            SplitPreviewHidden?.Invoke(this);
        }

        private void OnDragHandlePointerFixedDrag(PointerInputHandlerElement sender,
            (Point Position, Point Delta) arguments)
        {
            var targetWindow = _rootWindow ?? this;
            targetWindow.Position += arguments.Delta.ToVector2();
        }

        private void OnTabButtonPointerDown(PointerInputHandlerElement sender, Point position)
        {
            _dragDeltaAccumulator = Vector2.Zero;

            _composableWindowsSolver?.SelectSource(this);

            TabPointerDown?.Invoke(this, position);
        }

        private void OnTabButtonPointerUp(PointerInputHandlerElement sender, Point position)
        {
            if (_composableWindowsSolver is not null)
            {
                if (!_composableWindowsSolver.TryAttach())
                    Position += _dragDeltaAccumulator;
            }

            TabPointerUp?.Invoke(this, position);
        }

        private void OnTabButtonPointerDrag(PointerInputHandlerElement sender,
            (Point Position, Point Delta) arguments)
        {
            _dragDeltaAccumulator += arguments.Delta.ToVector2();

            TabPointerDrag?.Invoke(this, arguments.Delta);
        }

        private void OnSplitAreaPointerMove(PointerInputHandlerElement sender, Point position)
        {
            if (_composableWindowsSolver is null)
                return;

            var sourceWindow = _composableWindowsSolver.Source;
            if (sourceWindow is null)
                return;

            if (sourceWindow == this)
                return;

            if (TryShowSplitPreview(sourceWindow, position))
            {
                _composableWindowsSolver.SelectTarget(this);
            }
            else
            {
                _composableWindowsSolver.ReleaseTarget(this);
            }
        }

        private void OnSplitAreaPointerLeave(PointerInputHandlerElement sender, Point position)
        {
            _composableWindowsSolver?.ReleaseTarget(this);
            HideSplitPreviewIfPossible();
        }

        private enum SplitDirection
        {
            None,

            Horizontal,
            Vertical
        }
    }
}
