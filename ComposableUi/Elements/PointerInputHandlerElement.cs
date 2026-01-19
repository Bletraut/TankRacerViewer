using ComposableUi.Core;

using Microsoft.Xna.Framework;

namespace ComposableUi.Elements
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

        public event ElementEventHandler<Vector2> PointerOver;
        public event ElementEventHandler<Vector2> PointerMove;
        public event ElementEventHandler<Vector2> PointerOut;

        public event ElementEventHandler<Vector2> PointerDrag;

        public event ElementEventHandler<Vector2> PointerDown;
        public event ElementEventHandler<Vector2> PointerUp;

        public event ElementEventHandler<Vector2> PointerClick;

        public PointerInputHandlerElement(bool blockInput = true,
            Element innerElement = default)
            : base(innerElement)
        {
            _blockInput = blockInput;
        }

        protected virtual void OnPointerOver(Vector2 position)
            => PointerOver?.Invoke(this, position);
        protected virtual void OnPointerMove(Vector2 position)
            => PointerMove?.Invoke(this, position);
        protected virtual void OnPointerOut(Vector2 position)
            => PointerOut?.Invoke(this, position);

        protected virtual void OnPointerDrag(Vector2 position)
            => PointerDrag?.Invoke(this, position);

        protected virtual void OnPointerDown(Vector2 position)
            => PointerDown?.Invoke(this, position);
        protected virtual void OnPointerUp(Vector2 position)
            => PointerUp?.Invoke(this, position);

        protected virtual void OnPointerClick(Vector2 position)
            => PointerClick?.Invoke(this, position);
    }
}
