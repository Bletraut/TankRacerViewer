using System;
using System.Collections.Generic;
using System.Diagnostics;

using ComposableUi.Utilities;

using Microsoft.Xna.Framework;

namespace ComposableUi
{
    public class Window2Element : ResizableElement
    {
        public const int DefaultHeaderHeight = 30;

        private const float SplitSizeFactor = 0.3f;
        private const float SplitAreaSelfThicknessFactor = 0.3f;
        private const float SplitAreaParentThicknessFactor = 0.1f;

        public static readonly Vector2 DefaultSize = new(250, 300);
        public static readonly Vector2 DefaultMinSize = new(80, 100);
        public static readonly Vector2 DefaultContentPadding = new(10, 10);

        // Static.
        private static readonly Stack<Window2Element> _windowPool = new();

        private static readonly Window2Element _splitPreviewWindow = CreateNonInteractiveWindow();

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
            window._splitArea.IsEnabled = false;

            return window;
        }

        internal static LineLayout GetRow()
        {
            var layout = new RowLayout(
                expandChildrenMainAxis: true,
                expandChildrenCrossAxis: true,
                mainAxisChildrenExpandingMode: ExpandingMode.Size
            );

            // For debug.
            layout.AddChild(new LayoutElement(
                ignoreLayout: true,
                innerElement: new ExpandedElement(
                    innerElement: new SpriteElement(
                        skin: StandardSkin.WhitePixel,
                        color: new Color(Random.Shared.NextSingle(), Random.Shared.NextSingle(), Random.Shared.NextSingle())
                    )
                )
            ));
            layout.Spacing = 4;
            layout.LeftPadding = layout.RightPadding = 4;
            layout.TopPadding = layout.BottomPadding = 4;
            // end.

            return layout;
        }

        internal static LineLayout GetColumn()
        {
            var layout = new ColumnLayout(
                expandChildrenMainAxis: true,
                expandChildrenCrossAxis: true,
                mainAxisChildrenExpandingMode: ExpandingMode.Size
            );

            // For debug.
            layout.AddChild(new LayoutElement(
                ignoreLayout: true,
                innerElement: new ExpandedElement(
                    innerElement: new SpriteElement(
                        skin: StandardSkin.WhitePixel,
                        color: new Color(Random.Shared.NextSingle(), Random.Shared.NextSingle(), Random.Shared.NextSingle())
                    )
                )
            ));
            layout.Spacing = 4;
            layout.LeftPadding = layout.RightPadding = 4;
            layout.TopPadding = layout.BottomPadding = 4;
            // end.

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

            var shouldAttachToParent = target._containerWindow?._currentSplitDirection == splitDirection;
            if (shouldAttachToParent)
            {
                if (TryAttachToParent(source, target, edge))
                    return;
            }

            var insertIndex = EdgeToInsertIndex(edge);
            var splitLayout = splitDirection is SplitDirection.Horizontal 
                ? GetRow() 
                : GetColumn();

            var containerWindow = GetContainerWindow();
            containerWindow._currentSplitDirection = splitDirection;
            containerWindow._viewHolder.InnerElement = splitLayout;
            containerWindow.SetSize(target.Size);
            containerWindow._containerWindow = target._containerWindow;
            containerWindow.IsInteractable =  target.IsInteractable;
            containerWindow.LocalPosition = target.LocalPosition 
                - target.PivotOffset + containerWindow.PivotOffset;

            // Replaces the target window with the container window.
            if (target._containerWindow is not null)
            {
                if (target._containerWindow._viewHolder.InnerElement is LineLayout parentSplitLayout)
                {
                    var index = parentSplitLayout.IndexOf(target);
                    parentSplitLayout.InsertChild(index, containerWindow);
                }

                target._containerWindow._childWindows.Remove(target);
                target._containerWindow._childWindows.Add(containerWindow);
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
            containerWindow._childWindows.Add(source);

            containerWindow.ApplyRootWindow(target._rootWindow);
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

            if (containerWindow._childWindows.Count == 1)
            {
                var lastWindow = containerWindow._childWindows[0];
                containerWindow._childWindows.Clear();

                lastWindow.SetSize(containerWindow.Size);
                lastWindow._containerWindow = containerWindow._containerWindow;
                lastWindow.ApplyRootWindow(containerWindow._rootWindow);
                lastWindow.IsInteractable = containerWindow.IsInteractable;

                if (containerWindow._containerWindow?._viewHolder.InnerElement is ContainerElement parentSplitLayout)
                {
                    var index = parentSplitLayout.IndexOf(containerWindow);
                    parentSplitLayout.InsertChild(index, lastWindow);

                    containerWindow._containerWindow._childWindows.Add(lastWindow);
                    containerWindow._containerWindow._childWindows.Remove(containerWindow);
                }
                else
                {
                    var worldPosition = containerWindow.Position;
                    containerWindow.Parent.AddChild(lastWindow);
                    lastWindow.Position = worldPosition - containerWindow.PivotOffset + lastWindow.PivotOffset;
                }
            }
            if (containerWindow._childWindows.Count <= 0)
            {
                containerWindow._currentSplitDirection = SplitDirection.None;
                containerWindow._containerWindow = null;
                containerWindow.ApplyRootWindow(null);
                containerWindow._viewHolder.InnerElement = null;
                containerWindow.Parent?.RemoveChild(containerWindow);

                ReleaseWindow(containerWindow);
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
            target._containerWindow._childWindows.Add(source);

            return true;
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

        // Class.
        public Element Header { get; }
        public PointerInputHandlerElement DragHandle { get; }
        public TabElement Tab { get; }

        public event ElementEventHandler<Window2Element, Point> TabPointerDown;
        public event ElementEventHandler<Window2Element, Point> TabPointerUp;
        public event ElementEventHandler<Window2Element, Point> TabPointerDrag;
        public event ElementEventHandler<Window2Element> SplitPreviewShown;
        public event ElementEventHandler<Window2Element> SplitPreviewHidden;

        private readonly Element _view;
        private readonly ExpandedElement _viewHolder;

        private readonly RowLayout _tabsRow;
        private readonly ContainerElement _contentContainer;

        private readonly PointerInputHandlerElement _splitArea;

        private readonly ExpandedElement _splitPreviewExpanded;
        private readonly AlignmentElement _splitPreviewAlignment;

        private List<Window2Element> _childWindows;

        private Window2Element _containerWindow;
        private Window2Element _rootWindow;

        private Vector2 _dragDeltaAccumulator;
        private Window2Element _currentSplitPreviewTarget;
        private Vector2 _currentSplitPreviewEdgeNormal;
        private SplitDirection _currentSplitDirection;

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
            _splitArea.PointerDown += OnSlitAreaPointerDown;
            _splitArea.PointerMove += OnSplitAreaPointerMove;
            _splitArea.PointerLeave += OnSplitAreaPointerLeave;
            _splitArea.PointerFixedDrag += OnSplitAreaPointerFixedDrag;

            _view = new ContainerElement(
                children: [
                    background,
                    contentContainerParent,
                    headerParent
                ]
            );
            _viewHolder = new ExpandedElement(_view);

            InnerElement = new ContainerElement(
                size: size ?? DefaultSize,
                children: [
                    _viewHolder,
                    _splitPreviewExpanded,
                    splitAreaParent,
                ]
            );
        }

        internal void Attach(Window2Element source)
        {
            var edge = EdgeNormalToEdge(_currentSplitPreviewEdgeNormal);
            AttachTo(source, _currentSplitPreviewTarget, edge);

            HideSplitPreviewIfPossible();
        }

        internal void AttachSolver(ComposableWindowsSolver solver)
        {
            _composableWindowsSolver = solver;
        }

        public void BringToFront()
        {
            var target = _rootWindow ?? this;
            if (target.Parent is ContainerElement container)
                container.BringToFront(target);
        }

        private bool TryShowSplitPreview(Window2Element window, Point position)
        {
            if (TryDetectSplitTarget(position, out var edgeNormal, out var target))
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
                    _currentSplitPreviewTarget.ShowSplitPreview(window, edgeNormal);

                    SplitPreviewShown?.Invoke(this);
                }

                return true;
            }

            HideSplitPreviewIfPossible();

            return false;
        }

