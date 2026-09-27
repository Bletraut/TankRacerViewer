using Microsoft.Xna.Framework.Graphics;

namespace ComposableUi
{
    public sealed class Theme
    {
        public required SpriteFont DefaultSpriteFont { get; set; }

        public SpriteResolver SpriteResolver { get; } = new();
    }
}
