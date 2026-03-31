using ComposableUi.Elements.DropDownList;

using Microsoft.Xna.Framework;

namespace ComposableUi
{
    public class DropDownListTextItemElement : PointerInputHandlerElement<DropDownListTextItemElement>,
        IDropDownListItem<DropDownListTextItemElement>
    {
        public const float DefaultValueIndent = 6;

        // Static.
        public readonly Color DefaultNormalBackgroundColor = Color.Transparent;
        public readonly Color DefaultHoverBackgroundColor = Color.Red;
        public readonly Color DefaultSelectedBackgroundColor = Color.Blue;

        public readonly Color DefaultNormalValueColor = Color.Black;
        public readonly Color DefaultHoverValueColor = Color.White;
        public readonly Color DefaultSelectedValueColor = Color.Black;

        // Class.
        public bool IsSelectable
        {
            get => IsInteractable;
            set => IsInteractable = value;
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
            Color? normalBackgroundColor = default,
            Color? hoverBackgroundColor = default,
            Color? selectedBackgroundColor = default,
            Color? normalValueColor = default,
            Color? hoverValueColor = default,
            Color? selectedValueColor = default)
        {
            _normalBackgroundColor = normalBackgroundColor ?? DefaultNormalBackgroundColor;
            _hoverBackgroundColor = hoverBackgroundColor ?? DefaultHoverBackgroundColor;
            _selectedBackgroundColor = selectedBackgroundColor ?? DefaultSelectedBackgroundColor;
            _normalValueColor = normalValueColor ?? DefaultNormalValueColor;
            _hoverValueColor = hoverValueColor ?? DefaultHoverValueColor;
            _selectedValueColor = selectedValueColor ?? DefaultSelectedValueColor;

            Background = new SpriteElement(
                skin: StandardSkin.WhitePixel
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
            var (backgroundColor, valueColor) = (IsHover, _isSelected) switch
            {
                (true, _) => (HoverBackgroundColor, HoverValueColor),
                (false, true) => (SelectedBackgroundColor, SelectedValueColor),
                _ => (NormalBackgroundColor, NormalValueColor)
            };

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
