using Microsoft.Xna.Framework;

namespace ComposableUi
{
    public class ContentButtonElement : ButtonElement
    {
        public const float DefaultContentSpacing = 4;
        public const float DefaultContentPaddings = 4;

        // Static.
        public static readonly Vector2 DefaultIconSize = new(18);

        // Class.
        public SpriteElement Icon { get; }
        public TextElement Text { get; }
        public RowLayout ContentLayout { get; }

        private Color _normalIconColor;
        public Color NormalIconColor
        {
            get => _normalIconColor;
            set
            {
                _normalIconColor = value;
                RefreshContentVisualState();
            }
        }
        private Color _hoverIconColor;
        public Color HoverIconColor
        {
            get => _hoverIconColor;
            set
            {
                _hoverIconColor = value;
                RefreshContentVisualState();
            }
        }
        private Color _pressedIconColor;
        public Color PressedIconColor
        {
            get => _pressedIconColor;
            set
            {
                _pressedIconColor = value;
                RefreshContentVisualState();
            }
        }
        private Color _disabledIconColor;
        public Color DisabledIconColor
        {
            get => _disabledIconColor;
            set
            {
                _disabledIconColor = value;
                RefreshContentVisualState();
            }
        }

        private Color _normalTextColor;
        public Color NormalTextColor
        {
            get => _normalTextColor;
            set
            {
                _normalTextColor = value;
                RefreshContentVisualState();
            }
        }
        private Color _hoverTextColor;
        public Color HoverTextColor
        {
            get => _hoverTextColor;
            set
            {
                _hoverTextColor = value;
                RefreshContentVisualState();
            }
        }
        private Color _pressedTextColor;
        public Color PressedTextColor
        {
            get => _pressedTextColor;
            set
            {
                _pressedTextColor = value;
                RefreshContentVisualState();
            }
        }
        private Color _disabledTextColor;
        public Color DisabledTextColor
        {
            get => _disabledTextColor;
            set
            {
                _disabledTextColor = value;
                RefreshContentVisualState();
            }
        }

        public ContentButtonElement(Vector2? iconSize = default,
            Sprite iconSprite = default,
            StandardSkin iconSkin = StandardSkin.WhitePixel,
            string text = default,
            Sprite normalSprite = default,
            Sprite hoverSprite = default,
            Sprite pressedSprite = default,
            Sprite disabledSprite = default,
            StandardSkin normalSkin = StandardSkin.RectangleButton,
            StandardSkin hoverSkin = StandardSkin.HoverRectangleButton,
            StandardSkin pressedSkin = StandardSkin.PressedRectangleButton,
            StandardSkin disabledSkin = StandardSkin.DisabledRectangleButton,
            Color? normalButtonColor = default,
            Color? hoverButtonColor = default,
            Color? pressedButtonColor = default,
            Color? disabledButtonColor = default,
            Color? normalIconColor = default,
            Color? hoverIconColor = default,
            Color? pressedIconColor = default,
            Color? disabledIconColor = default,
            Color? normalTextColor = default,
            Color? hoverTextColor = default,
            Color? pressedTextColor = default,
            Color? disabledTextColor = default,
            bool isInteractable = true)
            : base(normalSprite: normalSprite,
                  hoverSprite: hoverSprite,
                  pressedSprite: pressedSprite,
                  disabledSprite: disabledSprite,
                  normalSkin: normalSkin,
                  hoverSkin: hoverSkin,
                  pressedSkin: pressedSkin,
                  disabledSkin: disabledSkin,
                  normalColor: normalButtonColor,
                  hoverColor: hoverButtonColor,
                  pressedColor: pressedButtonColor,
                  disabledColor: disabledButtonColor,
                  isInteractable: isInteractable)
        {
            _normalIconColor = normalIconColor ?? Color.White;
            _hoverIconColor = hoverIconColor ?? Color.White;
            _pressedIconColor = pressedIconColor ?? Color.White;
            _disabledIconColor = disabledIconColor ?? Color.White;

            _normalTextColor = normalTextColor ?? Color.White;
            _hoverTextColor = hoverTextColor ?? Color.White;
            _pressedTextColor = pressedTextColor ?? Color.White;
            _disabledTextColor = disabledTextColor ?? Color.White;

            Icon = new SpriteElement(
                size: iconSize ?? DefaultIconSize,
                sprite: iconSprite,
                skin: iconSkin,
                sizeToSource: true,
                drawMode: DrawMode.Simple
            );

            Text = new TextElement(
                text: text,
                textAlignmentFactor: Alignment.MiddleLeft,
                sizeToTextWidth: true,
                sizeToTextHeight: true
            );

            ContentLayout = new RowLayout(
                alignmentFactor: Alignment.Center,
                spacing: DefaultContentSpacing,
                leftPadding: DefaultContentPaddings,
                rightPadding: DefaultContentPaddings,
                topPadding: DefaultContentPaddings,
                bottomPadding: DefaultContentPaddings,
                sizeMainAxisToContent: true,
                sizeCrossAxisToContent: true,
                children: [
                    Icon,
                    Text
                ]
            );

            InnerElement = ContentLayout;

            RefreshContentVisualState();
        }

        private void RefreshContentVisualState()
        {
            var (iconColor, textColor) = CurrentInteractionState switch
            {
                InteractionState.Normal => (NormalIconColor, NormalTextColor),
                InteractionState.Hover => (HoverIconColor, HoverTextColor),
                InteractionState.Pressed => (PressedIconColor, PressedTextColor),
                InteractionState.Disabled => (DisabledIconColor, DisabledTextColor),
                _ => (NormalIconColor, NormalTextColor)
            };

            Icon.Color = iconColor;
            Text.Color = textColor;
        }
    }
}
