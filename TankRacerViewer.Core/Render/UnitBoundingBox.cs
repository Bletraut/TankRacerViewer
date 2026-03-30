using Microsoft.Xna.Framework.Graphics;

namespace TankRacerViewer.Core
{
    public static class UnitBoundingBox
    {
        public static VertexPositionTexture[] Vertices { get; private set; } =
        [
            // Front.
            new(new(-1, 1, 1), new(0, 1)),
            new(new(1, 1, 1), new(1, 1)),
            new(new(-1, -1, 1), new(0, 0)),

            new(new(1, 1, 1), new(1, 1)),
            new(new(1, -1, 1), new(1, 0)),
            new(new(-1, -1, 1), new(0, 0)),

            // Back.
            new(new(1, 1, -1), new(0, 1)),
            new(new(-1, 1, -1), new(1, 1)),
            new(new(1, -1, -1), new(0, 0)),

            new(new(-1, 1, -1), new(1, 1)),
            new(new(-1, -1, -1), new(1, 0)),
            new(new(1, -1, -1), new(0, 0)),

            // Left.
            new(new(-1, 1, -1), new(0, 1)),
            new(new(-1, 1, 1), new(1, 1)),
            new(new(-1, -1, -1), new(0, 0)),

            new(new(-1, 1, 1), new(1, 1)),
            new(new(-1, -1, 1), new(1, 0)),
            new(new(-1, -1, -1), new(0, 0)),

            // Right.
            new(new(1, 1, 1), new(0, 1)),
            new(new(1, 1, -1), new(1, 1)),
            new(new(1, -1, 1), new(0, 0)),

            new(new(1, 1, -1), new(1, 1)),
            new(new(1, -1, -1), new(1, 0)),
            new(new(1, -1, 1), new(0, 0)),

            // Top.
            new(new(-1, 1, -1), new(0, 1)),
            new(new(1, 1, -1), new(1, 1)),
            new(new(-1, 1, 1), new(0, 0)),

            new(new(1, 1, -1), new(1, 1)),
            new(new(1, 1, 1), new(1, 0)),
            new(new(-1, 1, 1), new(0, 0)),

            //// Bottom.
            new(new(-1, -1, 1), new(0, 1)),
            new(new(1, -1, 1), new(1, 1)),
            new(new(-1, -1, -1), new(0, 0)),

            new(new(1, -1, 1), new(1, 1)),
            new(new(1, -1, -1), new(1, 0)),
            new(new(-1, -1, -1), new(0, 0)),
        ];
    }
}
