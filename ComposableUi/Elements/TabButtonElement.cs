using Microsoft.Xna.Framework;

namespace ComposableUi
{
    public sealed class TabButtonElement : PointerInputHandlerElement
    {
        public const int DefaultLeftPadding = 6;
        public const int DefaultRightPadding = 12;
        public const int DefaultItemSpacing = 4;

        public SpriteElement Background { get; }
        public SpriteElement Icon { get; }
        public TextElement Text { get; }

        public TabButtonElement(string titleText = default,
            Sprite iconSprite = default) 
        {
            Background = new SpriteElement(
                skin: StandardSkin.TabNormalHeader
            );
            var backgroundParent = new LayoutElement(
                ignoreLayout: true,
                innerElement: new ExpandedElement(
                    innerElement: Background
                )
            );

            Icon = new SpriteElement(
                size: new Vector2(10),
                sprite: iconSprite,
                skin: StandardSkin.RectangleButton,
                sizeToSource: true
            );

            Text = new TextElement(
                text: titleText,
                sizeToTextWidth: true,
                sizeToTextHeight: true
            );

            InnerElement = new RowLayout(
                spacing: DefaultItemSpacing,
                leftPadding: DefaultLeftPadding,
                rightPadding: DefaultRightPadding,
                alignmentFactor: Alignment.MiddleLeft,
                sizeMainAxisToContent: true,
                children: [backgroundParent, Icon, Text]
            );
        }
    }
}
