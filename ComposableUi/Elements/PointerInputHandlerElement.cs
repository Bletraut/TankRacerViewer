using Microsoft.Xna.Framework;

namespace ComposableUi
{
    public class PointerInputHandlerElement : SizedToContentHolderElement,
        IPointerInputHandler
    {
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

        public event ElementEventHandler<Point> PointerClick;
        public event ElementEventHandler<Point> PointerSecondaryClick;

        public PointerInputHandlerElement(bool blockInput = true,
            Element innerElement = default,
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
            => PointerDown?.Invoke(this, position);
        protected virtual void OnPointerUp(Point position)
            => PointerUp?.Invoke(this, position);

        protected virtual void OnPointerSecondaryDown(Point position)
            => PointerSecondaryDown?.Invoke(this, position);
        protected virtual void OnPointerSecondaryUp(Point position)
            => PointerSecondaryUp?.Invoke(this, position);

        protected virtual void OnPointerDrag(Point position, Point delta)
            => PointerDrag?.Invoke(this, (position, delta));

        protected virtual void OnPointerClick(Point position)
            => PointerClick?.Invoke(this, position);
        protected virtual void OnPointerSecondaryClick(Point position)
            => PointerSecondaryClick?.Invoke(this, position);
    }
}
