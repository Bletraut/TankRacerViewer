using Microsoft.Xna.Framework;

using System.Text;

namespace ComposableUi
{
    public interface IUiRenderer
    {
        public Vector2 MeasureString(string text);
        public Vector2 MeasureString(StringBuilder text);

        public void DrawRectangle(Rectangle boundingRectangle,
            Rectangle? clipMask, Color color);

        public void DrawSprite(Sprite sprite, DrawMode drawMode,
            Rectangle boundingRectangle, Rectangle? clipMask, Color color);

        public void DrawSkinnedRectangle(StandardSkin skin, DrawMode drawMode,
            Rectangle boundingRectangle, Rectangle? clipMask, Color color);
    }
}
