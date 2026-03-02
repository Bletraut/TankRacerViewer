using System;
using System.Collections.Generic;

using ComposableUi.Utilities;

using Microsoft.Xna.Framework;

namespace ComposableUi
{
    public class Window2Element : ResizableElement
    {
        public const int DefaultHeaderHeight = 30;

        private const int EmptyInsertIndex = -1;

        private const float SplitSizeFactor = 0.3f;
        private const float SplitAreaSelfThicknessFactor = 0.3f;
        private const float SplitAreaParentThicknessFactor = 0.1f;
        private const int SplitAreaResizeHandleSize = 4;

        public static readonly Vector2 DefaultSize = new(250, 300);
        public static readonly Vector2 DefaultMinSize = new(80, 100);
        public static readonly Vector2 DefaultContentPadding = new(10, 10);

        // Static.
        private static readonly Stack<Window2Element> _windowPool = new();

        private static readonly Element _insertPreviewPlaceHolder = new();
        private static readonly Window2Element _splitPreviewWindow = CreateNonInteractiveWindow();

        private static readonly List<Element> _tabList = [];

        internal static Window2Element CreateNonInteractiveWindow()
        {
            var window = new Window2Element()
            {
                IsInteractable = false,
                BlockInput = false
            };
            window.DragHandle.IsInteractable = false;
            window.DragHandle.BlockInput = false;
            window.Tab.IsInteractable = false;
            window.Tab.BlockInput = false;
            window._insertArea.IsEnabled = false;
            window._splitArea.IsEnabled = false;

            return window;
        }

        internal static LineLayout GetRow()
            => ApplyLineLayoutParameters(new RowLayout());

        internal static LineLayout GetColumn()
            => ApplyLineLayoutParameters(new ColumnLayout());

        internal static LineLayout ApplyLineLayoutParameters(LineLayout layout)
        {
            layout.ExpandChildrenCrossAxis = true;
            layout.ExpandChildrenMainAxis = true;
            layout.MainAxisChildrenExpandingMode = ExpandingMode.Size;

            layout.AddChild(new LayoutElement(
                ignoreLayout: true,
                innerElement: new ExpandedElement(
                    innerElement: new SpriteElement(
                        skin: StandardSkin.SolidDarkPixel
                    )
                )
            ));

            return layout;
        }

