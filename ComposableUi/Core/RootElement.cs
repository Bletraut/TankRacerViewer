using System;

using Microsoft.Xna.Framework;

namespace ComposableUi
{
    public sealed class RootElement : ContainerElement
    {
        internal bool IsDirty { get; private set; }

        internal void MarkAsDirty()
        {
            IsDirty = true;
        }

        public void ShowInOverlay(Element element,
            Vector2 position, Vector2 fallbackPosition,
            bool clampToRootWidth = true, bool clampToRootHeight = true)
        {
            element.IsEnabled = true;
            element.Layer = BuiltInLayer.Overlay;
            AddChild(element);

            element.Position = CalculateClampedPosition(element,
                position, fallbackPosition, clampToRootWidth, clampToRootHeight);
        }

        private Vector2 CalculateClampedPosition(Element element,
            Vector2 position, Vector2 fallbackPosition,
            bool clampToRootWidth, bool clampToRootHeight)
        {
            var shouldClamp = clampToRootWidth || clampToRootHeight;
            if (!shouldClamp)
                return position;

            var clampedPosition = position;

            var size = element.CalculatePreferredSize();
            var pivotOffset = size * element.Pivot;
            var boundingRectangle = new Rectangle((position - pivotOffset).ToPoint(), size.ToPoint());
            var rootBoundingRectangle = BoundingRectangle;

            if (clampToRootWidth)
            {
                var rightOverflow = boundingRectangle.Right - rootBoundingRectangle.Right;
                if (rightOverflow > 0)
                {
                    clampedPosition.X = fallbackPosition.X;
                    boundingRectangle.X = (int)(clampedPosition.X - pivotOffset.X);

                    var isOutOfBounds = rootBoundingRectangle.Left - boundingRectangle.Left > 0
                        || boundingRectangle.Right - rootBoundingRectangle.Right > 0;
                    if (isOutOfBounds)
                    {
                        clampedPosition.X = position.X - rightOverflow;
                        boundingRectangle.X = (int)(clampedPosition.X - pivotOffset.X);
                    }
                }

                clampedPosition.X += MathF.Max(0, rootBoundingRectangle.Left - boundingRectangle.Left)
                    + MathF.Min(0, rootBoundingRectangle.Right - boundingRectangle.Right);
            }

            if (clampToRootHeight)
            {
                var bottomOverflow = boundingRectangle.Bottom - rootBoundingRectangle.Bottom;
                if (bottomOverflow > 0)
                {
                    clampedPosition.Y = fallbackPosition.Y;
                    boundingRectangle.Y = (int)(clampedPosition.Y - pivotOffset.Y);

                    var isOutOfBounds = rootBoundingRectangle.Top - boundingRectangle.Top > 0
                        || boundingRectangle.Bottom - rootBoundingRectangle.Bottom > 0;
                    if (isOutOfBounds)
                    {
                        clampedPosition.Y = position.Y - bottomOverflow;
                        boundingRectangle.Y = (int)(clampedPosition.Y - pivotOffset.Y);
                    }
                }

                clampedPosition.Y += MathF.Max(0, rootBoundingRectangle.Top - boundingRectangle.Top)
                    + MathF.Min(0, rootBoundingRectangle.Bottom - boundingRectangle.Bottom);
            }

            return clampedPosition;
        }

        public override void Rebuild(Vector2 size, bool excludeChildren)
        {
            base.Rebuild(size, excludeChildren);

            IsDirty = false;
        }
    }
}
