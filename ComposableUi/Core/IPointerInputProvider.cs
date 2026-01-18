using Microsoft.Xna.Framework;

namespace ComposableUi
{
    public interface IPointerInputProvider
    {
        Vector2 PointerPosition { get; }
    }
}
