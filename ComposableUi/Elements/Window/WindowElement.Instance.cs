using System;
using System.Diagnostics;

using ComposableUi.Utilities;

using Microsoft.Xna.Framework;

namespace ComposableUi
{
    using Item = WindowNodeElement<WindowContainerElement>;

    public partial class WindowElement : WindowNodeElement<WindowContainerElement>
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
        public event ElementEventHandler<WindowElement, PointerEvent> TabPointerDown;
        public event ElementEventHandler<WindowElement, PointerEvent> TabPointerUp;
        public event ElementEventHandler<WindowElement, PointerDragEvent> TabPointerDrag;
        public event ElementEventHandler<WindowElement, Element> TabPreviewShown;
        public event ElementEventHandler<WindowElement> TabPreviewHidden;
        public event ElementEventHandler<WindowElement> SplitPreviewShown;
        public event ElementEventHandler<WindowElement> SplitPreviewHidden;
        public event ElementEventHandler<WindowElement> Focused;

        // Fields.
        private readonly Element _view;

        private readonly RowLayout _tabRow;
        private readonly ContainerElement _contentContainer;

        private readonly PointerInputHandlerElement _tabPreviewInputArea;
        private readonly PointerInputHandlerElement _splitPreviewInputArea;
        private readonly PointerInputHandlerElement _innerResizeHandle;

        private bool _isTabPressed;
        private Vector2 _dragDeltaAccumulator;

        private int _currentTabIndex = EmptyTabIndex;

        private Item _currentSplitPreviewTarget;
        private Vector2 _currentSplitPreviewEdgeNormal;

        private Vector2 _resizeNormal;
        private Vector2 _resizeAxis;

        private ComposableWindowsSolver _composableWindowsSolver;

        public WindowElement(string titleText = default,
            Element content = default,
            Vector2? size = default,
            Vector2? minSize = default) 
            : base(size ?? DefaultSize,
                  minSize ?? DefaultMinSize)
        {
            var background = new ExpandedElement(
                innerElement: new SpriteElement(
                    skin: StandardSkin.WindowBody,
                    color: new Color(Color.White, 0.8f)
                )
            );

            DragHandle = new PointerInputHandlerElement(
                innerElement: new SpriteElement(skin: StandardSkin.InactiveTab)
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

        internal void AttachSolver(ComposableWindowsSolver solver)
        {
            _composableWindowsSolver = solver;
        }

        // Docking.
        internal void Dock(WindowElement source)
        {
            if (_currentSplitPreviewTarget is not null)
            {
                var edge = EdgeNormalToEdge(_currentSplitPreviewEdgeNormal);

                var shouldRemoveTargetAfterDetach = source.Container == Container
                    && _currentSplitPreviewTarget == Container
                    && Container.ItemCount <= 2;
                if (shouldRemoveTargetAfterDetach)
                {
                    for (var i = 0; i < Container.ItemCount; i++)
                    {
                        var item = Container.GetItemAt(i);
                        if (item == source)
                            continue;

                        HideSplitPreviewIfPossible();

                        _currentSplitPreviewTarget = item;
                        if (_currentSplitPreviewTarget is WindowContainerElement container)
                        {
                            _currentSplitPreviewTarget = EdgeToInsertIndex(edge) > 0
                                ? container.GetLastItem()
                                : container.GetFirstItem();
                        }

                        break;
                    }
                }

                DockTo(source, _currentSplitPreviewTarget, edge);
            }

            HideSplitPreviewIfPossible();
        }

        internal void DockAsTab(WindowElement source)
        {
            DockAsTab(source, this, _currentTabIndex);

            HideTabPreviewIfPossible();
        }

        internal void AddTab(TabElement tab)
        {
            _tabRow.AddChild(tab);
        }

        internal void InsertTab(int index, TabElement tab)
        {
            _tabRow.InsertChild(index, tab);
        }

        internal void MoveTab(int index, TabElement tab)
        {
            _tabRow.RemoveChild(tab);
            _tabRow.InsertChild(index, tab);
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

        internal void SetViewActive(bool value)
        {
            BlockInput = value;
            ViewHolder.IsEnabled = value;
        }

        internal void RestoreTab()
        {
            _tabRow.AddChild(Tab);
        }

        public void Focus()
        {
            var isTabbed = Container is not null
                && Container.DockingMode is DockingMode.Tab;
            if (isTabbed)
            {
                _tabRow.Clear();
                for (var i = 0; i < Container.ItemCount; i++)
                {
                    var item = Container.GetItemAt(i);
                    if (item is not WindowElement window)
                        continue;

                    AddTab(window.Tab);

                    if (window != this)
                    {
                        window.Tab.SetState(TabState.Inactive);
                        window.SetViewActive(false);
                    }    
                }
            }

            Tab.SetState(TabState.Focused);
            SetViewActive(true);

            Focused?.Invoke(this);
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
        private void ShowTabPreview(WindowElement source, Point position)
        {
            var isTabbed = Container is not null
                && Container.DockingMode is DockingMode.Tab;

            _tabList.Clear();
            if (isTabbed)
            {
                for (var i = 0; i < Container.ItemCount; i++)
                {
                    var item = Container.GetItemAt(i);
                    if (item is not WindowElement window)
                        continue;

                    _tabList.Add(window.Tab);
                }
            }
            else
            {
                _tabList.Add(Tab);
            }

            Element placeHolder;
            var shouldUseSourceTabAsPlaceHolder = isTabbed && Container == source.Container;
            if (shouldUseSourceTabAsPlaceHolder)
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

            if (shouldUseSourceTabAsPlaceHolder)
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
        private bool TryShowSplitPreview(WindowElement source, Point position)
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

        private bool TryDetectSplitTarget(Point position, WindowElement source,
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

        private void ShowSplitPreview(WindowElement source, Vector2 edgeNormal)
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

        internal override void PrepareReplacementWith(Item node)
        {
            base.PrepareReplacementWith(node);

            RestoreTab();
            SetViewActive(true);
        }
    }
}
