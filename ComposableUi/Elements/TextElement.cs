using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ComposableUi.Elements
{
    public sealed class TextElement : Element
    {
        public static readonly Vector2 DefaultSize = new Vector2(200, 50);

        private string _text;
        public string Text
        {
            get => _text;
            set => SetAndChangeState(ref _text, value);
        }

        private SpriteFont _font;
        public SpriteFont Font
        {
            get => _font;
            set => SetAndChangeState(ref _font, value);
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

        public TextElement(string text = default,
            SpriteFont font = default,
            Vector2? size = default,
            bool sizeToTextWidth = default,
            bool sizeToTextHeight = default)
        {
            Text = text;
            Font = font;
            Size = size ?? DefaultSize;
            SizeToTextWidth = sizeToTextWidth;
            SizeToTextHeight = sizeToTextHeight;
        }
    }
}
