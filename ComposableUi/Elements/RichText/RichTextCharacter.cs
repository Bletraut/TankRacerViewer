using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ComposableUi
{
    public readonly record struct RichTextCharacter(int TokenIndex, TokenType TokenType,
        Vector2 Position, Texture2D FontTexture, SpriteFont.Glyph Glyph);
}
