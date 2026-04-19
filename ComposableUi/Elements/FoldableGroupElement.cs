using Microsoft.Xna.Framework;

namespace ComposableUi
{
    public class FoldableGroupElement : SizedToContentHolderElement
    {
        public const float DefaultTitleSpacing = 4;
        public const float DefaultContentSpacing = 4;
        public const float DefaultContentVerticalPadding = 4;

        public const StandardSkin DefaultTitleBackgroundSkin = StandardSkin.SelectionSoftDarkPixel;
        public const StandardSkin DefaultContentBackgroundSkin = StandardSkin.HoverSoftDarkPixel;

        public readonly float DefaultTitleHorizontalPadding = 4;
        public readonly float DefaultContentIndent = 12;

        public readonly Vector2 DefaultFoldButtonSize = new(12);
        public readonly Vector2 DefaultIconSize = new(12);

        private bool _isFolded;
        public bool IsFolded
        {
            get => _isFolded;
            set
            {
                _isFolded = value;

                ContentLayout.IsEnabled = !_isFolded;
                RefreshFoldButtonSkin();
            }
        }

        public SpriteElement TitleBackground { get; }
        public SpriteElement ContentBackground { get; }

        public ButtonElement FoldButton { get; }
        public SpriteElement Icon { get; }
        public TextElement Name { get; }

        public RowLayout TitleLayout { get; }
        public ColumnLayout ContentLayout { get; }

        private readonly ColumnLayout _groupColumn;

        public FoldableGroupElement(Sprite iconSprite = default,
            StandardSkin iconSkin = default,
            string name = default,
            Element content = default,
            bool isFolded = default,
            Color? titleBackgroundColor = default,
            Color? contentBackgroundColor = default)
        {
            TitleBackground = new SpriteElement(
                skin: DefaultTitleBackgroundSkin
            );
            ContentBackground = new SpriteElement(
                skin: DefaultContentBackgroundSkin
            );

            FoldButton = new ButtonElement(
                size: DefaultFoldButtonSize,
                normalColor: Color.FloralWhite,
                hoverColor: Color.BlanchedAlmond,
                pressedColor: Color.LightSteelBlue
            );

            FoldButton.PointerClick += OnFoldButtonPointerClick;

            Icon = new SpriteElement(
                size: DefaultIconSize,
                sprite: iconSprite,
                skin: iconSkin
            );

            Name = new TextElement(
                textAlignmentFactor: Alignment.MiddleLeft,
                text: name,
                sizeToTextWidth: true,
                sizeToTextHeight: true
            );

            TitleLayout = new RowLayout(
                alignmentFactor: Alignment.MiddleLeft,
                leftPadding: DefaultTitleHorizontalPadding,
                rightPadding: DefaultTitleHorizontalPadding,
                spacing: DefaultTitleSpacing,
                sizeMainAxisToContent: true,
                sizeCrossAxisToContent: true,
                children: [
                    new LayoutElement(
                        ignoreLayout: true,
                        innerElement: new ExpandedElement(
                            innerElement: TitleBackground
                        )
                    ),
                    FoldButton,
                    Icon,
                    Name,
                ]
            );

            ContentLayout = new ColumnLayout(
                leftPadding: DefaultContentIndent,
                topPadding: DefaultContentVerticalPadding,
                bottomPadding: DefaultContentVerticalPadding,
                spacing: DefaultContentSpacing,
                sizeMainAxisToContent: true,
                children: [
                    new LayoutElement(
                        ignoreLayout: true,
                        innerElement: new ExpandedElement(
                            innerElement: ContentBackground
                        )
                    ),
                ]
            );

            _groupColumn = new ColumnLayout(
                sizeMainAxisToContent: true,
                expandChildrenCrossAxis: true,
                children: [
                    TitleLayout,
                    ContentLayout
                ]
            );

            if (content is not null)
                ContentLayout.AddChild(content);

            InnerElement = _groupColumn;

            IsFolded = isFolded;
        }

        private void RefreshFoldButtonSkin()
        {
            var skin = IsFolded 
                ? StandardSkin.RightArrowIcon 
                : StandardSkin.DownArrowIcon;

            FoldButton.NormalSkin = skin;
            FoldButton.HoverSkin = skin;
            FoldButton.PressedSkin = skin;
            FoldButton.DisabledSkin = skin;
        }

        private void OnFoldButtonPointerClick(PointerInputHandlerElement sender,
            PointerEvent pointerEvent)
        {
            IsFolded = !IsFolded;
        }
    }
}
