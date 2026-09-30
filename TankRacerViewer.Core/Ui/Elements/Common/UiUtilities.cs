using ComposableUi;

namespace TankRacerViewer.Core
{
    public static class UiUtilities
    {
        public static void SetToggle(this ContentButtonElement toggle,
            bool isActive)
        {
            if (isActive)
            {
                toggle.NormalSprite = BuiltInSprite.SoftDarkRectangle;
                toggle.HoverSprite = BuiltInSprite.HoverSoftDarkRectangle;
                toggle.PressedSprite = BuiltInSprite.HoverSoftDarkRectangle;
                toggle.DisabledSprite = BuiltInSprite.HoverSoftDarkRectangle;
            }
            else
            {
                toggle.NormalSprite = BuiltInSprite.DarkRectangle;
                toggle.HoverSprite = BuiltInSprite.HoverDarkRectangle;
                toggle.PressedSprite = BuiltInSprite.HoverDarkRectangle;
                toggle.DisabledSprite = BuiltInSprite.HoverDarkRectangle;
            }
        }
    }
}
