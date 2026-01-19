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

        private readonly RasterizerState _scissorRasterizerState;

        public DefaultUiRenderer(ContentManager contentManager, SpriteBatch spriteBatch)
        {
            _contentManager = contentManager;
            _spriteBatch = spriteBatch;

            if (WhitePixelTexture == null)
            {
                WhitePixelTexture = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
                WhitePixelTexture.SetData([Color.White]);
            }

            _scissorRasterizerState = new RasterizerState()
            {
                ScissorTestEnable = true
            };
        }

        void IUiRenderer.DrawRectangle(Rectangle boundingRectangle,
            Rectangle? clipMask, Color color)
        {
            RasterizerState rasterizerState = null;
            if (clipMask.HasValue)
            {
                rasterizerState = _scissorRasterizerState;
                _spriteBatch.GraphicsDevice.ScissorRectangle = clipMask.Value;
            }

            _spriteBatch.Begin(rasterizerState: rasterizerState);

            _spriteBatch.Draw(WhitePixelTexture, boundingRectangle, color);

            // Debug.
            var center = new Vector2(boundingRectangle.Left, boundingRectangle.Top) 
                + boundingRectangle.Size.ToVector2() / 2;
            _spriteBatch.Draw(WhitePixelTexture, center - Vector2.One * 3f,
                null, Color.Red, 0, Vector2.Zero, Vector2.One * 8, SpriteEffects.None, 0);
            _spriteBatch.Draw(WhitePixelTexture, new Vector2(boundingRectangle.Left + 4, boundingRectangle.Center.Y),
                null, Color.Blue, 0, Vector2.Zero, new Vector2(boundingRectangle.Width - 8, 2), SpriteEffects.None, 0);
            _spriteBatch.Draw(WhitePixelTexture, new Vector2(center.X, boundingRectangle.Top + 4),
                null, Color.Green, 0, Vector2.Zero, new Vector2(2, boundingRectangle.Height - 8), SpriteEffects.None, 0);

            _spriteBatch.End();
        }
    }
}
