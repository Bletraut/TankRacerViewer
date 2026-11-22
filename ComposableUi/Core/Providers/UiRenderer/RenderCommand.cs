using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ComposableUi
{
    internal readonly record struct RenderCommand(int StartIndex, int Length, RenderCommandType Type,
        Rectangle BoundingRectangle, Rectangle? ClipMask, Texture Texture)
        : IRenderCommand<RenderCommand>
    {
        bool IRenderCommand<RenderCommand>.CanBatchWith(in RenderCommand otherCommand)
        {
            return Type == otherCommand.Type
                && ClipMask == otherCommand.ClipMask
                && Texture == otherCommand.Texture;
        }
    }
}
