using System.Collections.Generic;
using System.Diagnostics;

using ComposableUi.Utilities;

using Microsoft.Xna.Framework;

namespace ComposableUi
{
    public class TabElement : ResizableElement
    {
        public const int DefaultHeaderHeight = 30;

        public static readonly Vector2 DefaultSize = new(250, 300);
        public static readonly Vector2 DefaultMinSize = new(80, 100);
        public static readonly Vector2 DefaultContentPadding = new(10, 10);

        // Static.
        private static readonly Stack<TabElement> _tabPool = new();

        private static readonly TabElement _splitPreviewTab = CreateNonInteractiveTab();

        internal static TabElement Get()
        {
            if (!_tabPool.TryPop(out var tab))
                tab = new TabElement();
            tab.IsEnabled = true;

            return tab;
        }

        internal static void Release(TabElement tab)
        {
            tab.IsEnabled = false;
            _tabPool.Push(tab);
        }

        internal static TabElement CreateNonInteractiveTab()
        {
            var tab =  new TabElement()
            {
                IsInteractable = false,
                BlockInput = false
            };
            tab.DragHandle.IsInteractable = false;
            tab.DragHandle.BlockInput = false;
            tab.TabButton.IsInteractable = false;
            tab.TabButton.BlockInput = false;
            tab._splitArea.IsEnabled = false;

            return tab;
        }

        // Class.
        public Element Header { get; }
        public PointerInputHandlerElement DragHandle { get; }
        public TabButtonElement TabButton { get; }

        public event ElementEventHandler<TabElement, Point> TabButtonPointerDown;
        public event ElementEventHandler<TabElement, Point> TabButtonPointerUp;
        public event ElementEventHandler<TabElement, Point> TabButtonPointerDrag;
        public event ElementEventHandler<TabElement> SplitPreviewShown;
        public event ElementEventHandler<TabElement> SplitPreviewHidden;

        private readonly RowLayout _tabsRow;
        private readonly ContainerElement _contentContainer;

        private readonly PointerInputHandlerElement _splitArea;

        private readonly ExpandedElement _splitPreviewExpanded;
        private readonly AlignmentElement _splitPreviewAlignment;

        private TabElement _parentTab;
        private TabElement _rootTab;

        private Vector2 _currentSplitPreviewEdgeNormal;

        private ComposableTabsSolver _composableTabsSolver;

        public TabElement(string titleText = default,
            Element content = default,
            Vector2? size = default,
            Vector2? minSize = default)
        {
            MinSize = minSize ?? DefaultMinSize;

            var background = new ExpandedElement(
                innerElement: new SpriteElement(
                    skin: StandardSkin.TabBody,
                    color: new Color(Color.Green, 0.5f)
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

            TabButton = new TabButtonElement(
                titleText: titleText
            );
            _tabsRow.AddChild(TabButton);

            TabButton.PointerDown += OnTabButtonPointerDown;
            TabButton.PointerUp += OnTabButtonPointerUp;
            TabButton.PointerDrag += OnTabButtonPointerDrag;

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

            InnerElement = new ContainerElement(
                children: [
                    background,
                    contentContainerParent,
                    headerParent,
                    _splitPreviewExpanded,
                    splitAreaParent,
                ]
            )
            {
                Size = size ?? DefaultSize
            };
        }

        internal void AttachSolver(ComposableTabsSolver solver)
        {
            _composableTabsSolver = solver;
        }

        private void ShowSplitPreviewIfPossible(TabElement tab, Point position)
        {
            var thickness = (Size * 0.3f).ToPoint();
            var normal = InteractionRectangle.GetEdgeNormal(thickness, position);
            normal.Y = normal.X != 0 ? 0 : normal.Y;

            if (_currentSplitPreviewEdgeNormal == normal)
                return;

            if (normal == Vector2.Zero)
            {
                HideSplitPreviewIfPossible();
                return;
            }

            _currentSplitPreviewEdgeNormal = normal;
            ShowSplitPreview(tab, _currentSplitPreviewEdgeNormal);

            SplitPreviewShown?.Invoke(this);
        }

        private void ShowSplitPreview(TabElement tab, Vector2 edgeNormal)
        {
            var alignmentFactor = Vector2.Max(Vector2.Zero, edgeNormal);

            _splitPreviewExpanded.IsEnabled = true;
            _splitPreviewExpanded.ExpandWidth = edgeNormal.Y != 0;
            _splitPreviewExpanded.ExpandHeight = edgeNormal.X != 0;

            _splitPreviewAlignment.InnerElement = _splitPreviewTab;
            _splitPreviewAlignment.AlignmentFactor = alignmentFactor;
            _splitPreviewAlignment.Pivot = alignmentFactor;

            _splitPreviewTab.InnerElement.Size = Size * 0.3f;
            _splitPreviewTab.TabButton.Icon.Sprite = tab.TabButton.Icon.Sprite;
            _splitPreviewTab.TabButton.Text.Text = tab.TabButton.Text.Text;
        }

        private void HideSplitPreviewIfPossible()
        {
            if (_currentSplitPreviewEdgeNormal == Vector2.Zero)
                return;

            _currentSplitPreviewEdgeNormal = Vector2.Zero;

            _splitPreviewExpanded.IsEnabled = false;

            SplitPreviewHidden?.Invoke(this);
        }

        private void OnDragHandlePointerFixedDrag(PointerInputHandlerElement sender, (Point Position, Point Delta) arguments)
        {
            var targetTab = _rootTab ?? this;
            targetTab.Position += arguments.Delta.ToVector2();
        }

        private void OnTabButtonPointerDown(PointerInputHandlerElement sender, Point position)
        {
            _composableTabsSolver?.Select(this);
            TabButtonPointerDown?.Invoke(this, position);
        }

        private void OnTabButtonPointerUp(PointerInputHandlerElement sender, Point position)
        {
            _composableTabsSolver?.Release(this);
            TabButtonPointerUp?.Invoke(this, position);
        }

        private void OnTabButtonPointerDrag(PointerInputHandlerElement sender,
            (Point Position, Point Delta) arguments)
        {
            TabButtonPointerDrag?.Invoke(this, arguments.Delta);
        }

        private void OnSplitAreaPointerMove(PointerInputHandlerElement sender, Point position)
        {
            if (_composableTabsSolver is null)
                return;

            var selectedTab = _composableTabsSolver.SelectedTab;
            if (selectedTab is null)
                return;

            if (selectedTab == this)
                return;

            ShowSplitPreviewIfPossible(selectedTab, position);
        }

        private void OnSplitAreaPointerLeave(PointerInputHandlerElement sender, Point position)
        {
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