        private bool TryDetectSplitTarget(Point position, out Vector2 edgeNormal, out Window2Element window)
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
                var thicknessFactor = _containerWindow._currentSplitDirection is SplitDirection.Horizontal
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

            return true;
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
            InnerElement.Size = Size;
        }

        private void ApplyRootWindow(Window2Element window)
        {
            _rootWindow = window;

            if (_childWindows is null)
                return;

            foreach (var childWindow in _childWindows)
                childWindow.ApplyRootWindow(window ?? this);
        }

        private void OnDragHandlePointerDown(PointerInputHandlerElement sender, Point e)
        {
            BringToFront();
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
                {
                    if (_containerWindow is not null)
                    {
                        DetachFromParent(this, Position + _dragDeltaAccumulator);
                    }
                    else
                    {
                        Position += _dragDeltaAccumulator;
                    }
                }
                BringToFront();
            }

            TabPointerUp?.Invoke(this, position);
        }

        private void OnTabButtonPointerDrag(PointerInputHandlerElement sender,
            (Point Position, Point Delta) arguments)
        {
            _dragDeltaAccumulator += arguments.Delta.ToVector2();

            TabPointerDrag?.Invoke(this, arguments.Delta);
        }

        private void OnSlitAreaPointerDown(PointerInputHandlerElement sender, Point position)
        {
            BringToFront();
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

        private void OnSplitAreaPointerFixedDrag(PointerInputHandlerElement sender,
            (Point Position, Point Delta) arguments)
        {
            if (_containerWindow is null) 
                return;

            // TODO: Implement child expanding.
            //SetSize(Size + arguments.Delta.ToVector2());
        }

        protected override void OnPointerDown(Point position)
        {
            base.OnPointerDown(position);
            BringToFront();
        }

        public enum Edge
        {
            Left,
            Right,
            Top,
            Bottom
        }

        private enum SplitDirection
        {
            None,

            Horizontal,
            Vertical
        }
    }
}
