using System;

using ComposableUi.Elements;

using Microsoft.Xna.Framework;

namespace ComposableUi
{
    public sealed class ContextMenuItemElement : Element
    {
        public const float DefaultHeight = 22;
        public const float DefaultArrowWidth = 10;

        internal Element ExtraContent { get; }
        internal SpriteElement Arrow { get; }

        public bool IsInteractable
        {
            get => Button.IsInteractable;
            set
            {
                Button.IsInteractable = value;
                SetContentColor(Button.IsInteractable ? ContentNormalColor : ContentDisabledColor);
            }
        }

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

        public Color HoverColor
        {
            get => _buttonBackground.Color;
            set => _buttonBackground.Color = value;
        }

        public Color ContentNormalColor { get; set; }
        public Color ContentHoverColor { get; set; }
        public Color ContentDisabledColor { get; set; }

        public TextElement NameText { get; }
        public TextElement KeyBindingsText { get; }
        public PointerInputHandlerElement Button { get; }

        public Action<ContextMenuItemElement> ClickAction { get; set; }

        public override bool IsEnabled 
        { 
            get => base.IsEnabled;
            set
            {
                base.IsEnabled = value;
                NameText.IsEnabled = value;
                ExtraContent.IsEnabled = value;
                Button.IsEnabled = value;
            }
        }

        public event ElementEventHandler<ContextMenuItemElement, Point> PointerEnter;
        public event ElementEventHandler<ContextMenuItemElement, Point> PointerLeave;

        private readonly Element _arrowSpaceHolder;
        private readonly SpriteElement _buttonBackground;

        public ContextMenuItemElement(string key = default,
            string name = default,
            string keyBindings = default,
            Action<ContextMenuItemElement> clickAction = default,
            float height = DefaultHeight,
            Color? hoverColor = default,
            Color? contentNormalColor = default,
            Color? contentHoverColor = default,
            Color? contentDisabledColor = default,
            bool isInteractable = true)
        {
            Key = key ?? string.Empty;
            ClickAction = clickAction;

            ContentNormalColor = contentNormalColor ?? Color.Black;
            ContentHoverColor = contentHoverColor ?? Color.White;
            ContentDisabledColor = contentDisabledColor ?? Color.DimGray;

            NameText = new TextElement(
                size: new Vector2(height),
                textAlignmentFactor: Alignment.MiddleRight,
                sizeToTextWidth: true,
                color: ContentNormalColor);
            Name = name;

            KeyBindingsText = new TextElement(
                size: new Vector2(height),
                textAlignmentFactor: Alignment.MiddleRight,
                sizeToTextWidth: true,
                color: ContentNormalColor);
            KeyBindings = keyBindings;

            Arrow = new SpriteElement(
                size: new Vector2(DefaultArrowWidth),
                skin: StandardSkin.RightArrowIcon,
                color: ContentNormalColor)
            {
                Pivot = Alignment.MiddleRight
            };

            _arrowSpaceHolder = new Element()
            {
                Size = new Vector2(DefaultArrowWidth)
            };
            EnableArrow(false);

            ExtraContent = new RowLayout(
                children: [KeyBindingsText, Arrow, _arrowSpaceHolder],
                alignmentFactor: Alignment.MiddleRight,
                sizeMainAxisToContent: true)
            {
                Size = new Vector2(height),
                Pivot = Alignment.MiddleRight
            };

            _buttonBackground = new SpriteElement(
                skin: StandardSkin.None);
            HoverColor = hoverColor ?? Color.DarkSlateBlue;

            Button = new PointerInputHandlerElement(_buttonBackground);
            IsInteractable = isInteractable;

            Button.PointerEnter += OnButtonPointerEnter;
            Button.PointerLeave += OnButtonPointerLeave;
            Button.PointerClick += OnButtonClicked;
        }

        internal void EnableArrow(bool isEnabled)
        {
            Arrow.IsEnabled = isEnabled;
            _arrowSpaceHolder.IsEnabled = !isEnabled;
        }

        internal void SetHover(bool isHover)
        {
            if (!IsInteractable)
                return;

            if (isHover)
            {
                _buttonBackground.Skin = StandardSkin.WhitePixel;
                SetContentColor(ContentHoverColor);
            }
            else
            {
                _buttonBackground.Skin = StandardSkin.None;
                SetContentColor(ContentNormalColor);
            }
        }

        private void SetContentColor(Color color)
        {
            NameText.Color = color;
            KeyBindingsText.Color = color;
            Arrow.Color = color;
        }

        private void OnButtonPointerEnter(Element sender, Point position)
        {
            PointerEnter?.Invoke(this, position);
        }

        private void OnButtonPointerLeave(Element sender, Point position)
        {
            PointerLeave?.Invoke(this, position);
        }

        private void OnButtonClicked(Element sender, Point position)
        {
            ClickAction?.Invoke(this);
        }
    }
}
