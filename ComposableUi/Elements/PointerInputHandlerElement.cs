using System;

using Microsoft.Xna.Framework;

namespace ComposableUi
{
    public class PointerInputHandlerElement : SizedToContentHolderElement,
        IPointerInputHandler
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

        public event ElementEventHandler<PointerInputHandlerElement, bool> InteractionChanged;

        public event ElementEventHandler<PointerInputHandlerElement, PointerScrollEvent> ScrollWheel;
        public event ElementEventHandler<PointerInputHandlerElement, PointerScrollEvent> HorizontalScrollWheel;

        public event ElementEventHandler<PointerInputHandlerElement, PointerEvent> PointerEnter;
        public event ElementEventHandler<PointerInputHandlerElement, PointerEvent> PointerMove;
        public event ElementEventHandler<PointerInputHandlerElement, PointerEvent> PointerLeave;

        public event ElementEventHandler<PointerInputHandlerElement, PointerEvent> PointerDown;
        public event ElementEventHandler<PointerInputHandlerElement, PointerEvent> PointerUp;

        public event ElementEventHandler<PointerInputHandlerElement, PointerEvent> PointerSecondaryDown;
        public event ElementEventHandler<PointerInputHandlerElement, PointerEvent> PointerSecondaryUp;

        public event ElementEventHandler<PointerInputHandlerElement, PointerDragEvent> PointerDrag;
        public event ElementEventHandler<PointerInputHandlerElement, PointerDragEvent> PointerFixedDrag;

        public event ElementEventHandler<PointerInputHandlerElement, PointerEvent> PointerClick;
        public event ElementEventHandler<PointerInputHandlerElement, PointerEvent> PointerSecondaryClick;

        public event ElementEventHandler<PointerInputHandlerElement, PointerFocusEvent> FocusChanged;

        private Vector2 _pointerDownNormalizedPosition = Vector2.Zero;

        public PointerInputHandlerElement(Element innerElement = default,
            bool blockInput = true,
            bool isInteractable = true)
            : base(innerElement)
        {
            BlockInput = blockInput;
            IsInteractable = isInteractable;
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
            => InteractionChanged?.Invoke(this, value);

        protected virtual void OnScrollWheel(in PointerScrollEvent pointerEvent)
            => ScrollWheel?.Invoke(this, pointerEvent);
        protected virtual void OnHorizontalScrollWheel(in PointerScrollEvent pointerEvent)
            => HorizontalScrollWheel?.Invoke(this, pointerEvent);

        protected virtual void OnPointerEnter(in PointerEvent pointerEvent)
            => PointerEnter?.Invoke(this, pointerEvent);
        protected virtual void OnPointerMove(in PointerEvent pointerEvent)
            => PointerMove?.Invoke(this, pointerEvent);
        protected virtual void OnPointerLeave(in PointerEvent pointerEvent)
            => PointerLeave?.Invoke(this, pointerEvent);

        protected virtual void OnPointerDown(in PointerEvent pointerEvent)
        {
            PointerDown?.Invoke(this, pointerEvent);

            var boundingBox = BoundingRectangle;
            var localPointerPosition = pointerEvent.Position - new Point(boundingBox.Left, boundingBox.Top);

            _pointerDownNormalizedPosition = localPointerPosition.ToVector2() / Size;
        }
        protected virtual void OnPointerUp(in PointerEvent pointerEvent)
            => PointerUp?.Invoke(this, pointerEvent);

        protected virtual void OnPointerSecondaryDown(in PointerEvent pointerEvent)
            => PointerSecondaryDown?.Invoke(this, pointerEvent);
        protected virtual void OnPointerSecondaryUp(in PointerEvent pointerEvent)
            => PointerSecondaryUp?.Invoke(this, pointerEvent);

        protected virtual void OnPointerDrag(in PointerDragEvent pointerEvent)
        {
            PointerDrag?.Invoke(this, pointerEvent);

            var boundingBox = BoundingRectangle;
            var pointerDownPosition = new Vector2(boundingBox.Left, boundingBox.Top)
                + Size * _pointerDownNormalizedPosition;

            var vectorPosition = pointerEvent.Position.ToVector2();
            var axisXPointerPosition = Vector2.Dot(Vector2.UnitX, vectorPosition * Vector2.UnitX);
            var axisXPointerDownPosition = Vector2.Dot(Vector2.UnitX, pointerDownPosition * Vector2.UnitX);
            var axisYPointerPosition = Vector2.Dot(Vector2.UnitY, vectorPosition * Vector2.UnitY);
            var axisYPointerDownPosition = Vector2.Dot(Vector2.UnitY, pointerDownPosition * Vector2.UnitY);

            var vectorDelta = pointerEvent.Delta.ToVector2();
            var axisXDelta = Vector2.Dot(Vector2.UnitX, vectorDelta);
            var axisYDelta = Vector2.Dot(Vector2.UnitY, vectorDelta);

            var canDragX = MathF.Sign(axisXPointerPosition - axisXPointerDownPosition) == MathF.Sign(axisXDelta);
            var canDragY = MathF.Sign(axisYPointerPosition - axisYPointerDownPosition) == MathF.Sign(axisYDelta);

            if (!canDragX && !canDragY)
                return;

            var delta = new Point()
            {
                X = canDragX ? pointerEvent.Delta.X : 0,
                Y = canDragY ? pointerEvent.Delta.Y : 0
            };
            OnPointerFixedDrag(new PointerDragEvent(pointerEvent.Pointer, pointerEvent.Position,
                pointerEvent.IsPrimaryButtonPressed, pointerEvent.IsSecondaryButtonPressed, delta));
        }
        protected virtual void OnPointerFixedDrag(in PointerDragEvent pointerEvent)
            => PointerFixedDrag?.Invoke(this, pointerEvent);

        protected virtual void OnPointerClick(in PointerEvent pointerEvent)
            => PointerClick?.Invoke(this, pointerEvent);
        protected virtual void OnPointerSecondaryClick(in PointerEvent pointerEvent)
            => PointerSecondaryClick?.Invoke(this, pointerEvent);

        protected virtual void OnFocusChanged(in PointerFocusEvent pointerEvent)
        {
            IsFocused = pointerEvent.IsFocused;
            FocusChanged?.Invoke(this, pointerEvent);
        }
    }
}
