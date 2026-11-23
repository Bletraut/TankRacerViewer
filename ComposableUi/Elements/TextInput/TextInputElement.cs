using System.Text;

using Microsoft.Xna.Framework;

namespace ComposableUi
{
    public sealed class TextInputElement : ContainerElement,
        ITextInputHandler
    {
        public const float DefaultTextPaddings = 4;
        public const float DefaultMaskPaddings = 2;

        public string Text
        {
            get => _stringBuilder.ToString();
            set
            {
                if (!_stringBuilder.Equals(value))
                {
                    _stringBuilder.Clear();

                    if (!string.IsNullOrEmpty(value))
                        _stringBuilder.Append(value);

                    OnStateChanged();
                }
            }
        }

        public string PlaceholderText { get; set; }

        public SpriteElement Background { get; }

        public ScrollViewElement ScrollView { get; }

        public RichTextElement TextElement { get; }
        public RichTextElement PlaceholderTextElement { get; }

        private PointerInputHandlerElement _pointeInputHandler;

        private readonly StringBuilder _stringBuilder = new();

        public TextInputElement(string text = default,
            string placeholderText = default,
            Color? textColor = default,
            Color? placeholderTextColor = default)
        {
            Text = text;
            PlaceholderText = placeholderText;

            Background = new SpriteElement(
                spriteSource: BuiltInSprite.TextField,
                drawMode: DrawMode.Sliced
            );

            AddChild(new ExpandedElement(
                innerElement: Background
            ));

            TextElement = new RichTextElement(
                text: Text,
                color: textColor ?? Color.Black,
                sizeToTextHeight: true
            );

            ScrollView = new ScrollViewElement(
                expandingContentWidthMode: ScrollViewElement.ExpandingMode.FillParent,
                content: new RowLayout(
                    leftPadding: DefaultTextPaddings,
                    rightPadding: DefaultTextPaddings,
                    topPadding: DefaultTextPaddings,
                    bottomPadding: DefaultTextPaddings,
                    sizeCrossAxisToContent: true,
                    expandChildrenMainAxis: true,
                    children: [
                        TextElement
                    ]
                )
            );

            ScrollView.ContentClipMask.LeftPadding = DefaultMaskPaddings;
            ScrollView.ContentClipMask.RightPadding = DefaultMaskPaddings;
            ScrollView.ContentClipMask.TopPadding = DefaultMaskPaddings;
            ScrollView.ContentClipMask.BottomPadding = DefaultMaskPaddings;

            AddChild(new ExpandedElement(
                innerElement: ScrollView
            ));

            _pointeInputHandler = new PointerInputHandlerElement(
                blockInput: false
            );
            AddChild(new ExpandedElement(
                innerElement: _pointeInputHandler
            ));

            _pointeInputHandler.FocusChanged += OnFocusChanged;
        }

        void ITextInputHandler.OnTextInput(string text)
        {
            if (!_pointeInputHandler.IsFocused)
                return;

            _stringBuilder.Append(text);

            TextElement.Text = _stringBuilder.ToString();
        }

        private void OnFocusChanged(PointerInputHandlerElement sender,
            PointerFocusEvent pointerEvent)
        {
            Background.SpriteSource = pointerEvent.IsFocused
                ? BuiltInSprite.ActiveTextField
                : BuiltInSprite.TextField;
        }
    }
}
