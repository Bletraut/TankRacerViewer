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

        public event ElementEventHandler<int> ScrollWheel;
        public event ElementEventHandler<int> HorizontalScrollWheel;

        public event ElementEventHandler<Point> PointerEnter;
        public event ElementEventHandler<Point> PointerMove;
        public event ElementEventHandler<Point> PointerLeave;

        public event ElementEventHandler<Point> PointerDown;
        public event ElementEventHandler<Point> PointerUp;

        public event ElementEventHandler<Point> PointerSecondaryDown;
        public event ElementEventHandler<Point> PointerSecondaryUp;

        public event ElementEventHandler<Point> PointerDrag;

        public event ElementEventHandler<Point> PointerClick;
        public event ElementEventHandler<Point> PointerSecondaryClick;

        public PointerInputHandlerElement(bool blockInput = true,
            Element innerElement = default,
            bool isInteractable = true)
            : base(innerElement)
        {
            _blockInput = blockInput;
            _isInteractable = isInteractable;
        }

        public virtual void OnInteractionChanged(bool value)
            => InteractionChanged?.Invoke(this, value);

        public virtual void OnScrollWheel(int delta)
            => ScrollWheel?.Invoke(this, delta);
        public virtual void OnHorizontalScrollWheel(int delta)
            => HorizontalScrollWheel?.Invoke(this, delta);

        public virtual void OnPointerEnter(Point position)
            => PointerEnter?.Invoke(this, position);
        public virtual void OnPointerMove(Point position)
            => PointerMove?.Invoke(this, position);
        public virtual void OnPointerLeave(Point position)
            => PointerLeave?.Invoke(this, position);

        public virtual void OnPointerDown(Point position)
            => PointerDown?.Invoke(this, position);
        public virtual void OnPointerUp(Point position)
            => PointerUp?.Invoke(this, position);

        public virtual void OnPointerSecondaryDown(Point position)
            => PointerSecondaryDown?.Invoke(this, position);
        public virtual void OnPointerSecondaryUp(Point position)
            => PointerSecondaryUp?.Invoke(this, position);

        public virtual void OnPointerDrag(Point delta)
            => PointerDrag?.Invoke(this, delta);

        public virtual void OnPointerClick(Point position)
            => PointerClick?.Invoke(this, position);
        public virtual void OnPointerSecondaryClick(Point position)
            => PointerSecondaryClick?.Invoke(this, position);
    }
}
