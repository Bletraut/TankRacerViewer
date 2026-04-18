using ComposableUi.Elements.DropDownList;

using Microsoft.Xna.Framework;

namespace ComposableUi
{
    public class DropDownListTextItemElement : PointerInputHandlerElement<DropDownListTextItemElement>,
        IDropDownListItem<DropDownListTextItemElement>
    {
        public const float DefaultValueIndent = 6;

        public const StandardSkin DefaultNormalBackgroundSkin = StandardSkin.None;
        public const StandardSkin DefaultHoverBackgroundSkin = StandardSkin.SelectionStrongLightPixel;
        public const StandardSkin DefaultSelectedBackgroundSkin = StandardSkin.SelectionStrongDarkPixel;

        // Static.
        public readonly Color DefaultNormalValueColor = Color.Black;
        public readonly Color DefaultHoverValueColor = Color.White;
        public readonly Color DefaultSelectedValueColor = Color.White;

        // Class.
        public bool IsSelectable
        {
            get => IsInteractable;
            set => IsInteractable = value;
        }

        private Sprite _normalBackgroundSprite;
        public Sprite NormalBackgroundSprite
        {
            get => _normalBackgroundSprite;
            set
            {
                _normalBackgroundSprite = value;
                RefreshVisualState();
            }
        }

        private Sprite _hoverBackgroundSprite;
        public Sprite HoverBackgroundSprite
        {
            get => _hoverBackgroundSprite;
            set
            {
                _hoverBackgroundSprite = value;
                RefreshVisualState();
            }
        }

        private Sprite _selectedBackgroundSprite;
        public Sprite SelectedBackgroundSprite
        {
            get => _selectedBackgroundSprite;
            set
            {
                _selectedBackgroundSprite = value;
                RefreshVisualState();
            }
        }

        private StandardSkin _normalBackgroundSkin;
        public StandardSkin NormalBackgroundSkin
        {
            get => _normalBackgroundSkin;
            set
            {
                _normalBackgroundSkin = value;
                RefreshVisualState();
            }
        }

        private StandardSkin _hoverBackgroundSkin;
        public StandardSkin HoverBackgroundSkin
        {
            get => _hoverBackgroundSkin;
            set
            {
                _hoverBackgroundSkin = value;
                RefreshVisualState();
            }
        }

        private StandardSkin _selectedBackgroundSkin;
        public StandardSkin SelectedBackgroundSkin
        {
            get => _selectedBackgroundSkin;
            set
            {
                _selectedBackgroundSkin = value;
                RefreshVisualState();
            }
        }

        private Color _normalBackgroundColor;
        public Color NormalBackgroundColor
        {
            get => _normalBackgroundColor;
            set
            {
                _normalBackgroundColor = value;
                RefreshVisualState();
            }
        }

        public Color _hoverBackgroundColor;
        public Color HoverBackgroundColor
        {
            get => _hoverBackgroundColor;
            set
            {
                _hoverBackgroundColor = value;
                RefreshVisualState();
            }
        }

        private Color _selectedBackgroundColor;
        public Color SelectedBackgroundColor
        {
            get => _selectedBackgroundColor;
            set
            {
                _selectedBackgroundColor = value;
                RefreshVisualState();
            }
        }

        private Color _normalValueColor;
        public Color NormalValueColor
        {
            get => _normalValueColor;
            set
            {
                _normalValueColor = value;
                RefreshVisualState();
            }
        }

        private Color _hoverValueColor;
        public Color HoverValueColor
        {
            get => _hoverValueColor;
            set
            {
                _hoverValueColor = value;
                RefreshVisualState();
            }
        }

        private Color _selectedValueColor;
        public Color SelectedValueColor
        {
            get => _selectedValueColor;
            set
            {
                _selectedValueColor = value;
                RefreshVisualState();
            }
        }

        public SpriteElement Background { get; }
        public TextElement Value { get; }

        public event ElementEventHandler<DropDownListTextItemElement> Selected;

        private bool _isSelected;

        public DropDownListTextItemElement(string value = default,
            Sprite normalBackgroundSprite = default,
            Sprite hoverBackgroundSprite = default,
            Sprite selectedBackgroundSprite = default,
            StandardSkin normalBackgroundSkin = DefaultNormalBackgroundSkin,
            StandardSkin hoverBackgroundSkin = DefaultHoverBackgroundSkin,
            StandardSkin selectedBackgroundSkin = DefaultSelectedBackgroundSkin,
            Color? normalBackgroundColor = default,
            Color? hoverBackgroundColor = default,
            Color? selectedBackgroundColor = default,
            Color? normalValueColor = default,
            Color? hoverValueColor = default,
            Color? selectedValueColor = default)
        {
            _normalBackgroundSprite = normalBackgroundSprite;
            _hoverBackgroundSprite = hoverBackgroundSprite;
            _selectedBackgroundSprite = selectedBackgroundSprite;
            _normalBackgroundSkin = normalBackgroundSkin;
            _hoverBackgroundSkin = hoverBackgroundSkin;
            _selectedBackgroundSkin = selectedBackgroundSkin;
            _normalBackgroundColor = normalBackgroundColor ?? Color.White;
            _hoverBackgroundColor = hoverBackgroundColor ?? Color.White;
            _selectedBackgroundColor = selectedBackgroundColor ?? Color.White;

            _normalValueColor = normalValueColor ?? DefaultNormalValueColor;
            _hoverValueColor = hoverValueColor ?? DefaultHoverValueColor;
            _selectedValueColor = selectedValueColor ?? DefaultSelectedValueColor;

            Background = new SpriteElement(
                skin: DefaultNormalBackgroundSkin
            );

            Value = new TextElement(
                text: value,
                textAlignmentFactor: Alignment.MiddleLeft,
                sizeToTextWidth: true,
                sizeToTextHeight: true
            );

            InnerElement = new RowLayout(
                alignmentFactor: Alignment.MiddleLeft,
                leftPadding: DefaultValueIndent,
                sizeMainAxisToContent: true,
                sizeCrossAxisToContent: true,
                expandChildrenMainAxis: true,
                children: [
                    new LayoutElement(
                        ignoreLayout: true,
                        innerElement: new ExpandedElement(
                            innerElement: Background
                        )
                    ),
                    Value
                ]
            );

            RefreshVisualState();
        }

        private void RefreshVisualState()
        {
            var (backgroundSprite, backgroundSkin, backgroundColor, valueColor) = (IsHover, _isSelected) switch
            {
                (true, _) => (HoverBackgroundSprite, HoverBackgroundSkin, HoverBackgroundColor, HoverValueColor),
                (false, true) => (SelectedBackgroundSprite, SelectedBackgroundSkin, SelectedBackgroundColor, SelectedValueColor),
                _ => (NormalBackgroundSprite, NormalBackgroundSkin, NormalBackgroundColor, NormalValueColor)
            };

            Background.Sprite = backgroundSprite;
            Background.Skin = backgroundSkin;
            Background.Color = backgroundColor;
            Value.Color = valueColor;
        }

        void IDropDownListItem<DropDownListTextItemElement>.SetSelected(bool value)
        {
            _isSelected = value;
            RefreshVisualState();
        }

        DropDownListTextItemElement IDropDownListItem<DropDownListTextItemElement>.CreateEmpty()
        {
            return new DropDownListTextItemElement();
        }

        void IDropDownListItem<DropDownListTextItemElement>.Clone(DropDownListTextItemElement source)
        {
            Value.Text = source.Value.Text;
        }

        protected override void OnPointerEnter(in PointerEvent pointerEvent)
        {
            base.OnPointerEnter(pointerEvent);
            RefreshVisualState();
        }

        protected override void OnPointerLeave(in PointerEvent pointerEvent)
        {
            base.OnPointerLeave(pointerEvent);
            RefreshVisualState();
        }

        protected override void OnPointerClick(in PointerEvent pointerEvent)
        {
            base.OnPointerClick(pointerEvent);
            Selected?.Invoke(this);
        }
    }
}
