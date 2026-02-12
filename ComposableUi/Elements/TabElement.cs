using Microsoft.Xna.Framework;

namespace ComposableUi
{
    public class TabElement : ResizableElement
    {
        public const int DefaultHeaderHeight = 30;

        public static readonly Vector2 DefaultSize = new(250, 300);
        public static readonly Vector2 DefaultMinSize = new(80, 100);
        public static readonly Vector2 DefaultContentPadding = new(10, 10);

        public Element Header { get; }
        public TabButtonElement TabButton { get; }

        private readonly RowLayout _tabsRow;
        private readonly ContainerElement _contentContainer;

        private readonly PointerInputHandlerElement _splitArea;

        private TabElement _parentTab;
        private TabElement _rootTab;

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
                        new ExpandedElement(new SpriteElement(skin: StandardSkin.TabInactiveHeader)),
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

            _splitArea = new PointerInputHandlerElement(
                blockInput: false
            );
            var splitAreaParent = new ExpandedElement(
                topPadding: DefaultHeaderHeight,
                innerElement: _splitArea
            );
            _splitArea.PointerMove += OnSplitAreaPointerMove;

            InnerElement = new ContainerElement(
                children: [
                    background,
                    contentContainerParent,
                    headerParent,
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

        private void ShowSplitPreview()
        {

        }

        private void OnTabButtonPointerDown(Element sender, Point position)
        {
            _composableTabsSolver?.Select(this);
        }

        private void OnSplitAreaPointerMove(Element sender, Point position)
        {
            if (_composableTabsSolver is null)
                return;

            var selectedTab = _composableTabsSolver.SelectedTab;
            if (selectedTab is null)
                return;

            if (selectedTab == this)
                return;

            ShowSplitPreview();
        }

        private void OnTabButtonPointerUp(Element sender, Point position)
        {
            _composableTabsSolver?.Release(this);
        }
    }
}
