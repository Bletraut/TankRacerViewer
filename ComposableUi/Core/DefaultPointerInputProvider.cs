using Microsoft.Xna.Framework;

namespace ComposableUi.Core
{
    public sealed class DefaultPointerInputProvider : IPointerInputProvider
    {
        Vector2 IPointerInputProvider.PointerPosition => Vector2.Zero;

        public void Update() { }
    }
}
