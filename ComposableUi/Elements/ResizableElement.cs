using ComposableUi.Utilities;

using Microsoft.Xna.Framework;

namespace ComposableUi
{
    public class ResizableElement : PointerInputHandlerElement,
        IPointerInputHandler
    {
        public override Rectangle InteractionRectangle
        {
            get
            {
                var boundingRectangle = BoundingRectangle;
                return new Rectangle()
                {
                    X = boundingRectangle.Left - HandleSize,
                    Y = boundingRectangle.Top - HandleSize,
                    Width = boundingRectangle.Width + HandleSize * 2,
                    Height = boundingRectangle.Height + HandleSize * 2
                };
            }
        }

        private int _handleSize;
        public int HandleSize
        {
            get => _handleSize;
            set => SetAndChangeState(ref _handleSize, value);
        }

        private Vector2 _minSize;
        public Vector2 MinSize
        {
            get => _minSize;
            set => SetAndChangeState(ref _minSize, value);
        }

        private Vector2 _sizeDirection = Vector2.Zero;
        private Vector2 _positionDirection = Vector2.Zero;

        public ResizableElement(Element innerElement = default,
            int handleSize = 8,
            Vector2 minSize = default,
            bool blockInput = true,
            bool isInteractable = true)
            : base(innerElement,
                  blockInput,
                  isInteractable)
        {
            HandleSize = handleSize;
            MinSize = minSize;
        }

        protected override void OnPointerDown(Point position)
        {
            base.OnPointerDown(position);

            _sizeDirection = InteractionRectangle.GetEdgeNormal(HandleSize, position);
            _positionDirection = Vector2.Max(Vector2.Zero, -_sizeDirection) - Pivot;
        }

        protected override void OnPointerFixedDrag(Point position, Point delta)
        {
            base.OnPointerFixedDrag(position, delta);

            var deltaVector = delta.ToVector2();
            if (HasActiveInnerElement)
            {
                var size = InnerElement.Size;
                InnerElement.Size = Vector2.Max(size + deltaVector * _sizeDirection, _minSize);
                LocalPosition += (size - InnerElement.Size) * _positionDirection;
            }
        }
    }
}
