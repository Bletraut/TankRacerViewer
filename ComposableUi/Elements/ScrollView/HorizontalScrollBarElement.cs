using Microsoft.Xna.Framework;

namespace ComposableUi
{
    public sealed class HorizontalScrollBarElement : ScrollBarElement
    {
        public HorizontalScrollBarElement(Vector2? size = default) 
            : base(Vector2.UnitX, Vector2.UnitY, size)
        {
        }
    }
}
