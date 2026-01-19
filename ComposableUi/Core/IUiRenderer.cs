using Microsoft.Xna.Framework;

namespace ComposableUi
{
    public interface IUiRenderer
    {
        public void DrawRectangle(Rectangle boundingRectangle,
            Rectangle? clipMask, Color color);
    }
}
