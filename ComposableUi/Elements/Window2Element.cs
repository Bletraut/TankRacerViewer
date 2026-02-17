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
                expandChildrenCrossAxis: true
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
                expandChildrenCrossAxis: true
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
            window.InnerElement = null;

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

            var isHorizontalSplit = edge is Edge.Left or Edge.Right;
            var insertIndex = edge is Edge.Left or Edge.Top ? 0 : 1;

            var splitLayout = isHorizontalSplit ? GetRow() : GetColumn();

            var containerWindow = GetContainerWindow();
            containerWindow.Size = target.Size;
            containerWindow.InnerElement = splitLayout;
            containerWindow.InnerElement.Size = target.Size;
            containerWindow._containerWindow = target._containerWindow;
            containerWindow._rootWindow = target._rootWindow;
            containerWindow.IsInteractable =  target.IsInteractable;
            containerWindow.LocalPosition = target.LocalPosition 
                - target.PivotOffset + containerWindow.PivotOffset;

            if (target._containerWindow?.InnerElement is LineLayout parentSplitLayout)
            {
                var index = parentSplitLayout.IndexOf(target);
                parentSplitLayout.InsertChild(index, containerWindow);

                target._containerWindow._childWindows.Remove(target);
                target._containerWindow._childWindows.Add(containerWindow);
            }
            else
            {
                target.Parent.AddChild(containerWindow);
            }

            containerWindow._childWindows ??= [];

            target._containerWindow = containerWindow;
            target._rootWindow = containerWindow._rootWindow ?? containerWindow;
            target.IsInteractable = false;
            splitLayout.AddChild(target);
            containerWindow._childWindows.Add(target);

            source._containerWindow = containerWindow;
            source._rootWindow = containerWindow._rootWindow ?? containerWindow;
            source.IsInteractable = false;
            splitLayout.InsertChild(splitLayout.IndexOf(target) + insertIndex, source);
            containerWindow._childWindows.Add(source);
        }

        public static void AttachToParent(Window2Element source, Window2Element target, Edge edge)
        {

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
            source._rootWindow = null;
            source.IsInteractable = true;
            source.Position = position;

            if (containerWindow._childWindows.Count == 1)
            {
                var lastWindow = containerWindow._childWindows[0];
                containerWindow._childWindows.Clear();

                lastWindow.Size = containerWindow.Size;
                lastWindow.InnerElement.Size = containerWindow.Size;
                lastWindow._containerWindow = containerWindow._containerWindow;
                lastWindow._rootWindow = containerWindow._rootWindow;
                lastWindow.IsInteractable = containerWindow.IsInteractable;

                if (containerWindow._containerWindow?.InnerElement is LineLayout parentSplitLayout)
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
                containerWindow._containerWindow = null;
                containerWindow._rootWindow = null;
                containerWindow.InnerElement = null;
                containerWindow.Parent?.RemoveChild(containerWindow);

                ReleaseWindow(containerWindow);
            }
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

        private readonly ContainerElement _view;

        private readonly RowLayout _tabsRow;
        private readonly ContainerElement _contentContainer;

        private readonly PointerInputHandlerElement _splitArea;

        private readonly ExpandedElement _splitPreviewExpanded;
        private readonly AlignmentElement _splitPreviewAlignment;

        private List<Window2Element> _childWindows;

        private Window2Element _containerWindow;
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

            _view = new ContainerElement(
                size: size ?? DefaultSize,
                children: [
                    background,
                    contentContainerParent,
                    headerParent,
                    _splitPreviewExpanded,
                    splitAreaParent,
                ]
            );

            InnerElement = _view;
        }

        internal void Attach(Window2Element source)
        {
            var edge = _currentSplitPreviewEdgeNormal switch
            {
                { X: < 0 } => Edge.Left,
                { X: > 0 } => Edge.Right,
                { Y: < 0 } => Edge.Top,
                { Y: > 0 } => Edge.Bottom,
                _ => throw new NotImplementedException(),
            };

            AttachTo(source, this, edge);

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
