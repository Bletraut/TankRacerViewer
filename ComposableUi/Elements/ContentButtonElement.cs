using Microsoft.Xna.Framework;

namespace ComposableUi
{
    public class ContentButtonElement : ButtonElement
    {
        public const float DefaultContentSpacing = 4;
        public const float DefaultContentPaddings = 4;

        // Static.
        public static readonly Vector2 DefaultButtonSize = new(22);
        public static readonly Vector2 DefaultIconSize = new(18);

        public static readonly Color DefaultNormalButtonColor = Color.Plum;
        public static readonly Color DefaultHoverButtonColor = Color.Coral;
        public static readonly Color DefaultPressedButtonColor = Color.LightSlateGray;

        // Class.
        public SpriteElement Icon { get; }
        public TextElement Text { get; }
        public RowLayout ContentLayout { get; }

        public ContentButtonElement(Vector2? iconSize = default,
            Sprite iconSprite = default,
            StandardSkin iconSkin = StandardSkin.WhitePixel,
            string text = default,
            Sprite normalSprite = default,
            Sprite hoverSprite = default,
            Sprite pressedSprite = default,
            Sprite disabledSprite = default,
            StandardSkin normalSkin = StandardSkin.RectangleButton,
            StandardSkin hoverSkin = StandardSkin.HoverRectangleButton,
            StandardSkin pressedSkin = StandardSkin.PressedRectangleButton,
            StandardSkin disabledSkin = StandardSkin.DisabledRectangleButton,
            Color? normalColor = default,
            Color? hoverColor = default,
            Color? pressedColor = default,
            Color? disabledColor = default,
            bool isInteractable = true)
            : base(normalSprite: normalSprite,
                  hoverSprite: hoverSprite,
                  pressedSprite: pressedSprite,
                  disabledSprite: disabledSprite,
                  normalSkin: normalSkin,
                  hoverSkin: hoverSkin,
                  pressedSkin: pressedSkin,
                  disabledSkin: disabledSkin,
                  normalColor: normalColor,
                  hoverColor: hoverColor,
                  pressedColor: pressedColor,
                  disabledColor: disabledColor,
                  isInteractable: isInteractable)
        {
            Icon = new SpriteElement(
                size: iconSize ?? DefaultIconSize,
                sprite: iconSprite,
                skin: iconSkin,
                sizeToSource: true,
                drawMode: DrawMode.Simple
            );

            Text = new TextElement(
                text: text,
                textAlignmentFactor: Alignment.MiddleLeft,
                sizeToTextWidth: true,
                sizeToTextHeight: true
            );

            ContentLayout = new RowLayout(
                alignmentFactor: Alignment.Center,
                spacing: DefaultContentSpacing,
                leftPadding: DefaultContentPaddings,
                rightPadding: DefaultContentPaddings,
                topPadding: DefaultContentPaddings,
                bottomPadding: DefaultContentPaddings,
                sizeMainAxisToContent: true,
                sizeCrossAxisToContent: true,
                children: [
                    Icon,
                    Text
                ]
            );

            InnerElement = ContentLayout;
        }
    }
}