        internal static Window2Element GetContainerWindow()
        {
            var window = GetWindow();
            window._viewHolder.InnerElement = null;
            window._splitArea.IsEnabled = false;

            return window;
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

        public static void AttachTo(Window2Element source, Window2Element target, Edge edge)
        {
            DetachFromParent(source, Vector2.Zero);

            var splitDirection = EdgeToSplitDirection(edge);

            var shouldAttachToParent = target._containerWindow?._splitDirection == splitDirection;
            if (shouldAttachToParent)
            {
                if (TryAttachToParent(source, target, edge))
                    return;
            }

            var insertIndex = EdgeToInsertIndex(edge);
            var splitLayout = splitDirection is SplitDirection.Horizontal 
                ? GetRow() 
                : GetColumn();

            var isTabbed = target._containerWindow is not null
                && target._containerWindow._compositionType is CompositionType.Tabbed;
            if (isTabbed)
                target = target._containerWindow;

            var containerWindow = GetContainerWindow();
            containerWindow._compositionType = CompositionType.Adjacent;
            containerWindow._splitDirection = splitDirection;
            containerWindow._viewHolder.PropagateToInnerElementChildren = false;
            containerWindow._viewHolder.InnerElement = splitLayout;
            containerWindow.SetSize(target.Size);
            containerWindow._containerWindow = target._containerWindow;
            containerWindow.IsInteractable = target.IsInteractable;
            containerWindow.LocalPosition = target.LocalPosition 
                - target.PivotOffset + containerWindow.PivotOffset;

            // Replaces the target window with the container window.
            if (target._containerWindow is not null)
            {
                if (target._containerWindow._viewHolder.InnerElement is ContainerElement parentSplitLayout)
                {
                    var index = parentSplitLayout.IndexOf(target);
                    parentSplitLayout.InsertChild(index, containerWindow);
                }

                var targetIndex = target._containerWindow._childWindows.IndexOf(target);
                target._containerWindow._childWindows.Remove(target);
                target._containerWindow._childWindows.Insert(targetIndex, containerWindow);
            }
            else
            {
                target.Parent.AddChild(containerWindow);
            }

            var sourceSize = containerWindow.Size * SplitSizeFactor;
            var targetSize = containerWindow.Size - sourceSize;
            containerWindow._childWindows ??= [];

            target._containerWindow = containerWindow;
            target.IsInteractable = false;
            target.SetSize(targetSize);
            splitLayout.AddChild(target);
            containerWindow._childWindows.Add(target);

            source._containerWindow = containerWindow;
            source.IsInteractable = false;
            source.SetSize(sourceSize);
            splitLayout.InsertChild(splitLayout.IndexOf(target) + insertIndex, source);
            containerWindow._childWindows.Insert(insertIndex, source);

            containerWindow.ApplyRootWindow(target._rootWindow);
            containerWindow.RecalculateContainerMinSize();
        }

        public static void InsertTo(Window2Element source, Window2Element target, int index)
        {
            var areSame = target._containerWindow is not null
                && target._containerWindow._compositionType == CompositionType.Tabbed
                && source._containerWindow == target._containerWindow;
            if (areSame)
            {
                target._containerWindow._childWindows.Remove(source);
                target._containerWindow._childWindows.Insert(index, source);

                target._tabRow.RemoveChild(source.Tab);
                target._tabRow.InsertChild(index, source.Tab);

                return;
            }

            if (source == target)
                return;

            DetachFromParent(source, Vector2.Zero);

            var shouldInsertToParent = target._containerWindow?._compositionType is CompositionType.Tabbed;
            if (shouldInsertToParent)
            {
                if (TryInsertToParent(source, target))
                    return;
            }

            var layout = new ContainerElement();

            var containerWindow = GetContainerWindow();
            containerWindow._compositionType = CompositionType.Tabbed;
            containerWindow._splitDirection = SplitDirection.None;
            containerWindow._viewHolder.PropagateToInnerElementChildren = true;
            containerWindow._viewHolder.InnerElement = layout;
            containerWindow.SetSize(target.Size);
            containerWindow._containerWindow = target._containerWindow;
            containerWindow.IsInteractable = target.IsInteractable;
            containerWindow.LocalPosition = target.LocalPosition
                - target.PivotOffset + containerWindow.PivotOffset;

            // Replaces the target window with the container window.
            if (target._containerWindow is not null)
            {
                if (target._containerWindow._viewHolder.InnerElement is ContainerElement parentSplitLayout)
                {
                    var viewIndex = parentSplitLayout.IndexOf(target);
                    parentSplitLayout.InsertChild(viewIndex, containerWindow);
                }

                var targetIndex = target._containerWindow._childWindows.IndexOf(target);
                target._containerWindow._childWindows.Remove(target);
                target._containerWindow._childWindows.Insert(targetIndex, containerWindow);
            }
            else
            {
                target.Parent.AddChild(containerWindow);
            }

            containerWindow._childWindows ??= [];

            target._containerWindow = containerWindow;
            target.IsInteractable = false;
            layout.AddChild(target);
            containerWindow._childWindows.Add(target);

            source._containerWindow = containerWindow;
            source.IsInteractable = false;
            source.BlockInput = false;
            source._viewHolder.IsEnabled = false;
            source.SetSize(target.Size);
            layout.AddChild(source);
            target._tabRow.InsertChild(index, source.Tab);
            containerWindow._childWindows.Insert(index, source);

            containerWindow.ApplyRootWindow(target._rootWindow);
            containerWindow.RecalculateContainerMinSize();
        }

        public static void DetachFromParent(Window2Element source, Vector2 position)
        {
            var containerWindow = source._containerWindow;
            if (containerWindow is null)
                return;

            var parent = containerWindow._rootWindow?.Parent ?? containerWindow.Parent;
            parent.AddChild(source);
            containerWindow._childWindows.Remove(source);
            source._containerWindow = null;
            source.ApplyRootWindow(null);
            source.IsInteractable = true;
            source.Position = position;
            // For inserted tab.
            source.BlockInput = true;
            source._viewHolder.IsEnabled = true;
            source._tabRow.AddChild(source.Tab);

            if (containerWindow._compositionType == CompositionType.Tabbed)
            {
                Window2Element firstWindow = null;
                foreach ( var child in containerWindow._childWindows)
                {
                    if (child != source)
                    {
                        firstWindow = child;
                        break;
                    }
                }
                firstWindow.BlockInput = true;
                firstWindow._viewHolder.IsEnabled = true;
                firstWindow._tabRow.AddChild(firstWindow.Tab);

                foreach ( var child in containerWindow._childWindows)
                {
                    if (child == firstWindow)
                        continue;

                    if (child == source)
                        continue;

                    child.BlockInput = false;
                    child._viewHolder.IsEnabled = false;
                    firstWindow._tabRow.AddChild(child.Tab);
                }
            }    

            if (containerWindow._childWindows.Count == 1)
            {
                var lastWindow = containerWindow._childWindows[0];
                containerWindow._childWindows.Clear();

                lastWindow.SetSize(containerWindow.Size);
                lastWindow._containerWindow = containerWindow._containerWindow;
                lastWindow.ApplyRootWindow(containerWindow._rootWindow);
                lastWindow.IsInteractable = containerWindow.IsInteractable;
                // For inserted tab.
                lastWindow.BlockInput = true;
                lastWindow._viewHolder.IsEnabled = true;
                lastWindow._tabRow.AddChild(lastWindow.Tab);

                if (containerWindow._containerWindow is not null)
                {
                    var parentContainerWindow = containerWindow._containerWindow;

                    if (parentContainerWindow._viewHolder.InnerElement is ContainerElement parentSplitLayout)
                    {
                        var index = parentSplitLayout.IndexOf(containerWindow);
                        parentSplitLayout.InsertChild(index, lastWindow);
                    }

                    var containerIndex = parentContainerWindow._childWindows.IndexOf(containerWindow);
                    parentContainerWindow._childWindows.Remove(containerWindow);
                    parentContainerWindow._childWindows.Insert(containerIndex, lastWindow);
                    parentContainerWindow.RecalculateContainerMinSize();
                }
                else
                {
                    var localPosition = containerWindow.LocalPosition;
                    containerWindow.Parent.AddChild(lastWindow);
                    lastWindow.LocalPosition = localPosition - containerWindow.PivotOffset + lastWindow.PivotOffset;
                }

                containerWindow._compositionType = CompositionType.None;
                containerWindow._splitDirection = SplitDirection.None;
                containerWindow._containerWindow = null;
                containerWindow.ApplyRootWindow(null);
                containerWindow._viewHolder.InnerElement = null;
                containerWindow.Parent?.RemoveChild(containerWindow);

                ReleaseWindow(containerWindow);
            }
            else
            {
                containerWindow.RecalculateContainerMinSize();
            }
        }

        private static bool TryAttachToParent(Window2Element source, Window2Element target, Edge edge)
        {
            if (target._containerWindow._viewHolder.InnerElement is not ContainerElement splitLayout)
                return false;

            var insertIndex = EdgeToInsertIndex(edge);
            var targetIndex = splitLayout.IndexOf(target);

            var sourceSize = target.Size * SplitSizeFactor;
            var targetSize = target.Size - sourceSize;

            target.SetSize(targetSize);

            source._containerWindow = target._containerWindow;
            source.ApplyRootWindow(target._rootWindow);
            source.IsInteractable = false;
            source.SetSize(sourceSize);
            splitLayout.InsertChild(targetIndex + insertIndex, source);
            targetIndex = target._containerWindow._childWindows.IndexOf(target);
            target._containerWindow._childWindows.Insert(targetIndex + insertIndex, source);

            target._containerWindow.RecalculateContainerMinSize();

            return true;
        }

        private static bool TryInsertToParent(Window2Element source, Window2Element target)
        {
            if (target._containerWindow._viewHolder.InnerElement is not ContainerElement splitLayout)
                return false;

            source._containerWindow = target._containerWindow;
            source.ApplyRootWindow(target._rootWindow);
            source.IsInteractable = false;
            source.BlockInput = false;
            source._viewHolder.IsEnabled = false;
            source.SetSize(target.Size);
            splitLayout.AddChild(source);
            target._tabRow.InsertChild(target._currentInsertIndex, source.Tab);
            target._containerWindow._childWindows.Insert(target._currentInsertIndex, source);

            target._containerWindow.RecalculateContainerMinSize();

            return true;
        }

        private static void IncreaseSizeIfPossible(Window2Element target, Vector2 axis,
            float delta, bool expandForward)
        {
            var increaseDelta = 0f;
            var decreaseDelta = delta;

            var childWindows = target._containerWindow._childWindows;
            var targetIndex = childWindows.IndexOf(target);

            var neighborIndex = targetIndex + (expandForward ? 1 : -1);
            while (neighborIndex >= 0 && neighborIndex < childWindows.Count)
            {
                var neighbor = childWindows[neighborIndex];
                neighborIndex += expandForward ? 1 : -1;

                var size = neighbor.Size;

                var axisSize = Vector2.Dot(axis, size);
                var minAxisSize = Vector2.Dot(axis, neighbor.MinSize);
                if (axisSize <= minAxisSize)
                    continue;

                var newAxisSize = MathF.Max(minAxisSize, axisSize - decreaseDelta);
                var newSize = size * (Vector2.One - axis) + new Vector2(newAxisSize) * axis;
                neighbor.SetSize(newSize);

                var sizeDelta = axisSize - newAxisSize;
                increaseDelta += sizeDelta;
                decreaseDelta -= sizeDelta;

                if (decreaseDelta <= 0)
                    break;
            }

            target.SetSize(target.Size + new Vector2(increaseDelta));
        }

        private static int EdgeToInsertIndex(Edge edge)
            => edge is Edge.Left or Edge.Top ? 0 : 1;

        private static SplitDirection EdgeToSplitDirection(Edge edge)
            => edge is Edge.Left or Edge.Right ? SplitDirection.Horizontal : SplitDirection.Vertical;

        private static Edge EdgeNormalToEdge(Vector2 edgeNormal)
        {
            var edge = edgeNormal switch
            {
                { X: < 0 } => Edge.Left,
                { X: > 0 } => Edge.Right,
                { Y: < 0 } => Edge.Top,
                { Y: > 0 } => Edge.Bottom,
                _ => throw new NotImplementedException(),
            };

            return edge;
        }

        private static SplitDirection EdgeNormalToSplitDirection(Vector2 edgeNormal)
        {
            var edge = EdgeNormalToEdge(edgeNormal);
            return EdgeToSplitDirection(edge);
        }

        // Class.
        public Element Header { get; }
        public PointerInputHandlerElement DragHandle { get; }
        public TabElement Tab { get; }

        private bool IsResizingInternally => _resizeNormal != Vector2.Zero;

        public event ElementEventHandler<Window2Element, PointerEvent> TabPointerDown;
        public event ElementEventHandler<Window2Element, PointerEvent> TabPointerUp;
        public event ElementEventHandler<Window2Element, PointerDragEvent> TabPointerDrag;
        public event ElementEventHandler<Window2Element, Element> InsertPreviewShown;
        public event ElementEventHandler<Window2Element> InsertPreviewHidden;
        public event ElementEventHandler<Window2Element> SplitPreviewShown;
        public event ElementEventHandler<Window2Element> SplitPreviewHidden;

        private readonly Element _view;
        private readonly ExpandedElement _viewHolder;

        private readonly RowLayout _tabRow;
        private readonly ContainerElement _contentContainer;

        private readonly PointerInputHandlerElement _insertArea;
        private readonly PointerInputHandlerElement _splitArea;
        private readonly PointerInputHandlerElement _innerResizeHandle;

        private readonly ExpandedElement _splitPreviewExpanded;
        private readonly AlignmentElement _splitPreviewAlignment;

        private List<Window2Element> _childWindows;

        private Window2Element _containerWindow;
        private Window2Element _rootWindow;

        private Vector2 _dragDeltaAccumulator;

        private int _currentInsertIndex = EmptyInsertIndex;

        private Window2Element _currentSplitPreviewTarget;
        private Vector2 _currentSplitPreviewEdgeNormal;

        private CompositionType _compositionType;
        private SplitDirection _splitDirection;

        private Vector2 _resizeNormal;
        private Vector2 _resizeAxis;

        private bool _isTapPressed;

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

            _splitPreviewAlignment = new AlignmentElement();
            _splitPreviewExpanded = new ExpandedElement(_splitPreviewAlignment)
            {
                IsEnabled = false
            };

            _insertArea = new PointerInputHandlerElement(blockInput: false)
            {
                Size = new Vector2(DefaultHeaderHeight)
            };
            var insertAreaParent = new ExpandedElement(
                expandHeight: false,
                innerElement: new AlignmentElement(
                    alignmentFactor: Alignment.TopCenter,
                    pivot: Alignment.TopCenter,
                    innerElement: _insertArea
                )
            );
            _insertArea.PointerMove += OnInsertAreaPointerMove;
            _insertArea.PointerLeave += OnInsertAreaPointerLeave;

            _splitArea = new PointerInputHandlerElement(
                blockInput: false
            );
            var splitAreaParent = new ExpandedElement(
                topPadding: DefaultHeaderHeight,
                innerElement: _splitArea
            );
            _splitArea.PointerDown += OnSplitAreaPointerDown;
            _splitArea.PointerMove += OnSplitAreaPointerMove;
            _splitArea.PointerLeave += OnSplitAreaPointerLeave;

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
            _viewHolder = new ExpandedElement(_view);

            InnerElement = new ContainerElement(
                size: size ?? DefaultSize,
                children: [
                    _viewHolder,
                    _splitPreviewExpanded
                ]
            );
        }

