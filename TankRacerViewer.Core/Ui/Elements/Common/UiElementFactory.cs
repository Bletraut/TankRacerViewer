using ComposableUi;

using Microsoft.Xna.Framework;

namespace TankRacerViewer.Core
{
    public static class UiElementFactory
    {
        public static ContentButtonElement CreateToggleButton(string text = default)
        {
            var toggle = new ContentButtonElement(
                iconSize: new Vector2(20),
                text: text,
                normalSkin: StandardSkin.RectanglePanel,
                hoverSkin: StandardSkin.RectanglePanel,
                pressedSkin: StandardSkin.RectanglePanel,
                disabledSkin: StandardSkin.RectanglePanel,
                hoverButtonColor: Color.LightCyan,
                pressedButtonColor: Color.DarkGoldenrod
            );
            toggle.Text.Color = Color.Black;

            return toggle;
        }
    }
}
