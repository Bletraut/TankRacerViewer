using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ComposableUi
{
    public readonly record struct RenderCommand(int Id, int Type,
        Rectangle BoundingRectangle, Rectangle? ClipMask, Texture Texture);
}
