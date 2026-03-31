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

            var size = element.CalculatePreferredSize();
            var pivotOffset = size * element.Pivot;
            var boundingRectangle = new Rectangle((position - pivotOffset).ToPoint(), size.ToPoint());
            var rootBoundingRectangle = BoundingRectangle;

            var fallbackTopLeftPosition = fallbackPosition - pivotOffset;

            if (clampToRootWidth)
            {
                if (boundingRectangle.Right > rootBoundingRectangle.Right)
                {
                    position.X = fallbackPosition.X;
                    boundingRectangle.X = (int)fallbackTopLeftPosition.X;
                }

                position.X += MathF.Max(0, rootBoundingRectangle.Left - boundingRectangle.Left)
                    + MathF.Min(0, rootBoundingRectangle.Right - boundingRectangle.Right);
            }
            if (clampToRootHeight)
            {
                if (boundingRectangle.Bottom > rootBoundingRectangle.Bottom)
                {
                    position.Y = fallbackPosition.Y;
                    boundingRectangle.Y = (int)fallbackTopLeftPosition.Y;
                }

                position.Y += MathF.Max(0, rootBoundingRectangle.Top - boundingRectangle.Top)
                    + MathF.Min(0, rootBoundingRectangle.Bottom - boundingRectangle.Bottom);
            }

            return position;
        }

        public override void Rebuild(Vector2 size, bool excludeChildren)
        {
            base.Rebuild(size, excludeChildren);

            IsDirty = false;
        }
    }
}
