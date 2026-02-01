using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ComposableUi
{
    public interface IUiRenderer
    {
        public void Begin();
        public void End();

        public void DrawSprite(Sprite sprite, DrawMode drawMode,
            Rectangle destinationRectangle, Rectangle? clipMask, Color color);

        public void DrawSkinnedRectangle(StandardSkin skin, DrawMode drawMode,
            Rectangle destinationRectangle, Rectangle? clipMask, Color color);

        public void DrawString(SpriteFont spriteFont, string text,
            Vector2 position, Rectangle? clipMask, Color color);
    }
}
