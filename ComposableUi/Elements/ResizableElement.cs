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

            _sizeDirection = Vector2.Zero;
            _positionDirection = Vector2.Zero;

            var interactionRectangle = InteractionRectangle;

            // Left
            var edgeRectangle = new Rectangle()
            {
                X = InteractionRectangle.Left,
                Y = InteractionRectangle.Top,
                Width = HandleSize,
                Height = InteractionRectangle.Height
            };
            if (edgeRectangle.Contains(position))
            {
                _sizeDirection = _sizeDirection with { X = -1 };
                _positionDirection = _positionDirection with { X = 1 - Pivot.X };
            }

            // Right
            edgeRectangle = new Rectangle()
            {
                X = InteractionRectangle.Right - HandleSize,
                Y = InteractionRectangle.Top,
                Width = HandleSize,
                Height = InteractionRectangle.Height
            };
            if (edgeRectangle.Contains(position))
            {
                _sizeDirection = _sizeDirection with { X = 1 };
                _positionDirection = _positionDirection with { X = -Pivot.X };
            }

            // Top
            edgeRectangle = new Rectangle()
            {
                X = InteractionRectangle.Left,
                Y = InteractionRectangle.Top,
                Width = interactionRectangle.Width,
                Height = HandleSize
            };
            if (edgeRectangle.Contains(position))
            {
                _sizeDirection = _sizeDirection with { Y = -1 };
                _positionDirection = _positionDirection with { Y = 1 - Pivot.Y };
            }

            // Bottom
            edgeRectangle = new Rectangle()
            {
                X = InteractionRectangle.Left,
                Y = InteractionRectangle.Bottom - HandleSize,
                Width = interactionRectangle.Width,
                Height = HandleSize
            };
            if (edgeRectangle.Contains(position))
            {
                _sizeDirection = _sizeDirection with { Y = 1 };
                _positionDirection = _positionDirection with { Y = -Pivot.Y };
            }
        }

        protected override void OnPointerDrag(Point position, Point delta)
        {
            base.OnPointerDrag(position, delta);

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
