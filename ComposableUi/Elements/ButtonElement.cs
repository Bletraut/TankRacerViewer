using Microsoft.Xna.Framework;

namespace ComposableUi
{
    public class ButtonElement : PointerInputHandlerElement, IDrawableElement
    {
        public static readonly Vector2 DefaultSize = new(200, 50);

        private Sprite _normalSprite;
        public Sprite NormalSprite
        {
            get => _normalSprite;
            set
            {
                _normalSprite = value;
                MarkVisualStateDirty();
            }
        }
        private Sprite _hoverSprite;
        public Sprite HoverSprite
        {
            get => _hoverSprite;
            set
            {
                _hoverSprite = value;
                MarkVisualStateDirty();
            }
        }
        private Sprite _pressedSprite;
        public Sprite PressedSprite
        {
            get => _pressedSprite;
            set
            {
                _pressedSprite = value;
                MarkVisualStateDirty();
            }
        }
        private Sprite _disabledSprite;
        public Sprite DisabledSprite
        {
            get => _disabledSprite;
            set
            {
                _disabledSprite = value;
                MarkVisualStateDirty();
            }
        }

        private StandardSkin _normalSkin;
        public StandardSkin NormalSkin
        {
            get => _normalSkin;
            set
            {
                _normalSkin = value;
                MarkVisualStateDirty();
            }
        }
        private StandardSkin _hoverSkin;
        public StandardSkin HoverSkin
        {
            get => _hoverSkin;
            set
            {
                _hoverSkin = value;
                MarkVisualStateDirty();
            }
        }
        private StandardSkin _pressedSkin;
        public StandardSkin PressedSkin
        {
            get => _pressedSkin;
            set
            {
                _pressedSkin = value;
                MarkVisualStateDirty();
            }
        }
        private StandardSkin _disabledSkin;
        public StandardSkin DisabledSkin
        {
            get => _disabledSkin;
            set
            {
                _disabledSkin = value;
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

        public bool IsHover { get; private set; }
        public bool IsPressed { get; private set; }

        private InteractionState _currentInteractionState;
        private (Sprite Sprite, StandardSkin Skin, Color Color) _currentVisualState;

        private bool _isVisualStateDirty = true;

        public ButtonElement(Vector2? size = default,
            Element innerElement = default,
            Sprite normalSprite = default,
            Sprite hoverSprite = default,
            Sprite pressedSprite = default,
            Sprite disabledSprite = default,
            StandardSkin normalSkin = StandardSkin.RectangleButton,
            StandardSkin hoverSkin = StandardSkin.HoverRectangleButton,
            StandardSkin pressedSkin = StandardSkin.PressedRectangleButton,
            StandardSkin disabledSkin = StandardSkin.DisabledRectangleButton,
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

            NormalSprite = normalSprite;
            HoverSprite = hoverSprite;
            PressedSprite = pressedSprite;
            DisabledSprite = disabledSprite;

            NormalSkin = normalSkin;
            HoverSkin = hoverSkin;
            PressedSkin = pressedSkin;
            DisabledSkin = disabledSkin;

            NormalColor = normalColor ?? Color.White;
            HoverColor = hoverColor ?? Color.White;
            PressedColor = pressedColor ?? Color.White;
            DisabledColor = disabledColor ?? Color.White;

            IsInteractable = isInteractable;

            OnInteractionStateChanged(isInteractable ? InteractionState.Normal : InteractionState.Disabled);
        }

        public void MarkVisualStateDirty()
        {
            _isVisualStateDirty = true;
        }

        private void RefreshVisualStateIfDirty()
        {
            if (!_isVisualStateDirty)
                return;

            _isVisualStateDirty = false;

            _currentVisualState = _currentInteractionState switch
            {
                InteractionState.Normal => (NormalSprite, NormalSkin, NormalColor),
                InteractionState.Hover => (HoverSprite, HoverSkin, HoverColor),
                InteractionState.Pressed => (PressedSprite, PressedSkin, PressedColor),
                InteractionState.Disabled => (DisabledSprite, DisabledSkin, DisabledColor),
                _ => (NormalSprite, NormalSkin, NormalColor)
            };
        }

        void IDrawableElement.Draw(IUiRenderer renderer)
        {
            RefreshVisualStateIfDirty();

            if (_currentVisualState.Sprite is not null)
            {
                renderer.DrawSprite(_currentVisualState.Sprite, DrawMode.Sliced,
                    BoundingRectangle, ClipMask, _currentVisualState.Color);
            }
            else
            {
                renderer.DrawSkinnedRectangle(_currentVisualState.Skin, DrawMode.Sliced,
                    BoundingRectangle, ClipMask, _currentVisualState.Color);
            }
        }

        protected virtual void OnInteractionStateChanged(InteractionState state)
        {
            if (_currentInteractionState == state)
                return;

            _currentInteractionState = state;
            MarkVisualStateDirty();
        }

        protected override void OnInteractionChanged(bool value)
        {
            base.OnInteractionChanged(value);

            IsHover = IsPressed = false;
            OnInteractionStateChanged(value ? InteractionState.Normal : InteractionState.Disabled);
        }

        protected override void OnPointerEnter(in PointerEvent pointerEvent)
        {
            base.OnPointerEnter(pointerEvent);

            IsHover = true;
            OnInteractionStateChanged(IsPressed ? InteractionState.Pressed : InteractionState.Hover);
        }

        protected override void OnPointerLeave(in PointerEvent pointerEvent)
        {
            base.OnPointerLeave(pointerEvent);

            IsHover = false;
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
