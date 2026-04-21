using Microsoft.Xna.Framework;

namespace ComposableUi
{
    public sealed class VerticalScrollBarElement : ScrollBarElement
    {
        public VerticalScrollBarElement(Vector2? size = null)
            : base(Vector2.UnitY, Vector2.UnitX, size)
        {
        }
    }
}
