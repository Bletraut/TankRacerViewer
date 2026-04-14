using System;

using Microsoft.Xna.Framework;

namespace ComposableUi
{
    public class PointerInputHandlerElement : PointerInputHandlerElement<PointerInputHandlerElement>
    {
        public PointerInputHandlerElement(Element innerElement = default,
            bool blockInput = true,
            bool isInteractable = true)
            : base(innerElement,
                  blockInput,
                  isInteractable)
        {
        }
    }

    public class PointerInputHandlerElement<TSelf> : SizedToContentHolderElement,
        IPointerInputHandler
        where TSelf : PointerInputHandlerElement<TSelf>
    {
        public virtual Rectangle InteractionRectangle => BoundingRectangle;

        public virtual Rectangle ClippedInteractionRectangle
        {
            get
            {
                var interactionRectangle = InteractionRectangle;

                var clipMask = ClipMask;
                if (clipMask.HasValue)
                    interactionRectangle = Rectangle.Intersect(interactionRectangle, clipMask.Value);

                return interactionRectangle;
            }
        }

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

        public bool IsFocused { get; private set; }

        public bool IsHover { get; private set; }

        public event ElementEventHandler<TSelf, bool> InteractionChanged;

        public event ElementEventHandler<TSelf, PointerScrollEvent> ScrollWheel;
        public event ElementEventHandler<TSelf, PointerScrollEvent> HorizontalScrollWheel;

        public event ElementEventHandler<TSelf, PointerEvent> PointerEnter;
        public event ElementEventHandler<TSelf, PointerEvent> PointerMove;
        public event ElementEventHandler<TSelf, PointerEvent> PointerLeave;

        public event ElementEventHandler<TSelf, PointerEvent> PointerDown;
        public event ElementEventHandler<TSelf, PointerEvent> PointerUp;

        public event ElementEventHandler<TSelf, PointerEvent> PointerSecondaryDown;
        public event ElementEventHandler<TSelf, PointerEvent> PointerSecondaryUp;

        public event ElementEventHandler<TSelf, PointerDragEvent> PointerDrag;
        public event ElementEventHandler<TSelf, PointerDragEvent> PointerFixedDrag;

        public event ElementEventHandler<TSelf, PointerEvent> PointerClick;
        public event ElementEventHandler<TSelf, PointerEvent> PointerSecondaryClick;

        public event ElementEventHandler<TSelf, PointerFocusEvent> FocusChanged;

        private Vector2 _pointerDownNormalizedPosition = Vector2.Zero;

        public PointerInputHandlerElement(Element innerElement = default,
            bool blockInput = true,
            bool isInteractable = true)
            : base(innerElement)
        {
            BlockInput = blockInput;
            IsInteractable = isInteractable;
        }

        public Point CalculateFixedDragDelta(Rectangle boundingRectangle,
            Point delta, Point position)
        {
            var pointerDownPosition = new Vector2(boundingRectangle.Left, boundingRectangle.Top)
                + Size * _pointerDownNormalizedPosition;

            var vectorDelta = delta.ToVector2();
            var vectorPosition = position.ToVector2();

            var canDragX = MathF.Sign(vectorPosition.X - pointerDownPosition.X) == MathF.Sign(vectorDelta.X);
            var canDragY = MathF.Sign(vectorPosition.Y - pointerDownPosition.Y) == MathF.Sign(vectorDelta.Y);

            if (!canDragX && !canDragY)
                return Point.Zero;

            return new Point()
            {
                X = canDragX ? delta.X : 0,
                Y = canDragY ? delta.Y : 0
            };
        }

        void IPointerInputHandler.OnScrollWheel(in PointerScrollEvent pointerEvent)
            => OnScrollWheel(pointerEvent);

        void IPointerInputHandler.OnHorizontalScrollWheel(in PointerScrollEvent pointerEvent)
            => OnHorizontalScrollWheel(pointerEvent);

        void IPointerInputHandler.OnPointerEnter(in PointerEvent pointerEvent)
            => OnPointerEnter(pointerEvent);
        void IPointerInputHandler.OnPointerMove(in PointerEvent pointerEvent)
            => OnPointerMove(pointerEvent);
        void IPointerInputHandler.OnPointerLeave(in PointerEvent pointerEvent)
            => OnPointerLeave(pointerEvent);

        void IPointerInputHandler.OnPointerDown(in PointerEvent pointerEvent)
            => OnPointerDown(pointerEvent);
        void IPointerInputHandler.OnPointerUp(in PointerEvent pointerEvent)
            => OnPointerUp(pointerEvent);

        void IPointerInputHandler.OnPointerSecondaryDown(in PointerEvent pointerEvent)
            => OnPointerSecondaryDown(pointerEvent);
        void IPointerInputHandler.OnPointerSecondaryUp(in PointerEvent pointerEvent)
            => OnPointerSecondaryUp(pointerEvent);

        void IPointerInputHandler.OnPointerDrag(in PointerDragEvent pointerEvent)
            => OnPointerDrag(pointerEvent);

        void IPointerInputHandler.OnPointerClick(in PointerEvent pointerEvent)
            => OnPointerClick(pointerEvent);
        void IPointerInputHandler.OnPointerSecondaryClick(in PointerEvent pointerEvent)
            => OnPointerSecondaryClick(pointerEvent);

        void IPointerInputHandler.OnFocusChanged(in PointerFocusEvent pointerEvent)
            => OnFocusChanged(pointerEvent);

        protected virtual void OnInteractionChanged(bool value)
            => InteractionChanged?.Invoke((TSelf)this, value);

        protected virtual void OnScrollWheel(in PointerScrollEvent pointerEvent)
            => ScrollWheel?.Invoke((TSelf)this, pointerEvent);
        protected virtual void OnHorizontalScrollWheel(in PointerScrollEvent pointerEvent)
            => HorizontalScrollWheel?.Invoke((TSelf)this, pointerEvent);

        protected virtual void OnPointerEnter(in PointerEvent pointerEvent)
        {
            IsHover = true;
            PointerEnter?.Invoke((TSelf)this, pointerEvent);
        }
        protected virtual void OnPointerMove(in PointerEvent pointerEvent)
            => PointerMove?.Invoke((TSelf)this, pointerEvent);
        protected virtual void OnPointerLeave(in PointerEvent pointerEvent)
        {
            IsHover = false;
            PointerLeave?.Invoke((TSelf)this, pointerEvent);
        }

        protected virtual void OnPointerDown(in PointerEvent pointerEvent)
        {
            PointerDown?.Invoke((TSelf)this, pointerEvent);

            var boundingBox = BoundingRectangle;
            var localPointerPosition = pointerEvent.Position - new Point(boundingBox.Left, boundingBox.Top);

            _pointerDownNormalizedPosition = localPointerPosition.ToVector2() / Size;
        }
        protected virtual void OnPointerUp(in PointerEvent pointerEvent)
            => PointerUp?.Invoke((TSelf)this, pointerEvent);

        protected virtual void OnPointerSecondaryDown(in PointerEvent pointerEvent)
            => PointerSecondaryDown?.Invoke((TSelf)this, pointerEvent);
        protected virtual void OnPointerSecondaryUp(in PointerEvent pointerEvent)
            => PointerSecondaryUp?.Invoke((TSelf)this, pointerEvent);

        protected virtual void OnPointerDrag(in PointerDragEvent pointerEvent)
        {
            PointerDrag?.Invoke((TSelf)this, pointerEvent);

            var delta = CalculateFixedDragDelta(BoundingRectangle,
                pointerEvent.Delta, pointerEvent.Position);
            if (delta == Point.Zero)
                return;

            OnPointerFixedDrag(new PointerDragEvent(pointerEvent.Pointer, pointerEvent.Position,
                pointerEvent.IsPrimaryButtonPressed, pointerEvent.IsSecondaryButtonPressed, delta));
        }
        protected virtual void OnPointerFixedDrag(in PointerDragEvent pointerEvent)
            => PointerFixedDrag?.Invoke((TSelf)this, pointerEvent);

        protected virtual void OnPointerClick(in PointerEvent pointerEvent)
            => PointerClick?.Invoke((TSelf)this, pointerEvent);
        protected virtual void OnPointerSecondaryClick(in PointerEvent pointerEvent)
            => PointerSecondaryClick?.Invoke((TSelf)this, pointerEvent);

        protected virtual void OnFocusChanged(in PointerFocusEvent pointerEvent)
        {
            IsFocused = pointerEvent.IsFocused;
            FocusChanged?.Invoke((TSelf)this, pointerEvent);
        }
    }
}
