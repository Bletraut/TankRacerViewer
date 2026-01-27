using ComposableUi.Elements;

using Microsoft.Xna.Framework;

namespace ComposableUi
{
    public class ButtonElement : PointerInputHandlerElement, IDrawableElement
    {
        public static readonly Vector2 DefaultSize = new(200, 50);

        public Sprite NormalSprite { get; set; }
        public Sprite HoverSprite { get; set; }
        public Sprite PressedSprite { get; set; }
        public Sprite DisabledSprite { get; set; }

        public StandardSkin NormalSkin { get; set; }
        public StandardSkin HoverSkin { get; set; }
        public StandardSkin PressedSkin { get; set; }
        public StandardSkin DisabledSkin { get; set; }

        public Color NormalColor { get; set; }
        public Color HoverColor { get; set; }
        public Color PressedColor { get; set; }
        public Color DisabledColor { get; set; }

        public bool IsHover { get; private set; }
        public bool IsPressed { get; private set; }

        private (Sprite Sprite, StandardSkin Skin, Color Color) _currentState;

        public ButtonElement(Vector2? size,
            Sprite normalSprite = default,
            Sprite hoverSprite = default,
            Sprite pressedSprite = default,
            Sprite disabledSprite = default,
            StandardSkin normalSkin = StandardSkin.RectangleButton,
            StandardSkin hoverSkin = StandardSkin.RectangleButtonHover,
            StandardSkin pressedSkin = StandardSkin.RectangleButtonPressed,
            StandardSkin disabledSkin = StandardSkin.RectangleButtonDisabled,
            Color? normalColor = default,
            Color? hoverColor = default,
            Color? pressedColor = default,
            Color? disabledColor = default,
            bool isInteractable = true)
        {
            Size = size ?? DefaultSize;

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

        void IDrawableElement.Draw(IUiRenderer renderer)
        {
            if (_currentState.Sprite != null)
            {
                renderer.DrawSprite(_currentState.Sprite, DrawMode.Sliced,
                    BoundingRectangle, ClipMask, _currentState.Color);
            }
            else
            {
                renderer.DrawSkinnedRectangle(_currentState.Skin, DrawMode.Sliced,
                    BoundingRectangle, ClipMask, _currentState.Color);
            }
        }

        protected virtual void OnInteractionStateChanged(InteractionState state)
        {
            _currentState = state switch
            {
                InteractionState.Normal => (NormalSprite, NormalSkin, NormalColor),
                InteractionState.Hover => (HoverSprite, HoverSkin, HoverColor),
                InteractionState.Pressed => (PressedSprite, PressedSkin, PressedColor),
                InteractionState.Disabled => (DisabledSprite, DisabledSkin, DisabledColor),
                _ => (NormalSprite, NormalSkin, NormalColor)
            };
        }

        protected override void OnInteractionChanged(bool value)
        {
            base.OnInteractionChanged(value);

            IsHover = IsPressed = false;
            OnInteractionStateChanged(value ? InteractionState.Normal : InteractionState.Disabled);
        }

        protected override void OnPointerEnter(Point position)
        {
            base.OnPointerEnter(position);

            IsHover = true;
            OnInteractionStateChanged(IsPressed ? InteractionState.Pressed : InteractionState.Hover);
        }

        protected override void OnPointerLeave(Point position)
        {
            base.OnPointerLeave(position);

            IsHover = false;
            OnInteractionStateChanged(InteractionState.Normal);
        }

        protected override void OnPointerDown(Point position)
        {
            base.OnPointerDown(position);

            IsPressed = true;
            OnInteractionStateChanged(InteractionState.Pressed);
        }

        protected override void OnPointerUp(Point position)
        {
            base.OnPointerUp(position);

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
