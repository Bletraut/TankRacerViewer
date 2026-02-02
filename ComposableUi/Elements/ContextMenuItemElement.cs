using System;

using ComposableUi.Elements;

using Microsoft.Xna.Framework;

namespace ComposableUi
{
    public sealed class ContextMenuItemElement : Element
    {
        public const float DefaultHeight = 20;
        public const float DefaultArrowWidth = 10;

        public string Key { get; }

        public string Name
        {
            get => NameText.Text;
            set => NameText.Text = value;
        }

        public string KeyBindings
        {
            get => KeyBindingsText.Text;
            set => KeyBindingsText.Text = value;
        }

        public Color ButtonHoverAndPressedColor { get; set; }

        public Color TextNormalColor { get; set; }
        public Color TextHoverColor { get; set; }

        public TextElement NameText { get; }
        public TextElement KeyBindingsText { get; }
        public ButtonElement Button { get; }
        public SpriteElement Arrow { get; }

        public Element ExtraContent { get; }

        public Action<ContextMenuItemElement> ClickAction { get; set; }

        private readonly Element _spaceHolder;

        public ContextMenuItemElement(string key,
            string name,
            string keyBindings = default,
            Action<ContextMenuItemElement> clickAction = default,
            float height = DefaultHeight,
            Color? textNormalColor = default,
            Color? textHoverColor = default,
            Color? buttonHoverAndPressedColor = default)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException($"{nameof(key)} can't be empty or null", nameof(key));

            Key = key;
            ClickAction = clickAction;

            TextNormalColor = textNormalColor ?? Color.Black;
            TextHoverColor = textHoverColor ?? Color.White;
            ButtonHoverAndPressedColor = buttonHoverAndPressedColor ?? Color.DarkSlateBlue;

            NameText = new TextElement(
                size: new Vector2(height),
                textAlignmentFactor: Alignment.MiddleRight,
                sizeToTextWidth: true,
                color: TextNormalColor);
            Name = name;

            KeyBindingsText = new TextElement(
                size: new Vector2(height),
                textAlignmentFactor: Alignment.MiddleRight,
                sizeToTextWidth: true,
                color: TextNormalColor);
            KeyBindings = keyBindings;

            Arrow = new SpriteElement(
                size: new Vector2(DefaultArrowWidth),
                skin: StandardSkin.RightArrowIcon,
                color: TextNormalColor)
            {
                Pivot = Alignment.MiddleRight
            };

            _spaceHolder = new Element()
            {
                Size = new Vector2(DefaultArrowWidth)
            };

            EnableArrow(Random.Shared.NextSingle() > 0.5f);

            ExtraContent = new RowLayout(
                children: [KeyBindingsText, Arrow, _spaceHolder],
                alignmentFactor: Alignment.MiddleRight,
                sizeMainAxisToContent: true)
            {
                Size = new Vector2(height),
                Pivot = Alignment.MiddleRight
            };

            Button = new ButtonElement(
                normalSkin: StandardSkin.None,
                hoverSkin: StandardSkin.WhitePixel,
                pressedSkin: StandardSkin.WhitePixel,
                disabledSkin: StandardSkin.None,
                hoverColor: ButtonHoverAndPressedColor,
                pressedColor: ButtonHoverAndPressedColor);

            Button.PointerEnter += OnButtonPointerEnter;
            Button.PointerLeave += OnButtonPointerLeave;
            Button.PointerClick += OnButtonClicked;
        }

        public void EnableArrow(bool isEnabled)
        {
            Arrow.IsEnabled = isEnabled;
            _spaceHolder.IsEnabled = !isEnabled;
        }

        private void SetContentColor(Color color)
        {
            NameText.Color = color;
            KeyBindingsText.Color = color;
            Arrow.Color = color;
        }

        private void OnButtonPointerEnter(Element sender, Point position)
        {
            SetContentColor(TextHoverColor);
        }

        private void OnButtonPointerLeave(Element sender, Point e)
        {
            SetContentColor(TextNormalColor);
        }

        private void OnButtonClicked(Element sender, Point position)
        {
            ClickAction?.Invoke(this);
        }
    }
}