        internal void Insert(Window2Element source)
        {
            InsertTo(source, this, _currentInsertIndex);

            HideInsertPreviewIfPossible();
        }

        internal void Attach(Window2Element source)
        {
            if (_currentSplitPreviewTarget is not null)
            {
                var edge = EdgeNormalToEdge(_currentSplitPreviewEdgeNormal);

                var shouldRemoveTargetAfterDetach = source._containerWindow == _containerWindow
                    && _currentSplitPreviewTarget == _containerWindow
                    && _currentSplitPreviewTarget._childWindows.Count <= 2;
                if (shouldRemoveTargetAfterDetach)
                {
                    foreach (var childWindow in _containerWindow._childWindows)
                    {
                        if (childWindow != source)
                        {
                            HideSplitPreviewIfPossible();

                            _currentSplitPreviewTarget = childWindow;
                            if (_currentSplitPreviewTarget._childWindows is not null)
                            {
                                _currentSplitPreviewTarget = EdgeToInsertIndex(edge) > 0
                                    ? _currentSplitPreviewTarget._childWindows[^1]
                                    : _currentSplitPreviewTarget._childWindows[0];
                            }

                            break;
                        }
                    }
                }

                AttachTo(source, _currentSplitPreviewTarget, edge);
            }

            HideSplitPreviewIfPossible();
        }

