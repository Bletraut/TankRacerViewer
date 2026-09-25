using System.Diagnostics;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ComposableUi
{
    public sealed class TextElement : Element, IDrawableElement
    {
        public static readonly Vector2 DefaultSize = new(200, 50);

        private string _text;
        public string Text
        {
            get => _text;
            set
            {
                if (SetAndChangeState(ref _text, value ?? string.Empty))
                    OnTextChanged();
            }
        }

        private SpriteFont _spriteFont;
        public SpriteFont SpriteFont
        {
            get => _spriteFont;
            set
            {
                if (SetAndChangeState(ref _spriteFont, value))
                    OnTextChanged();
            }
        }

        private Vector2 _textAlignmentFactor;
        public Vector2 TextAlignmentFactor
        {
            get => _textAlignmentFactor;
            set => SetAndChangeState(ref _textAlignmentFactor, value);
        }

        private bool _isTextMaskEnabled;
        public bool IsTextMaskEnabled
        {
            get => _isTextMaskEnabled;
            set => SetAndChangeState(ref _isTextMaskEnabled, value);
        }

        private bool _sizeToTextWidth;
        public bool SizeToTextWidth
        {
            get => _sizeToTextWidth;
            set => SetAndChangeState(ref _sizeToTextWidth, value);
        }

        private bool _sizeToTextHeight;
        public bool SizeToTextHeight
        {
            get => _sizeToTextHeight;
            set => SetAndChangeState(ref _sizeToTextHeight, value);
        }

        public Color Color { get; set; }

        private Vector2 _textSize;
        private Vector2 TextSize
        {
            get
            {
                RecalculateTextSizeIfDirty();
                return _textSize;
            }
        }

        private SpriteFont _currentSpriteFont;

        private bool _isTextSizeDirty = true;

        public TextElement(string text = default,
            SpriteFont spriteFont = default,
            Vector2? size = default,
            Vector2? textAlignmentFactor = default,
            Vector2? pivot = default,
            bool enableTextMask = true,
            bool sizeToTextWidth = default,
            bool sizeToTextHeight = default,
            Color? color = default)
        {
            Text = text ?? string.Empty;
            SpriteFont = spriteFont;
            Size = size ?? DefaultSize;
            TextAlignmentFactor = textAlignmentFactor ?? Alignment.TopLeft;
            Pivot = pivot ?? Alignment.Center;
            IsTextMaskEnabled = enableTextMask;
            SizeToTextWidth = sizeToTextWidth;
            SizeToTextHeight = sizeToTextHeight;
            Color = color ?? Color.White;
        }

        private void RecalculateTextSizeIfDirty()
        {
            if (!_isTextSizeDirty)
                return;

            _isTextSizeDirty = false;
            _textSize = _currentSpriteFont?.MeasureString(Text) ?? Vector2.Zero;
        }

        protected internal override Rectangle? CalculateClipMask()
        {
            if (!IsTextMaskEnabled)
                return Parent?.ClipMask;

            var textSize = TextSize;
            var isTextWithinBounds = textSize.X <= Size.X && textSize.Y <= Size.Y;
            if (isTextWithinBounds)
                return Parent?.ClipMask;

            return ClipMaskElement.CalculateElementSelfClipMask(this);
        }

        public override Vector2 CalculatePreferredSize()
        {
            if (SetAndChangeState(ref _currentSpriteFont, SpriteFont ?? Context?.Theme.DefaultSpriteFont))
                OnTextChanged();

            var textSize = TextSize;

            return new Vector2()
            {
                X = SizeToTextWidth ? textSize.X : Size.X,
                Y = SizeToTextHeight ? textSize.Y : Size.Y
            };
        }

        public override void Rebuild(Vector2 size)
        {
            Size = size;
        }

        void IDrawableElement.Draw(IUiRenderer renderer)
        {
            if (_currentSpriteFont is null)
                return;

            if (string.IsNullOrEmpty(Text))
                return;

            var localPosition = Size * TextAlignmentFactor - PivotOffset
                - TextSize * TextAlignmentFactor;

            renderer.DrawString(_currentSpriteFont, Text, localPosition + Position, ClipMask, Color);
        }

        private void OnTextChanged()
        {
            _isTextSizeDirty = true;
        }
    }
}
