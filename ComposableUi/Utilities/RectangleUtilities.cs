using System;

using Microsoft.Xna.Framework;

namespace ComposableUi.Utilities
{
    public static class RectangleUtilities
    {
        public static Vector2 GetEdgeNormal(this Rectangle rectangle,
            int thickness, Point point, EdgeNormalResolveMode resolveMode = default)
            => GetEdgeNormal(rectangle, new Point(thickness), point, resolveMode);
        public static Vector2 GetEdgeNormal(this Rectangle rectangle,
            Point thickness, Point point, EdgeNormalResolveMode resolveMode)
        {
            thickness.X = int.Clamp(thickness.X, 0, rectangle.Width / 2);
            thickness.Y = int.Clamp(thickness.Y, 0, rectangle.Height / 2);

            var normal = Vector2.Zero;

            // Left
            var edgeRectangle = new Rectangle()
            {
                X = rectangle.Left,
                Y = rectangle.Top,
                Width = thickness.X,
                Height = rectangle.Height
            };
            if (edgeRectangle.Contains(point))
                normal = normal with { X = -1 };

            // Right
            edgeRectangle = new Rectangle()
            {
                X = rectangle.Right - thickness.X,
                Y = rectangle.Top,
                Width = thickness.X,
                Height = rectangle.Height
            };
            if (edgeRectangle.Contains(point))
                normal = normal with { X = 1 };

            // Top
            edgeRectangle = new Rectangle()
            {
                X = rectangle.Left,
                Y = rectangle.Top,
                Width = rectangle.Width,
                Height = thickness.Y
            };
            if (edgeRectangle.Contains(point))
                normal = normal with { Y = -1 };

            // Bottom
            edgeRectangle = new Rectangle()
            {
                X = rectangle.Left,
                Y = rectangle.Bottom - thickness.Y,
                Width = rectangle.Width,
                Height = thickness.Y
            };
            if (edgeRectangle.Contains(point))
                normal = normal with { Y = 1 };

            switch (resolveMode)
            {
                case EdgeNormalResolveMode.AllowDiagonal:
                    break;
                case EdgeNormalResolveMode.PreferX:
                    normal.Y = normal.X != 0 ? 0 : normal.Y;
                    break;
                case EdgeNormalResolveMode.PreferY:
                    normal.X = normal.Y != 0 ? 0 : normal.X;
                    break;
                default:
                    throw new NotImplementedException();
            }

            return normal;
        }

        public enum EdgeNormalResolveMode
        {
            AllowDiagonal,
            PreferX,
            PreferY
        }
    }
}