        internal void AttachSolver(ComposableWindowsSolver solver)
        {
            _composableWindowsSolver = solver;
        }

        public Vector2 CalculateTabOffset()
        {
            if (Tab.Parent is not LineLayout parent)
                return Vector2.Zero;

            if (parent.ChildCount <= 1)
                return Vector2.Zero;

            var offset = Tab.Position - parent.GetChildAt(0).Position;
            return offset;
        }

        public void BringToFront()
        {
            var target = _rootWindow ?? this;
            if (target.Parent is ContainerElement container)
                container.BringToFront(target);
        }

        private void ShowInsertPreview(Window2Element source, Point position)
        {
            var isTabbed = _containerWindow is not null
                && _containerWindow._compositionType is CompositionType.Tabbed;

            _tabList.Clear();
            if (isTabbed)
            {
                for (var i = 0; i < _containerWindow._childWindows.Count; i++)
                {
                    var child = _containerWindow._childWindows[i];
                    _tabList.Add(child.Tab);
                }
            }
            else
            {
                _tabList.Add(Tab);
            }

            Element placeHolder;
            var areSame = isTabbed && _containerWindow == source._containerWindow;
            if (areSame)
            {
                placeHolder = source.Tab;
            }
            else
            {
                placeHolder = _insertPreviewPlaceHolder;
                placeHolder.Size = source.Tab.Size;

                if (_currentInsertIndex == EmptyInsertIndex)
                {
                    _tabList.Add(_insertPreviewPlaceHolder);
                }
                else
                {
                    _tabList.Insert(_currentInsertIndex, placeHolder);
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

            if (_currentInsertIndex == insertIndex)
                return;

            _currentInsertIndex = insertIndex;

            _tabRow.RemoveChild(placeHolder);
            _tabRow.InsertChild(insertIndex, placeHolder);

            if (areSame)
            {
                _containerWindow._childWindows.Remove(source);
                _containerWindow._childWindows.Insert(insertIndex, source);
            }

            InsertPreviewShown?.Invoke(this, Tab);
        }

        private void HideInsertPreviewIfPossible()
        {
            if (_currentInsertIndex == EmptyInsertIndex)
                return;

            _currentInsertIndex = EmptyInsertIndex;

            _tabRow.RemoveChild(_insertPreviewPlaceHolder);

            InsertPreviewHidden?.Invoke(this);
        }

        private bool TryShowSplitPreview(Window2Element source, Point position)
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
                    _currentSplitPreviewTarget.ShowSplitPreview(source, edgeNormal);

                    SplitPreviewShown?.Invoke(this);
                }

                return true;
            }

