using System;

using Microsoft.Xna.Framework;

namespace ComposableUi
{
    public class WindowNodeElement<T> : ResizableElement
        where T : Element
    {
        public const int DefaultLeftConstraintInset = 20;
        public const int DefaultRightConstraintInset = 20;
        public const int DefaultTopConstraintInset = 0;
        public const int DefaultBottomConstraintInset = 30;

        internal T RootContainer { get; private set; }

        internal ExpandedElement ViewHolder { get; }

        public T Container { get; private set; }

        private bool _constrainToParent;
        public bool ConstrainToParent
        {
            get => _constrainToParent;
            set => SetAndChangeState(ref _constrainToParent, value);
        }

        private int _leftConstraintInset;
        public int LeftConstraintInset
        {
            get => _leftConstraintInset;
            set => SetAndChangeState(ref _leftConstraintInset, value);
        }
        private int _rightConstraintInset;
        public int RightConstraintInset
        {
            get => _rightConstraintInset;
            set => SetAndChangeState(ref _rightConstraintInset, value);
        }
        private int _topConstraintInset;
        public int TopConstraintInset
        {
            get => _topConstraintInset;
            set => SetAndChangeState(ref _topConstraintInset, value);
        }
        private int _bottomConstraintInset;
        public int BottomConstraintInset
        {
            get => _bottomConstraintInset;
            set => SetAndChangeState(ref _bottomConstraintInset, value);
        }

        private readonly AlignmentElement _overlayAlignment;
        private readonly ExpandedElement _overlayExpanded;

        public WindowNodeElement(Vector2? size = default,
            Vector2? minSize = default,
            bool constrainToParent = true,
            int leftConstraintInset = DefaultLeftConstraintInset,
            int rightConstraintInset = DefaultRightConstraintInset,
            int topConstraintInset = DefaultTopConstraintInset,
            int bottomConstraintInset = DefaultBottomConstraintInset)
        {
            MinSize = minSize ?? Vector2.Zero;
            ConstrainToParent = constrainToParent;

            LeftConstraintInset = leftConstraintInset;
            RightConstraintInset = rightConstraintInset;
            TopConstraintInset = topConstraintInset;
            BottomConstraintInset = bottomConstraintInset;

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

        protected Point CalculateOffsetToConstrainToParent(Rectangle boundingRectangle,
            Rectangle parentBoundingRectangle)
        {
            parentBoundingRectangle.Location += new Point(LeftConstraintInset, TopConstraintInset);
            parentBoundingRectangle.Size -= new Point()
            {
                X = LeftConstraintInset + RightConstraintInset,
                Y = TopConstraintInset + BottomConstraintInset
            };

            var offset = new Point()
            {
                X = Math.Min(parentBoundingRectangle.Right - boundingRectangle.Left, 0)
                    - Math.Min(boundingRectangle.Right - parentBoundingRectangle.Left, 0),
                Y = Math.Min(parentBoundingRectangle.Bottom - boundingRectangle.Top, 0)
                    - Math.Min(boundingRectangle.Top - parentBoundingRectangle.Top, 0)
            };

            return offset;
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

        public override void Rebuild(Vector2 size, bool excludeChildren)
        {
            base.Rebuild(size, excludeChildren);

            var shouldConstrainToParent = ConstrainToParent
                && Parent is not null;
            if (shouldConstrainToParent)
            {
                var offset = CalculateOffsetToConstrainToParent(BoundingRectangle,
                    Parent.BoundingRectangle);
                if (offset == Point.Zero)
                    return;

                Position += offset.ToVector2();
            }
        }

        protected override void OnPointerFixedDrag(in PointerDragEvent pointerEvent)
        {
            var shouldConstrainToParent = ConstrainToParent
                && HasEnabledInnerElement
                && Parent is not null;
            if (shouldConstrainToParent)
            {
                var (sizeDelta, localPositionDelta) = CalculateResizeDeltas(pointerEvent.Delta.ToVector2());

                var boundingRectangle = BoundingRectangle;
                boundingRectangle.Size += sizeDelta.ToPoint();
                var sizedPivotOffset = (Size + sizeDelta) * Pivot;
                boundingRectangle.Location -= (sizedPivotOffset - PivotOffset - localPositionDelta).ToPoint();

                var constrainedDelta = new Point()
                {
                    X = (int)(MathF.Abs(sizeDelta.X) * Math.Sign(pointerEvent.Delta.X)),
                    Y = (int)(MathF.Abs(sizeDelta.Y) * Math.Sign(pointerEvent.Delta.Y)),
                };
                constrainedDelta += CalculateOffsetToConstrainToParent(boundingRectangle, Parent.BoundingRectangle);

                var constrainPointerEvent = new PointerDragEvent(pointerEvent.Pointer, pointerEvent.Position,
                    pointerEvent.IsPrimaryButtonPressed, pointerEvent.IsSecondaryButtonPressed, constrainedDelta);

                base.OnPointerFixedDrag(constrainPointerEvent);
            }
            else
            {
                base.OnPointerFixedDrag(pointerEvent);
            }
        }
    }
}
