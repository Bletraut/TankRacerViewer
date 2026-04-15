using ComposableUi;

namespace TankRacerViewer.Core
{
    public static class UiUtilities
    {
        public static void SetIcon(this WindowElement window, string iconName)
        {
            window.Tab.Icon.Sprite = IconCollection.Get(iconName);
            window.Tab.Icon.Size = window.Tab.Icon.Sprite.SourceRectangle.Size.ToVector2() * 2;
        }
    }
}