            HideSplitPreviewIfPossible();

            return false;
        }

        private bool TryDetectSplitTarget(Point position, Window2Element source,
            out Vector2 edgeNormal, out Window2Element window)
        {
            window = default;

            var interactionRectangle = _splitArea.InteractionRectangle;

            var selfThickness = (_splitArea.Size * SplitAreaSelfThicknessFactor).ToPoint();
            edgeNormal = interactionRectangle.GetEdgeNormal(selfThickness, position,
                RectangleUtilities.EdgeNormalResolveMode.PreferX);

            if (edgeNormal == Vector2.Zero)
                return false;

            window = this;

            if (_containerWindow is not null)
            {
                var thicknessFactor = _containerWindow._splitDirection is SplitDirection.Horizontal
                    ? Vector2.UnitY 
                    : Vector2.UnitX;
                var parentThickness = (_splitArea.Size * SplitAreaParentThicknessFactor * thicknessFactor).ToPoint();
                var parentEdgeNormal = interactionRectangle.GetEdgeNormal(parentThickness, position,
                    RectangleUtilities.EdgeNormalResolveMode.PreferX);

                if (parentEdgeNormal != Vector2.Zero)
                {
                    edgeNormal = parentEdgeNormal;
                    window = _containerWindow;
                }
            }

            var isValidSplitTarget = source != this
                || window == _containerWindow;

            return isValidSplitTarget;
        }

