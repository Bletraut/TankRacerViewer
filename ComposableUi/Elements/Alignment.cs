using Microsoft.Xna.Framework;

namespace ComposableUi
{
    public static class Alignment
    {
        public static readonly Vector2 TopLeft = new(0f, 0f);
        public static readonly Vector2 TopCenter = new(0.5f, 0f);
        public static readonly Vector2 TopRight = new(1f, 0f);

        public static readonly Vector2 MiddleLeft = new(0f, 0.5f);
        public static readonly Vector2 Center = new(0.5f, 0.5f);
        public static readonly Vector2 MiddleRight = new(1f, 0.5f);

        public static readonly Vector2 BottomLeft = new(0f, 1f);
        public static readonly Vector2 BottomCenter = new(0.5f, 1f);
        public static readonly Vector2 BottomRight = new(1f, 1f);
    }
}
