using Microsoft.Xna.Framework;

namespace ComposableUi
{
    public class ButtonElement : PointerInputHandlerElement, IDrawableElement
    {
        public static readonly Vector2 DefaultSize = new(200, 50);

        private ISpriteSource _normalSprite;
        public ISpriteSource NormalSprite
        {
            get => _normalSprite;
            set
            {
                _normalSprite = value;
                MarkVisualStateDirty();
            }
        }
        private ISpriteSource _hoverSprite;
        public ISpriteSource HoverSprite
        {
            get => _hoverSprite;
            set
            {
                _hoverSprite = value;
                MarkVisualStateDirty();
            }
        }
        private ISpriteSource _pressedSprite;
        public ISpriteSource PressedSprite
        {
            get => _pressedSprite;
            set
            {
                _pressedSprite = value;
                MarkVisualStateDirty();
            }
        }
        private ISpriteSource _disabledSprite;
        public ISpriteSource DisabledSprite
        {
            get => _disabledSprite;
            set
            {
                _disabledSprite = value;
                MarkVisualStateDirty();
            }
        }

        private Color _normalColor;
        public Color NormalColor
        {
            get => _normalColor;
            set
            {
                _normalColor = value;
                MarkVisualStateDirty();
            }
        }
        private Color _hoverColor;
        public Color HoverColor
        {
            get => _hoverColor;
            set
            {
                _hoverColor = value;
                MarkVisualStateDirty();
            }
        }
        private Color _pressedColor;
        public Color PressedColor
        {
            get => _pressedColor;
            set
            {
                _pressedColor = value;
                MarkVisualStateDirty();
            }
        }
        private Color _disabledColor;
        public Color DisabledColor
        {
            get => _disabledColor;
            set
            {
                _disabledColor = value;
                MarkVisualStateDirty();
            }
        }

        public bool IsPressed { get; private set; }

        protected InteractionState CurrentInteractionState { get; private set; }

        private (ISpriteSource Sprite, Color Color) _currentVisualState;

        private bool _isVisualStateDirty = true;

        public ButtonElement(Vector2? size = default,
            Element innerElement = default,
            ISpriteSource normalSprite = default,
            ISpriteSource hoverSprite = default,
            ISpriteSource pressedSprite = default,
            ISpriteSource disabledSprite = default,
            Color? normalColor = default,
            Color? hoverColor = default,
            Color? pressedColor = default,
            Color? disabledColor = default,
            bool isInteractable = true)
        {
            Size = size ?? DefaultSize;

            if (innerElement is not null)
            {
                InnerElement = innerElement;
                InnerElement.Size = Size;
            }

            NormalSprite = normalSprite ?? BuiltInSprite.RectangleButton;
            HoverSprite = hoverSprite ?? BuiltInSprite.HoverRectangleButton;
            PressedSprite = pressedSprite ?? BuiltInSprite.PressedRectangleButton;
            DisabledSprite = disabledSprite ?? BuiltInSprite.DisabledRectangleButton;

            NormalColor = normalColor ?? Color.White;
            HoverColor = hoverColor ?? Color.White;
            PressedColor = pressedColor ?? Color.White;
            DisabledColor = disabledColor ?? Color.White;

            IsInteractable = isInteractable;

            OnInteractionStateChanged(isInteractable ? InteractionState.Normal : InteractionState.Disabled);
        }

        private void MarkVisualStateDirty()
        {
            _isVisualStateDirty = true;
        }

        private void RefreshVisualStateIfDirty()
        {
            if (!_isVisualStateDirty)
                return;

            _isVisualStateDirty = false;

            _currentVisualState = CurrentInteractionState switch
            {
                InteractionState.Normal => (NormalSprite, NormalColor),
                InteractionState.Hover => (HoverSprite, HoverColor),
                InteractionState.Pressed => (PressedSprite, PressedColor),
                InteractionState.Disabled => (DisabledSprite, DisabledColor),
                _ => (NormalSprite, NormalColor)
            };
        }

        void IDrawableElement.Draw(IUiRenderer renderer)
        {
            RefreshVisualStateIfDirty();

            var sprite = _currentVisualState.Sprite.Resolve(Context);
            if (sprite is not null)
            {
                renderer.DrawSprite(sprite, DrawMode.Sliced,
                    BoundingRectangle, ClipMask, _currentVisualState.Color);
            }
        }

        protected virtual void OnInteractionStateChanged(InteractionState state)
        {
            if (CurrentInteractionState == state)
                return;

            CurrentInteractionState = state;
            MarkVisualStateDirty();
        }

        protected override void OnInteractionChanged(bool value)
        {
            base.OnInteractionChanged(value);

            IsPressed = false;
            OnInteractionStateChanged(value ? InteractionState.Normal : InteractionState.Disabled);
        }

        protected override void OnPointerEnter(in PointerEvent pointerEvent)
        {
            base.OnPointerEnter(pointerEvent);

            OnInteractionStateChanged(IsPressed ? InteractionState.Pressed : InteractionState.Hover);
        }

        protected override void OnPointerLeave(in PointerEvent pointerEvent)
        {
            base.OnPointerLeave(pointerEvent);

            OnInteractionStateChanged(InteractionState.Normal);
        }

        protected override void OnPointerDown(in PointerEvent pointerEvent)
        {
            base.OnPointerDown(pointerEvent);

            IsPressed = true;
            OnInteractionStateChanged(InteractionState.Pressed);
        }

        protected override void OnPointerUp(in PointerEvent pointerEvent)
        {
            base.OnPointerUp(pointerEvent);

            IsPressed = false;
            OnInteractionStateChanged(IsHover ? InteractionState.Hover : InteractionState.Normal);
        }

        protected enum InteractionState
        {
            Normal,
            Hover,
            Pressed,
            Disabled
        }
    }
}