        private void ShowSplitPreview(Window2Element source, Vector2 edgeNormal)
        {
            var alignmentFactor = Vector2.Max(Vector2.Zero, edgeNormal);

            _splitPreviewExpanded.IsEnabled = true;
            _splitPreviewExpanded.ExpandWidth = edgeNormal.Y != 0;
            _splitPreviewExpanded.ExpandHeight = edgeNormal.X != 0;

            _splitPreviewAlignment.InnerElement = _splitPreviewWindow;
            _splitPreviewAlignment.AlignmentFactor = alignmentFactor;
            _splitPreviewAlignment.Pivot = alignmentFactor;

            _splitPreviewWindow.SetSize(Size * SplitSizeFactor);
            _splitPreviewWindow.Tab.CopyHeaderFrom(source.Tab);
        }

        private void HideSplitPreviewIfPossible()
        {
            if (_currentSplitPreviewTarget is null)
                return;

            _currentSplitPreviewTarget._splitPreviewExpanded.IsEnabled = false;

            _currentSplitPreviewTarget = null;
            _currentSplitPreviewEdgeNormal = Vector2.Zero;

            SplitPreviewHidden?.Invoke(this);
        }

        private void SetSize(Vector2 size)
        {
            Size = size;

            if (InnerElement is not null)
                InnerElement.Size = Size;
        }

        private void RecalculateContainerMinSize()
        {
            var totalSize = Vector2.Zero;
            var maxMinSize = Vector2.Zero;

            foreach (var window in _childWindows)
            {
                totalSize += window.MinSize;
                maxMinSize = Vector2.Max(maxMinSize, window.MinSize);
            }

            if (_compositionType is CompositionType.Tabbed)
            {
                MinSize = maxMinSize;
            }
            else
            {
                MinSize = _splitDirection is SplitDirection.Horizontal
                    ? new Vector2(totalSize.X, maxMinSize.Y)
                    : new Vector2(maxMinSize.X, totalSize.Y);
            }

            _containerWindow?.RecalculateContainerMinSize();
        }

