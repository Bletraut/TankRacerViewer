using ComposableUi;

using Microsoft.Xna.Framework;

namespace TankRacerViewer.Core
{
    public sealed class IconButtonElement : ButtonElement
    {
        public const int DefaultButtonSize = 22;
        public const int DefaultButtonIconSize = 18;

        // Static.
        public static readonly Color DefaultNormalButtonColor = Color.Plum;
        public static readonly Color DefaultHoverButtonColor = Color.Coral;
        public static readonly Color DefaultPressedButtonColor = Color.LightSlateGray;

        public SpriteElement Icon { get; }

        public IconButtonElement(Sprite iconSprite = default,
            StandardSkin iconSkin = default)
            : base(size: new Vector2(DefaultButtonSize),
                normalSkin: StandardSkin.WhitePixel,
                hoverSkin: StandardSkin.WhitePixel,
                pressedSkin: StandardSkin.WhitePixel,
                normalColor: DefaultNormalButtonColor,
                hoverColor: DefaultHoverButtonColor,
                pressedColor: DefaultPressedButtonColor)
        {
            Icon = new SpriteElement(
                size: new Vector2(DefaultButtonIconSize),
                sprite: iconSprite,
                skin: iconSkin,
                sizeToSource: true,
                drawMode: DrawMode.Simple
            );

            InnerElement = new HolderElement(
                size: new Vector2(DefaultButtonSize),
                innerElement: new AlignmentElement(
                    alignmentFactor: Alignment.Center,
                    innerElement: Icon
                )
            );
        }
    }
}
