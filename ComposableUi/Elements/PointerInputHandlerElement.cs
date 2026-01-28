using Microsoft.Xna.Framework;

using System;

namespace ComposableUi
{
    public class PointerInputHandlerElement : SizedToContentHolderElement,
        IPointerInputHandler
    {
        public virtual Rectangle InteractionRectangle => BoundingRectangle;

        private bool _blockInput;
        public bool BlockInput
        {
            get => _blockInput;
            set => SetAndChangeState(ref _blockInput, value);
        }

        private bool _isInteractable = true;
        public bool IsInteractable
        {
            get => _isInteractable;
            set
            {
                if (_isInteractable == value)
                    return;

                _isInteractable = value;
                OnInteractionChanged(_isInteractable);
            }
        }

        public event ElementEventHandler<bool> InteractionChanged;

        public event ElementEventHandler<(Point Position, int Delta)> ScrollWheel;
        public event ElementEventHandler<(Point Position, int Delta)> HorizontalScrollWheel;

        public event ElementEventHandler<Point> PointerEnter;
        public event ElementEventHandler<Point> PointerMove;
        public event ElementEventHandler<Point> PointerLeave;

        public event ElementEventHandler<Point> PointerDown;
        public event ElementEventHandler<Point> PointerUp;

        public event ElementEventHandler<Point> PointerSecondaryDown;
        public event ElementEventHandler<Point> PointerSecondaryUp;

        public event ElementEventHandler<(Point Position, Point Delta)> PointerDrag;
        public event ElementEventHandler<(Point Position, Point Delta)> PointerFixedDrag;

        public event ElementEventHandler<Point> PointerClick;
        public event ElementEventHandler<Point> PointerSecondaryClick;

        private Vector2 _pointerDownNormalizedPosition = Vector2.Zero;

        public PointerInputHandlerElement(Element innerElement = default,
            bool blockInput = true,
            bool isInteractable = true)
            : base(innerElement)
        {
            BlockInput = blockInput;
            IsInteractable = isInteractable;
        }

        void IPointerInputHandler.OnScrollWheel(Point position, int delta)
            => OnScrollWheel(position, delta);

        void IPointerInputHandler.OnHorizontalScrollWheel(Point position, int delta)
            => OnHorizontalScrollWheel(position, delta);

        void IPointerInputHandler.OnPointerEnter(Point position)
            => OnPointerEnter(position);
        void IPointerInputHandler.OnPointerMove(Point position)
            => OnPointerMove(position);
        void IPointerInputHandler.OnPointerLeave(Point position)
            => OnPointerLeave(position);

        void IPointerInputHandler.OnPointerDown(Point position)
            => OnPointerDown(position);
        void IPointerInputHandler.OnPointerUp(Point position)
            => OnPointerUp(position);

        void IPointerInputHandler.OnPointerSecondaryDown(Point position)
            => OnPointerSecondaryDown(position);
        void IPointerInputHandler.OnPointerSecondaryUp(Point position)
            => OnPointerSecondaryUp(position);

        void IPointerInputHandler.OnPointerDrag(Point position, Point delta)
            => OnPointerDrag(position, delta);

        void IPointerInputHandler.OnPointerClick(Point position)
            => OnPointerClick(position);
        void IPointerInputHandler.OnPointerSecondaryClick(Point position)
            => OnPointerSecondaryClick(position);

        protected virtual void OnInteractionChanged(bool value)
            => InteractionChanged?.Invoke(this, value);

        protected virtual void OnScrollWheel(Point position, int delta)
            => ScrollWheel?.Invoke(this, (position, delta));
        protected virtual void OnHorizontalScrollWheel(Point position, int delta)
            => HorizontalScrollWheel?.Invoke(this, (position, delta));

        protected virtual void OnPointerEnter(Point position)
            => PointerEnter?.Invoke(this, position);
        protected virtual void OnPointerMove(Point position)
            => PointerMove?.Invoke(this, position);
        protected virtual void OnPointerLeave(Point position)
            => PointerLeave?.Invoke(this, position);

        protected virtual void OnPointerDown(Point position)
        {
            PointerDown?.Invoke(this, position);

            var boundingBox = BoundingRectangle;
            var localPointerPosition = position - new Point(boundingBox.Left, boundingBox.Top);

            _pointerDownNormalizedPosition = localPointerPosition.ToVector2() / Size;
        }
        protected virtual void OnPointerUp(Point position)
            => PointerUp?.Invoke(this, position);

        protected virtual void OnPointerSecondaryDown(Point position)
            => PointerSecondaryDown?.Invoke(this, position);
        protected virtual void OnPointerSecondaryUp(Point position)
            => PointerSecondaryUp?.Invoke(this, position);

        protected virtual void OnPointerDrag(Point position, Point delta)
        {
            PointerDrag?.Invoke(this, (position, delta));

            var boundingBox = BoundingRectangle;
            var pointerDownPosition = new Vector2(boundingBox.Left, boundingBox.Top)
                + Size * _pointerDownNormalizedPosition;

            var vectorPosition = position.ToVector2();
            var axisXPointerPosition = Vector2.Dot(Vector2.UnitX, vectorPosition * Vector2.UnitX);
            var axisXPointerDownPosition = Vector2.Dot(Vector2.UnitX, pointerDownPosition * Vector2.UnitX);
            var axisYPointerPosition = Vector2.Dot(Vector2.UnitY, vectorPosition * Vector2.UnitY);
            var axisYPointerDownPosition = Vector2.Dot(Vector2.UnitY, pointerDownPosition * Vector2.UnitY);

            var vectorDelta = delta.ToVector2();
            var axisXDelta = Vector2.Dot(Vector2.UnitX, vectorDelta);
            var axisYDelta = Vector2.Dot(Vector2.UnitY, vectorDelta);

            var canDragX = MathF.Sign(axisXPointerPosition - axisXPointerDownPosition) == MathF.Sign(axisXDelta);
            var canDragY = MathF.Sign(axisYPointerPosition - axisYPointerDownPosition) == MathF.Sign(axisYDelta);

            if (!canDragX && !canDragY)
                return;

            delta = new Point()
            {
                X = canDragX ? delta.X : 0,
                Y = canDragY ? delta.Y : 0
            };
            OnPointerFixedDrag(position, delta);
        }
        protected virtual void OnPointerFixedDrag(Point position, Point delta)
            => PointerFixedDrag?.Invoke(this, (position, delta));

        protected virtual void OnPointerClick(Point position)
            => PointerClick?.Invoke(this, position);
        protected virtual void OnPointerSecondaryClick(Point position)
            => PointerSecondaryClick?.Invoke(this, position);
    }
}