        private void ApplyRootWindow(Window2Element window)
        {
            _rootWindow = window;

            if (_childWindows is null)
                return;

            foreach (var childWindow in _childWindows)
                childWindow.ApplyRootWindow(window ?? this);
        }

        private void IncreaseSizeInHierarchyIfPossible(SplitDirection splitDirection,
            Vector2 axis, float axisDelta, float delta)
        {
            if (_containerWindow is null)
                return;

            var target = this;

            if (_containerWindow._splitDirection != splitDirection)
            {
                _containerWindow.IncreaseSizeInHierarchyIfPossible(splitDirection, axis, axisDelta, delta);
                return;
            }

            var targetIndex = _containerWindow._childWindows.IndexOf(target);
            var neighborIndex = targetIndex + MathF.Sign(axisDelta) * MathF.Sign(delta);

            var hasNeighbor = neighborIndex >= 0
                && neighborIndex < _containerWindow._childWindows.Count;
            if (!hasNeighbor)
            {
                _containerWindow.IncreaseSizeInHierarchyIfPossible(splitDirection, axis, axisDelta, delta);
                return;
            }

            // Should decrease size or increase size neighbor window.
            if (delta < 0)
            {
                target = _containerWindow._childWindows[neighborIndex];
                delta *= -1;
            }

            IncreaseSizeIfPossible(target, axis, delta, axisDelta > 0);
        }

        private void ResolveResizeCursor(IPointer pointer, Point position)
        {
            var cursor = PointerCursor.Arrow;

            var normal = GetResizeNormal(position);
            if (normal != Vector2.Zero)
            {
                var shouldCheckFirstChild = normal.X < 0 || normal.Y < 0;
                var splitDirection = EdgeNormalToSplitDirection(normal);

                var child = this;
                var containerWindow = _containerWindow;
                while (containerWindow is not null)
                {
                    var targetChild = shouldCheckFirstChild
                        ? containerWindow._childWindows[0]
                        : containerWindow._childWindows[^1];

                    var canResize = containerWindow._splitDirection == splitDirection
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
                    containerWindow = child._containerWindow;
                }
            }

            pointer.SetCursor(cursor);
        }

        private Vector2 GetResizeNormal(Point position)
        {
            var normal = _innerResizeHandle.InteractionRectangle.GetEdgeNormal(SplitAreaResizeHandleSize,
                position, RectangleUtilities.EdgeNormalResolveMode.PreferX);

            return normal;
        }

        private void OnDragHandlePointerDown(PointerInputHandlerElement sender,
            PointerEvent pointerEvent)
        {
            BringToFront();
        }

        private void OnDragHandlePointerFixedDrag(PointerInputHandlerElement sender,
            PointerDragEvent pointerEvent)
        {
            if (IsResizingInternally)
                return;

            var targetWindow = _rootWindow ?? this;
            targetWindow.Position += pointerEvent.Delta.ToVector2();
        }

        private void OnTabButtonPointerDown(PointerInputHandlerElement sender,
            PointerEvent pointerEvent)
        {
            if (IsResizingInternally)
                return;

            _isTapPressed = true;
            _dragDeltaAccumulator = CalculateTabOffset();

            Tab.InnerElement.IsEnabled = false;

            _composableWindowsSolver?.SelectSource(this);

            TabPointerDown?.Invoke(this, pointerEvent);
        }

        private void OnTabButtonPointerUp(PointerInputHandlerElement sender,
            PointerEvent pointerEvent)
        {
            if (!_isTapPressed)
                return;

            _isTapPressed = false;

            Tab.InnerElement.IsEnabled = true;

            if (_composableWindowsSolver is not null)
            {
                var result = _composableWindowsSolver.TryCompose();
                switch(result)
                {
                    case CompositionResult.None:
                        if (_containerWindow is not null)
                        {
                            DetachFromParent(this, Position + _dragDeltaAccumulator);

                            var oldSize = Size;
                            var newSize = Vector2.Max(MinSize, oldSize);
                            SetSize(newSize);
                            Position += (newSize - oldSize) * Pivot;
                        }
                        else
                        {
                            Position += _dragDeltaAccumulator;
                        }
                        break;
                    case CompositionResult.Attached:
                    case CompositionResult.Inserted:
                    default:
                        break;
                }
                BringToFront();
            }

            TabPointerUp?.Invoke(this, pointerEvent);
        }

