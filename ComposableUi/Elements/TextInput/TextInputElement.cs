using System;
using System.Diagnostics;
using System.Text;

using Microsoft.Xna.Framework;

namespace ComposableUi
{
    public sealed class TextInputElement : ContainerElement,
        ITextInputHandler,
        IKeyboardInputHandler
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

        public ScrollViewElement ScrollView { get; }

        public RichTextElement TextElement { get; }
        public RichTextElement PlaceholderTextElement { get; }

        private readonly Element _textInputBoundsElement;
        private readonly PointerInputHandlerElement _pointerInputHandler;

        private readonly StringBuilder _stringBuilder = new();

        private int _selectionAnchorIndex;
        private int _selectedFocusIndex;

        private bool _isTextInputBoundsActive;

        public TextInputElement(string text = default,
            string placeholderText = default,
            Color? textColor = default,
            Color? placeholderTextColor = default)
        {
            Text = text;
            PlaceholderText = placeholderText;

            TextElement = new RichTextElement(
                text: Text,
                textColor: textColor ?? Color.Black,
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

            _textInputBoundsElement = ScrollView.Background;
            ScrollView.Background.SpriteSource = BuiltInSprite.TextField;

            ScrollView.ContentClipMask.LeftPadding = DefaultMaskPaddings;
            ScrollView.ContentClipMask.RightPadding = DefaultMaskPaddings;
            ScrollView.ContentClipMask.TopPadding = DefaultMaskPaddings;
            ScrollView.ContentClipMask.BottomPadding = DefaultMaskPaddings;

            AddChild(new ExpandedElement(
                innerElement: ScrollView
            ));

            _pointerInputHandler = new PointerInputHandlerElement(
                blockInput: false
            );
            AddChild(new ExpandedElement(
                innerElement: _pointerInputHandler
            ));

            _pointerInputHandler.PointerMove += OnPointerMove;
            _pointerInputHandler.PointerLeave += OnPointerLeave;

            _pointerInputHandler.PointerDown += OnPointerDown;
            _pointerInputHandler.PointerUp += OnPointerUp;

            _pointerInputHandler.FocusChanged += OnFocusChanged;
        }

        private bool IsPointInsideTextInputBounds(Point point)
            => _textInputBoundsElement.BoundingRectangle.Contains(point);

        private void ResolvePointerCursor(Point position, IPointer pointer)
        {
            var cursor = IsPointInsideTextInputBounds(position)
                ? PointerCursor.IBeam
                : PointerCursor.Arrow;

            pointer.SetCursor(cursor);
        }

        private void RefreshTextSelection()
        {
            var min = Math.Min(_selectionAnchorIndex, _selectedFocusIndex);
            var max = Math.Max(_selectionAnchorIndex, _selectedFocusIndex);

            TextElement.SelectionStart = min;
            TextElement.SelectionLength = max - min;
        }

        void IKeyboardInputHandler.Handle(IKeyboardInputProvider keyboardInputProvider)
        {
        }

        private void OnPointerMove(PointerInputHandlerElement sender,
            PointerEvent pointerEvent)
        {
            if (pointerEvent.IsPrimaryButtonPressed)
            {
                if (_isTextInputBoundsActive)
                {
                    _selectedFocusIndex = TextElement.FindNearestTextIndexUnderPoint(pointerEvent.Position);

                    RefreshTextSelection();
                }

                return;
            }

            ResolvePointerCursor(pointerEvent.Position, pointerEvent.Pointer);
        }

        private void OnPointerLeave(PointerInputHandlerElement sender,
            PointerEvent pointerEvent)
        {
            if (pointerEvent.IsPrimaryButtonPressed)
                return;

            ResolvePointerCursor(pointerEvent.Position, pointerEvent.Pointer);
        }

        private void OnPointerDown(PointerInputHandlerElement sender,
            PointerEvent pointerEvent)
        {
            if (!IsPointInsideTextInputBounds(pointerEvent.Position))
                return;

            _isTextInputBoundsActive = true;

            _selectionAnchorIndex = TextElement.FindNearestTextIndexUnderPoint(pointerEvent.Position);
            _selectedFocusIndex = _selectionAnchorIndex;

            RefreshTextSelection();
        }

        private void OnPointerUp(PointerInputHandlerElement sender, PointerEvent arguments)
        {
            _isTextInputBoundsActive = false;
        }

        private void OnFocusChanged(PointerInputHandlerElement sender,
            PointerFocusEvent pointerEvent)
        {
            ScrollView.Background.SpriteSource = pointerEvent.IsFocused
                ? BuiltInSprite.ActiveTextField
                : BuiltInSprite.TextField;
        }

        void IKeyboardInputHandler.OnKeyDown()
        {
        }

        void IKeyboardInputHandler.OnKeyUp()
        {
        }

        void ITextInputHandler.OnTextInput(string text)
        {
            if (!_pointerInputHandler.IsFocused)
                return;

            _stringBuilder.Append(text);

            TextElement.Text = _stringBuilder.ToString();
        }
    }
}
