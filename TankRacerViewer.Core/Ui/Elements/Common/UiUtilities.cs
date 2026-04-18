using ComposableUi;

namespace TankRacerViewer.Core
{
    public static class UiUtilities
    {
        public static void SetScaledIcon(this WindowElement window, string iconName, float scale)
            => SetScaledSprite(window.Tab.Icon, iconName, scale);

        public static void SetScaledSprite(this SpriteElement spriteElement, string iconName, float scale)
        {
            spriteElement.Sprite = IconCollection.Get(iconName);
            spriteElement.Size = spriteElement.Sprite.SourceRectangle.Size.ToVector2() * scale;
        }

        public static void SetToggle(this ContentButtonElement toggle,
            bool isActive)
        {
            if (isActive)
            {
                toggle.NormalSkin = StandardSkin.SoftDarkRectangle;
                toggle.HoverSkin = StandardSkin.HoverSoftDarkRectangle;
                toggle.PressedSkin = StandardSkin.HoverSoftDarkRectangle;
                toggle.DisabledSkin = StandardSkin.HoverSoftDarkRectangle;
            }
            else
            {
                toggle.NormalSkin = StandardSkin.DarkRectangle;
                toggle.HoverSkin = StandardSkin.HoverDarkRectangle;
                toggle.PressedSkin = StandardSkin.HoverDarkRectangle;
                toggle.DisabledSkin = StandardSkin.HoverDarkRectangle;
            }
        }
    }
}
