using ComposableUi;

using Microsoft.Xna.Framework;

namespace TankRacerViewer.Core
{
    public class IconButtonElement : ButtonElement
    {
        // Static.
        public static readonly Vector2 DefaultButtonSize = new(22);
        public static readonly Vector2 DefaultIconSize = new(18);

        public static readonly Color DefaultNormalButtonColor = Color.Plum;
        public static readonly Color DefaultHoverButtonColor = Color.Coral;
        public static readonly Color DefaultPressedButtonColor = Color.LightSlateGray;

        // Class.
        public SpriteElement Icon { get; }

        public IconButtonElement(Vector2? size = default,
            Vector2? iconSize = default,
            Sprite iconSprite = default,
            StandardSkin iconSkin = default)
            : base(size: size ?? DefaultButtonSize,
                normalSkin: StandardSkin.WhitePixel,
                hoverSkin: StandardSkin.WhitePixel,
                pressedSkin: StandardSkin.WhitePixel,
                normalColor: DefaultNormalButtonColor,
                hoverColor: DefaultHoverButtonColor,
                pressedColor: DefaultPressedButtonColor)
        {
            Icon = new SpriteElement(
                size: iconSize ?? DefaultIconSize,
                sprite: iconSprite,
                skin: iconSkin,
                sizeToSource: true,
                drawMode: DrawMode.Simple
            );

            InnerElement = new HolderElement(
                size: size ?? DefaultButtonSize,
                innerElement: new AlignmentElement(
                    alignmentFactor: Alignment.Center,
                    innerElement: Icon
                )
            );
        }
    }
}
