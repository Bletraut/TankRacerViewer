using System;

using ComposableUi.Utilities;

using Microsoft.Xna.Framework;

namespace ComposableUi
{
    using Item = WindowNodeElement<WindowContainerElement>;

    public partial class Window3Element : WindowNodeElement<WindowContainerElement>
    {
        public const int DefaultHeaderHeight = 30;

        private const int EmptyTabIndex = -1;

        private const float SplitPreviewSizeFactor = 0.3f;
        private const float SplitPreviewAreaSelfThicknessFactor = 0.3f;
        private const float SplitPreviewAreaParentThicknessFactor = 0.1f;
        private const int SplitPreviewAreaResizeHandleSize = 4;

        public static readonly Vector2 DefaultSize = new(250, 300);
        public static readonly Vector2 DefaultMinSize = new(80, 100);
        public static readonly Vector2 DefaultContentPadding = new(10, 10);

        // Properties.
        public Element Header { get; }
        public PointerInputHandlerElement DragHandle { get; }
        public TabElement Tab { get; }

        private bool IsResizingInternally => _resizeNormal != Vector2.Zero;

        // Events.
        public event ElementEventHandler<Window3Element, PointerEvent> TabPointerDown;
        public event ElementEventHandler<Window3Element, PointerEvent> TabPointerUp;
        public event ElementEventHandler<Window3Element, PointerDragEvent> TabPointerDrag;
        public event ElementEventHandler<Window3Element, Element> TabPreviewShown;
        public event ElementEventHandler<Window3Element> TabPreviewHidden;
        public event ElementEventHandler<Window3Element> SplitPreviewShown;
        public event ElementEventHandler<Window3Element> SplitPreviewHidden;

        // Fields.
        private readonly Element _view;

        private readonly RowLayout _tabRow;
        private readonly ContainerElement _contentContainer;

        private readonly PointerInputHandlerElement _tabPreviewInputArea;
        private readonly PointerInputHandlerElement _splitPreviewInputArea;
        private readonly PointerInputHandlerElement _innerResizeHandle;

        private bool _isTabPressed;
        private Vector2 _dragDeltaAccumulator;

        private int _currentTabIndex;

        private Item _currentSplitPreviewTarget;
        private Vector2 _currentSplitPreviewEdgeNormal;

        private Vector2 _resizeNormal;
        private Vector2 _resizeAxis;

        private ComposableWindows3Solver _composableWindowsSolver;

