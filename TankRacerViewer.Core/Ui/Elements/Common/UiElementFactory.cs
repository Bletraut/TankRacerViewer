using ComposableUi;

using Microsoft.Xna.Framework;

namespace TankRacerViewer.Core
{
    public static class UiElementFactory
    {
        public const float DefaultSpriteScale = 2;

        public static readonly Vector2 DefaultToggleIconSize = new(20);

        public static ContentButtonElement CreateToggleButton(string text = default)
        {
            var toggle = new ContentButtonElement(
                text: text,
                normalSprite: BuiltInSprite.DarkRectangle,
                hoverSprite: BuiltInSprite.HoverDarkRectangle,
                pressedSprite: BuiltInSprite.HoverDarkRectangle,
                disabledSprite: BuiltInSprite.HoverDarkRectangle,
                hoverButtonColor: Color.White,
                pressedButtonColor: Color.White,
                normalTextColor: Color.White,
                hoverTextColor: Color.Azure,
                pressedTextColor: Color.White
            );
            toggle.Icon.SizeToSource = false;
            toggle.Icon.Size = DefaultToggleIconSize;

            return toggle;
        }
    }
}
