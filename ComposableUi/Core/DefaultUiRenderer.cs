using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ComposableUi.Core
{
    public sealed class DefaultUiRenderer : IUiRenderer
    {
        private static Texture2D WhitePixelTexture;

        private readonly ContentManager _contentManager;
        private readonly SpriteBatch _spriteBatch;

        public DefaultUiRenderer(ContentManager contentManager, SpriteBatch spriteBatch)
        {
            _contentManager = contentManager;
            _spriteBatch = spriteBatch;

            if (WhitePixelTexture == null)
            {
                WhitePixelTexture = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
                WhitePixelTexture.SetData([Color.White]);
            }
        }

        void IUiRenderer.DrawRectangle(Rectangle rectangle, Color color)
        {
            _spriteBatch.Begin();

            _spriteBatch.Draw(WhitePixelTexture, rectangle, color);

            // Debug.
            var center = new Vector2(rectangle.Left, rectangle.Top) + rectangle.Size.ToVector2() / 2;
            _spriteBatch.Draw(WhitePixelTexture, center - Vector2.One * 3f,
                null, Color.Red, 0, Vector2.Zero, Vector2.One * 8, SpriteEffects.None, 0);
            _spriteBatch.Draw(WhitePixelTexture, new Vector2(rectangle.Left + 4, rectangle.Center.Y),
                null, Color.Blue, 0, Vector2.Zero, new Vector2(rectangle.Width - 8, 2), SpriteEffects.None, 0);
            _spriteBatch.Draw(WhitePixelTexture, new Vector2(center.X, rectangle.Top + 4),
                null, Color.Green, 0, Vector2.Zero, new Vector2(2, rectangle.Height - 8), SpriteEffects.None, 0);

            _spriteBatch.End();
        }
    }
}
