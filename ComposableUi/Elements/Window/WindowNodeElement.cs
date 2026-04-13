using System;
using System.Diagnostics;

using Microsoft.Xna.Framework;

namespace ComposableUi
{
    public class WindowNodeElement<T> : ResizableElement
        where T : Element
    {
        internal T RootContainer { get; private set; }

        internal ExpandedElement ViewHolder { get; }

        public T Container { get; private set; }

        private readonly AlignmentElement _overlayAlignment;
        private readonly ExpandedElement _overlayExpanded;

        public WindowNodeElement(Vector2? size = default,
            Vector2? minSize = default) 
        {
            MinSize = minSize ?? Vector2.Zero;

            ViewHolder = new ExpandedElement();

            _overlayAlignment = new AlignmentElement();
            _overlayExpanded = new ExpandedElement(_overlayAlignment);

            InnerElement = new ContainerElement(
                size: size,
                children: [
                    ViewHolder,
                    _overlayExpanded
                ]
            );
        }

        internal void ShowInOverlayAndAlignToEdge(Element element, Vector2 edgeNormal)
        {
            var alignmentFactor = Vector2.Max(Vector2.Zero, edgeNormal);

            _overlayExpanded.IsEnabled = true;
            _overlayExpanded.ExpandWidth = edgeNormal.Y != 0;
            _overlayExpanded.ExpandHeight = edgeNormal.X != 0;

            _overlayAlignment.InnerElement = element;
            _overlayAlignment.AlignmentFactor = alignmentFactor;
            _overlayAlignment.Pivot = alignmentFactor;
        }

        internal void HideOverlay()
        {
            _overlayExpanded.IsEnabled = false;
        }

        internal void SetSize(Vector2 size)
        {
            Size = size;

            if (InnerElement is not null)
                InnerElement.Size = size;
        }

        protected Element ResolveRootContainer()
            => RootContainer is not null ? RootContainer : this;

        protected Point CalculateClampedInParentDragDelta(Rectangle boundingRectangle,
            Rectangle parentBoundingRectangle)
        {
            parentBoundingRectangle.Location += new Point(20, 0);
            parentBoundingRectangle.Size -= new Point(20 * 2, 30);

            var delta = new Point()
            {
                X = Math.Min(parentBoundingRectangle.Right - boundingRectangle.Left, 0)
                    - Math.Min(boundingRectangle.Right - parentBoundingRectangle.Left, 0),
                Y = Math.Min(parentBoundingRectangle.Bottom - boundingRectangle.Top, 0)
                    - Math.Min(boundingRectangle.Top - parentBoundingRectangle.Top, 0)
            };

            return delta;
        }

        internal virtual void ApplyContainer(T container)
        {
            Container = container;
        }

        internal virtual void ApplyRootContainer(T root)
        {
            RootContainer = root;
        }

        internal virtual void PrepareReplacementWith(WindowNodeElement<T> node)
        {
            node.SetSize(Size);
            node.ApplyContainer(Container);
            node.ApplyRootContainer(RootContainer);
            node.IsInteractable = IsInteractable;
        }

        internal virtual void SetSelected(bool value) { }

        protected override void OnPointerFixedDrag(in PointerDragEvent pointerEvent)
        {
            if (HasEnabledInnerElement && Parent is not null)
            {
                var (sizeDelta, localPositionDelta) = CalculateResizeDeltas(pointerEvent.Delta.ToVector2());

                var boundingRectangle = BoundingRectangle;
                boundingRectangle.Size += sizeDelta.ToPoint();
                var sizedPivotOffset = boundingRectangle.Size.ToVector2() * Pivot;
                boundingRectangle.Location -= (sizedPivotOffset - PivotOffset - localPositionDelta).ToPoint();

                var clampedDelta = new Point()
                {
                    X = (int)(MathF.Abs(sizeDelta.X) * Math.Sign(pointerEvent.Delta.X)),
                    Y = (int)(MathF.Abs(sizeDelta.Y) * Math.Sign(pointerEvent.Delta.Y)),
                };
                clampedDelta += CalculateClampedInParentDragDelta(boundingRectangle, Parent.BoundingRectangle);

                var clampedPointerEvent = new PointerDragEvent(pointerEvent.Pointer, pointerEvent.Position,
                    pointerEvent.IsPrimaryButtonPressed, pointerEvent.IsSecondaryButtonPressed, clampedDelta);

                base.OnPointerFixedDrag(clampedPointerEvent);
            }
            else
            {
                base.OnPointerFixedDrag(pointerEvent);
            }
        }
    }
}
