using Microsoft.Xna.Framework;

namespace ComposableUi
{
    public interface IUiRenderer
    {
        public void DrawSprite(Sprite sprite, DrawMode drawMode,
            Rectangle destinationRectangle, Rectangle? clipMask, Color color);

        public void DrawSkinnedRectangle(StandardSkin skin, DrawMode drawMode,
            Rectangle destinationRectangle, Rectangle? clipMask, Color color);
    }
}
