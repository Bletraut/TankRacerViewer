using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ComposableUi
{
    public sealed class RichTextElement : Element,
        IDrawableElement
    {
        // Static.
        private static readonly char[] _wordBreakChars = [' ', '!', '?', '-', '/'];

        // Class.
        private string _text;
        public string Text
        {
            get => _text;
            set
            {
                if (SetAndChangeState(ref _text, value))
                    OnTextChanged();
            }
        }

        private SpriteFont _spriteFont;
        public SpriteFont SpriteFont
        {
            get => _spriteFont;
            set
            {
                if (SetAndChangeState(ref _spriteFont, value ?? TextElement.DefaultSpriteFont))
                    OnTextChanged();
            }
        }

        private bool _isMultiline;
        public bool IsMultiline
        {
            get => _isMultiline;
            set
            {
                if (SetAndChangeState(ref _isMultiline, value))
                    OnTextChanged();
            }
        }

        public Color Color { get; set; }

        private bool _isTextDirty;

        public RichTextElement(string text = default,
            SpriteFont spriteFont = default,
            Vector2? size = default,
            Color? color = default)
        {
            Text = text ?? string.Empty;
            SpriteFont = spriteFont;
            Size = size ?? TextElement.DefaultSize;
            Color = color ?? Color.White;
        }

        void IDrawableElement.Draw(IUiRenderer renderer)
        {
            if (SpriteFont is null)
                return;

            // TODO: Add draw here.
            renderer.DrawSkinnedRectangle(StandardSkin.TextField, DrawMode.Sliced,
                BoundingRectangle, ClipMask, Color);
        }

        private void RebuildTextIfDirty()
        {
            if (!_isTextDirty)
                return;

            _isTextDirty = false;
        }

        private void OnTextChanged()
        {
            _isTextDirty = true;
        }
    }
}