        private void OnTabButtonPointerDrag(PointerInputHandlerElement sender,
            PointerDragEvent pointerEvent)
        {
            if (!_isTapPressed)
                return;

            _dragDeltaAccumulator += pointerEvent.Delta.ToVector2();

            TabPointerDrag?.Invoke(this, pointerEvent);
        }

        private void OnInsertAreaPointerMove(PointerInputHandlerElement sender,
            PointerEvent pointerEvent)
        {
            if (_composableWindowsSolver is null)
                return;

            var source = _composableWindowsSolver.Source;
            if (source is null)
                return;

            ShowInsertPreview(source, pointerEvent.Position);
            _composableWindowsSolver.SelectInsertTarget(this);
        }

        private void OnInsertAreaPointerLeave(PointerInputHandlerElement sender,
            PointerEvent pointerEvent)
        {
            _composableWindowsSolver?.ReleaseInsertTarget(this);
            HideInsertPreviewIfPossible();
        }

        private void OnSplitAreaPointerDown(PointerInputHandlerElement sender, PointerEvent e)
        {
            BringToFront();
        }

        private void OnSplitAreaPointerMove(PointerInputHandlerElement sender,
            PointerEvent pointerEvent)
        {
            if (_composableWindowsSolver is null)
                return;

            var source = _composableWindowsSolver.Source;
            if (source is null)
                return;

            if (TryShowSplitPreview(source, pointerEvent.Position))
            {
                _composableWindowsSolver.SelectAttachTarget(this);
            }
            else
            {
                _composableWindowsSolver.ReleaseAttachTarget(this);
            }
        }

        private void OnSplitAreaPointerLeave(PointerInputHandlerElement sender,
            PointerEvent pointerEvent)
        {
            _composableWindowsSolver?.ReleaseAttachTarget(this);
            HideSplitPreviewIfPossible();
        }

        private void OnInnerResizeHandlePointerMove(PointerInputHandlerElement sender,
            PointerEvent pointerEvent)
        {
            if (_containerWindow is null) 
                return;

            if (pointerEvent.IsPrimaryButtonPressed)
                return;

            ResolveResizeCursor(pointerEvent.Pointer, pointerEvent.Position);
        }

        private void OnInnerResizeHandlePointerLeave(PointerInputHandlerElement sender,
            PointerEvent pointerEvent)
        {
            if (pointerEvent.IsPrimaryButtonPressed)
                return;

            pointerEvent.Pointer.SetCursor(PointerCursor.Arrow);
        }

        private void OnInnerResizeHandlePointerDown(PointerInputHandlerElement sender,
            PointerEvent pointerEvent)
        {
            if (_containerWindow is null)
                return;

            _resizeNormal = GetResizeNormal(pointerEvent.Position);
            _resizeAxis = new Vector2(MathF.Abs(_resizeNormal.X), MathF.Abs(_resizeNormal.Y));
        }

        private void OnInnerResizeHandlePointerUp(PointerInputHandlerElement sender,
            PointerEvent pointerEvent)
        {
            _resizeNormal = Vector2.Zero;
        }

        private void OnInnerResizeHandlePointerFixedDrag(PointerInputHandlerElement sender,
            PointerDragEvent pointerEvent)
        {
            if (!IsResizingInternally)
                return;

            var deltaVector = pointerEvent.Delta.ToVector2();

            var axisDelta = Vector2.Dot(_resizeAxis, deltaVector);
            if (axisDelta == 0)
                return;

            var delta = Vector2.Dot(_resizeNormal, deltaVector);
            var splitDirection = EdgeNormalToSplitDirection(_resizeNormal);

            IncreaseSizeInHierarchyIfPossible(splitDirection, _resizeAxis, axisDelta, delta);
        }

        protected override void OnPointerDown(in PointerEvent pointerEvent)
        {
            base.OnPointerDown(pointerEvent);
            BringToFront();
        }

        private enum SplitDirection
        {
            None,

            Horizontal,
            Vertical
        }

        private enum CompositionType
        {
            None,

            Adjacent,
            Tabbed
        }
    }
}