        public Window3Element(string titleText = default,
            Element content = default,
            Vector2? size = default,
            Vector2? minSize = default) 
            : base(size ?? DefaultSize,
                  minSize ?? DefaultMinSize)
        {
            var background = new ExpandedElement(
                innerElement: new SpriteElement(
                    skin: StandardSkin.TabBody,
                    color: new Color(Color.White, 0.8f)
                )
            );

            DragHandle = new PointerInputHandlerElement(
                innerElement: new SpriteElement(skin: StandardSkin.TabInactiveHeader)
            );
            DragHandle.PointerDown += OnDragHandlePointerDown;
            DragHandle.PointerFixedDrag += OnDragHandlePointerFixedDrag;

            _tabRow = new RowLayout(
                alignmentFactor: Alignment.TopLeft,
                expandChildrenCrossAxis: true
            );

            Tab = new TabElement(
                titleText: titleText
            );
            _tabRow.AddChild(Tab);

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
            if (content is not null)
                _contentContainer.AddChild(content);

            Header = new ClipMaskElement(
                innerElement: new ContainerElement(
                    size: new Vector2(DefaultHeaderHeight),
                    children: [
                        new ExpandedElement(DragHandle),
                        new ExpandedElement(_tabRow)
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

            _tabPreviewInputArea = new PointerInputHandlerElement(blockInput: false)
            {
                Size = new Vector2(DefaultHeaderHeight)
            };
            var insertAreaParent = new ExpandedElement(
                expandHeight: false,
                innerElement: new AlignmentElement(
                    alignmentFactor: Alignment.TopCenter,
                    pivot: Alignment.TopCenter,
                    innerElement: _tabPreviewInputArea
                )
            );
            _tabPreviewInputArea.PointerMove += OnTabPreviewInputAreaPointerMove;
            _tabPreviewInputArea.PointerLeave += OnTabPreviewInputAreaPointerLeave;

            _splitPreviewInputArea = new PointerInputHandlerElement(
                blockInput: false
            );
            var splitAreaParent = new ExpandedElement(
                topPadding: DefaultHeaderHeight,
                innerElement: _splitPreviewInputArea
            );
            _splitPreviewInputArea.PointerDown += OnSplitPreviewInputAreaPointerDown;
            _splitPreviewInputArea.PointerMove += OnSplitPreviewInputAreaPointerMove;
            _splitPreviewInputArea.PointerLeave += OnSplitPreviewInputAreaPointerLeave;

            _innerResizeHandle = new PointerInputHandlerElement(
                blockInput: false
            );
            var innerResizeHandleParent = new ExpandedElement(
                innerElement: _innerResizeHandle
            );
            _innerResizeHandle.PointerMove += OnInnerResizeHandlePointerMove;
            _innerResizeHandle.PointerLeave += OnInnerResizeHandlePointerLeave;
            _innerResizeHandle.PointerDown += OnInnerResizeHandlePointerDown;
            _innerResizeHandle.PointerUp += OnInnerResizeHandlePointerUp;
            _innerResizeHandle.PointerFixedDrag += OnInnerResizeHandlePointerFixedDrag;

            _view = new ContainerElement(
                children: [
                    background,
                    contentContainerParent,
                    headerParent,
                    innerResizeHandleParent,
                    insertAreaParent,
                    splitAreaParent
                ]
            );
            ViewHolder.InnerElement = _view;
        }

        internal void AttachSolver(ComposableWindows3Solver solver)
        {
            _composableWindowsSolver = solver;
        }

        // Docking.
        internal void Dock(Window3Element source)
        {

        }

        internal void DockAsTab(Window3Element source)
        {

        }

        internal Vector2 CalculateTabOffset()
        {
            if (Tab.Parent is not ContainerElement parent)
                return Vector2.Zero;

            if (parent.ChildCount <= 1)
                return Vector2.Zero;

            var offset = Tab.Position - parent.GetChildAt(0).Position;
            return offset;
        }

        public void BringToFront()
        {
            var root = ResolveRoot();
            if (root.Parent is ContainerElement container)
                container.BringToFront(root);
        }

        private void ResolveResizeCursor(IPointer pointer, Point position)
        {
            var cursor = PointerCursor.Arrow;

            var normal = GetResizeNormal(position);
            if (normal != Vector2.Zero)
            {
                var shouldCheckFirstChild = normal.X < 0 || normal.Y < 0;
                var splitDirection = EdgeNormalToDockingMode(normal);

                Item child = this;
                var containerWindow = Container;
                while (containerWindow is not null)
                {
                    var targetChild = shouldCheckFirstChild
                        ? containerWindow.GetFirstItem()
                        : containerWindow.GetLastItem();

                    var canResize = containerWindow.DockingMode == splitDirection
                        && child != targetChild;
                    if (canResize)
                    {
                        cursor = normal switch
                        {
                            { X: not 0 } => PointerCursor.SizeWE,
                            { Y: not 0 } => PointerCursor.SizeNS,
                            _ => PointerCursor.Arrow
                        };

                        break;
                    }

                    child = containerWindow;
                    containerWindow = child.Container;
                }
            }

            pointer.SetCursor(cursor);
        }

        private Vector2 GetResizeNormal(Point position)
        {
            var normal = _innerResizeHandle.InteractionRectangle.GetEdgeNormal(SplitPreviewAreaResizeHandleSize,
                position, RectangleUtilities.EdgeNormalResolveMode.PreferX);

            return normal;
        }

        // Tab preview.
        private void ShowTabPreview(Window3Element source, Point position)
        {
            var isTabbed = Container is not null
                && Container.DockingMode is DockingMode.Tab;

            _tabList.Clear();
            if (isTabbed)
            {
                for (var i = 0; i < Container.ItemCount; i++)
                {
                    var child = Container.GetItemAt(i);
                    if (child is not Window3Element window)
                        continue;

                    _tabList.Add(window.Tab);
                }
            }
            else
            {
                _tabList.Add(Tab);
            }

            Element placeHolder;
            var areSame = isTabbed && Container == source.Container;
            if (areSame)
            {
                placeHolder = source.Tab;
            }
            else
            {
                placeHolder = _tabPreviewPlaceHolder;
                placeHolder.Size = source.Tab.Size;

                if (_currentTabIndex == EmptyTabIndex)
                {
                    _tabList.Add(_tabPreviewPlaceHolder);
                }
                else
                {
                    _tabList.Insert(_currentTabIndex, placeHolder);
                }
            }

            var insertIndex = _tabList.Count - 1;
            var insertElement = _tabList[insertIndex];
            for (var i = 0; i < _tabList.Count; i++)
            {
                var child = _tabList[i];

                var boundingRectangle = child.BoundingRectangle;

                var isContained = position.X < boundingRectangle.X
                    || (position.X - boundingRectangle.X <= placeHolder.Size.X);
                if (isContained)
                {
                    insertIndex = i;
                    insertElement = child;
                    break;
                }
            }

            if (_currentTabIndex == insertIndex)
                return;

            _currentTabIndex = insertIndex;

            _tabRow.RemoveChild(placeHolder);
            _tabRow.InsertChild(insertIndex, placeHolder);

            if (areSame)
                Container.MoveItem(insertIndex, source);

            TabPreviewShown?.Invoke(this, Tab);
        }

        private void HideTabPreviewIfPossible()
        {
            if (_currentTabIndex == EmptyTabIndex)
                return;

            _currentTabIndex = EmptyTabIndex;

            _tabRow.RemoveChild(_tabPreviewPlaceHolder);

            TabPreviewHidden?.Invoke(this);
        }

        // Split preview.
        private bool TryShowSplitPreview(Window3Element source, Point position)
        {
            if (TryDetectSplitTarget(position, source, out var edgeNormal, out var target))
            {
                var isTargetChanged = _currentSplitPreviewTarget != target;
                var isNormalChanged = _currentSplitPreviewEdgeNormal != edgeNormal;

                var isSplitPreviewChanged = isTargetChanged || isNormalChanged;
                if (isSplitPreviewChanged)
                {
                    if (isTargetChanged)
                        HideSplitPreviewIfPossible();

                    _currentSplitPreviewTarget = target;
                    _currentSplitPreviewEdgeNormal = edgeNormal;
                    ShowSplitPreview(source, edgeNormal);

                    SplitPreviewShown?.Invoke(this);
                }

                return true;
            }

            HideSplitPreviewIfPossible();

            return false;
        }

        private bool TryDetectSplitTarget(Point position, Window3Element source,
            out Vector2 edgeNormal, out Item splitTarget)
        {
            splitTarget = default;

            var interactionRectangle = _splitPreviewInputArea.InteractionRectangle;

            var selfThickness = (_splitPreviewInputArea.Size * SplitPreviewAreaSelfThicknessFactor).ToPoint();
            edgeNormal = interactionRectangle.GetEdgeNormal(selfThickness, position,
                RectangleUtilities.EdgeNormalResolveMode.PreferX);

            if (edgeNormal == Vector2.Zero)
                return false;

            splitTarget = this;

            if (Container is not null)
            {
                var thicknessFactor = Container.DockingMode switch
                {
                    DockingMode.HorizontalSplit => Vector2.UnitY,
                    DockingMode.VerticalSplit => Vector2.UnitX,
                    _ => Vector2.Zero
                };
                var parentThickness = (_splitPreviewInputArea.Size * SplitPreviewAreaParentThicknessFactor * thicknessFactor).ToPoint();
                var parentEdgeNormal = interactionRectangle.GetEdgeNormal(parentThickness, position,
                    RectangleUtilities.EdgeNormalResolveMode.PreferX);

                if (parentEdgeNormal != Vector2.Zero)
                {
                    edgeNormal = parentEdgeNormal;
                    splitTarget = Container;
                }
            }

            var isValidSplitTarget = source != this
                || splitTarget == Container;

            return isValidSplitTarget;
        }

        private void ShowSplitPreview(Window3Element source, Vector2 edgeNormal)
        {
            _currentSplitPreviewTarget.ShowInOverlayAndAlignToEdge(_splitPreviewWindow, edgeNormal);

            _splitPreviewWindow.SetSize(Size * SplitPreviewSizeFactor);
            _splitPreviewWindow.Tab.CopyHeaderFrom(source.Tab);
        }

        private void HideSplitPreviewIfPossible()
        {
            if (_currentSplitPreviewTarget is null)
                return;

            _currentSplitPreviewTarget.HideOverlay();

            _currentSplitPreviewTarget = null;
            _currentSplitPreviewEdgeNormal = Vector2.Zero;

            SplitPreviewHidden?.Invoke(this);
        }
    }
}
