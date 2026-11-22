using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ComposableUi
{
    public sealed class RichTextElement : Element,
        IDrawableElement
    {
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

        public Color Color { get; set; }

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

        private TextBoundsMode _horizontalSizingMode;
        public TextBoundsMode HorizontalSizingMode
        {
            get => _horizontalSizingMode;
            set => SetAndChangeState(ref _horizontalSizingMode, value);
        }

        private TextBoundsMode _verticalSizingMode;
        public TextBoundsMode VerticalSizingMode
        {
            get => _verticalSizingMode;
            set => SetAndChangeState(ref _verticalSizingMode, value);
        }

        private bool _multiline;
        public bool Multiline
        {
            get => _multiline;
            set
            {
                if (SetAndChangeState(ref _multiline, value))
                    OnTextChanged();
            }
        }

        private bool _wordWrap;
        public bool WordWrap
        {
            get => _wordWrap;
            set => SetAndChangeState(ref _wordWrap, value);
        }

        private bool _preserveWhitespace;
        public bool PreserveWhitespace
        {
            get => _preserveWhitespace;
            set => SetAndChangeState(ref _preserveWhitespace, value);
        }

        private float _characterSpacing;
        public float CharacterSpacing
        {
            get => _characterSpacing;
            set => SetAndChangeState(ref _characterSpacing, value);
        }

        private float _wordSpacing;
        public float WordSpacing
        {
            get => _wordSpacing;
            set => SetAndChangeState(ref _wordSpacing, value);
        }

        private float _lineSpacing;
        public float LineSpacing
        {
            get => _lineSpacing;
            set => SetAndChangeState(ref _lineSpacing, value);
        }

        private HorizontalAlignmentMode _horizontalAlignmentMode;
        public HorizontalAlignmentMode HorizontalAlignmentMode
        {
            get => _horizontalAlignmentMode;
            set => SetAndChangeState(ref _horizontalAlignmentMode, value);
        }

        private VerticalAlignmentMode _verticalAlignmentMode;
        public VerticalAlignmentMode VerticalAlignmentMode
        {
            get => _verticalAlignmentMode;
            set => SetAndChangeState(ref _verticalAlignmentMode, value);
        }

        private TextBoundsMode _horizontalAlignmentBoundsMode;
        public TextBoundsMode HorizontalAlignmentBoundsMode
        {
            get => _horizontalAlignmentBoundsMode;
            set => SetAndChangeState(ref _horizontalAlignmentBoundsMode, value);
        }

        private TextBoundsMode _verticalAlignmentBoundsMode;
        public TextBoundsMode VerticalAlignmentBoundsMode
        {
            get => _verticalAlignmentBoundsMode;
            set => SetAndChangeState(ref _verticalAlignmentBoundsMode, value);
        }

        private readonly List<RichTextToken> _tokens = [];
        private readonly IReadOnlyList<RichTextToken> _readOnlyTokens;
        public IReadOnlyList<RichTextToken> Tokens
        {
            get
            {
                TokenizeIfDirty();
                return _readOnlyTokens;
            }
        }

        private readonly RichTextLayout _preferredTextLayout = new();
        private readonly RichTextLayout _textLayout = new();

        private readonly List<Sprite> _spritePool = new();
        private readonly List<SpriteDrawCommand> _spriteDrawCommands = new();

        private SpriteFont _currentSpriteFont;

        private bool _isTextDirty;

        private bool _isSpriteDrawCommandsDirty;
        private Rectangle? _lastClipMask;
        private Rectangle _lastBoundingRectangle;

        public RichTextElement(string text = default,
            Color? color = default,
            SpriteFont spriteFont = default,
            Vector2? size = default,
            bool sizeToTextWidth = default,
            bool sizeToTextHeight = default,
            TextBoundsMode horizontalSizingMode = TextBoundsMode.Typographic,
            TextBoundsMode verticalSizingMode = TextBoundsMode.Typographic,
            bool multiline = true,
            bool wordWrap = true,
            bool preserveWhiteSpace = default,
            float characterSpacing = default,
            float wordSpacing = default,
            float lineSpacing = default,
            HorizontalAlignmentMode horizontalAlignmentMode = HorizontalAlignmentMode.Left,
            VerticalAlignmentMode verticalAlignmentMode = VerticalAlignmentMode.Top,
            TextBoundsMode horizontalAlignmentBoundsMode = TextBoundsMode.Typographic,
            TextBoundsMode verticalAlignmentBoundsMode = TextBoundsMode.Typographic)
        {
            _readOnlyTokens = _tokens.AsReadOnly();

            Text = text ?? string.Empty;
            Color = color ?? Color.White;
            SpriteFont = spriteFont;

            Size = size ?? TextElement.DefaultSize;
            SizeToTextWidth = sizeToTextWidth;
            SizeToTextHeight = sizeToTextHeight;
            HorizontalSizingMode = horizontalSizingMode;
            VerticalSizingMode = verticalSizingMode;

            Multiline = multiline;
            WordWrap = wordWrap;
            PreserveWhitespace = preserveWhiteSpace;

            CharacterSpacing = characterSpacing;
            WordSpacing = wordSpacing;
            LineSpacing = lineSpacing;

            HorizontalAlignmentMode = horizontalAlignmentMode;
            VerticalAlignmentMode = verticalAlignmentMode;
            HorizontalAlignmentBoundsMode = horizontalAlignmentBoundsMode;
            VerticalAlignmentBoundsMode = verticalAlignmentBoundsMode;
        }

        private void TokenizeIfDirty()
        {
            if (!_isTextDirty)
                return;

            _isTextDirty = false;

            _tokens.Clear();
            RichTextTokenizer.Tokenize(Text, _tokens);
        }

        private RichTextLayoutOptions GetRichTextLayoutOptions(Vector2 size, bool wordWrap)
        {
            return new RichTextLayoutOptions(_currentSpriteFont, Tokens, Text, size,
                Multiline, wordWrap, PreserveWhitespace,
                CharacterSpacing, WordSpacing, LineSpacing,
                HorizontalAlignmentMode, VerticalAlignmentMode,
                HorizontalAlignmentBoundsMode, VerticalAlignmentBoundsMode);
        }

        private void RebuildSpriteDrawCommandsIfDirty()
        {
            var clipMask = ClipMask;
            var boundingRectangle = BoundingRectangle;

            if (!_isSpriteDrawCommandsDirty)
            {
                var isChanged = _lastClipMask != clipMask
                    || _lastBoundingRectangle != boundingRectangle;
                if (!isChanged)
                    return;
            }

            _isSpriteDrawCommandsDirty = false;

            _lastClipMask = clipMask;
            _lastBoundingRectangle = boundingRectangle;

            var spriteIndex = 0;
            _spriteDrawCommands.Clear();

            var position = _lastBoundingRectangle.Location.ToVector2();
            foreach (ref readonly var character in _textLayout.Characters)
            {
                var destinationRectangle = new Rectangle()
                {
                    Location = (character.Position + position).ToPoint(),
                    Size = character.Glyph.BoundsInTexture.Size
                };
                var isClipped = _lastClipMask.HasValue
                    && !_lastClipMask.Value.Intersects(destinationRectangle);
                if (isClipped)
                    continue;

                Sprite sprite;
                if (spriteIndex < _spritePool.Count)
                {
                    sprite = _spritePool[spriteIndex];
                }
                else
                {
                    sprite = new Sprite();
                    _spritePool.Add(sprite);
                }

                sprite.Texture = character.FontTexture;
                sprite.SourceRectangle = character.Glyph.BoundsInTexture;

                _spriteDrawCommands.Add(new SpriteDrawCommand(sprite, DrawMode.Simple,
                    destinationRectangle, Color));

                spriteIndex++;
            }

            for (var i = spriteIndex; i < _spritePool.Count; i++)
                _spritePool[i].Texture = null;
        }

        public override Vector2 CalculatePreferredSize(float? exactWidth, float? exactHeight)
        {
            if (SetAndChangeState(ref _currentSpriteFont, SpriteFont ?? Context?.Theme.DefaultSpriteFont))
                OnTextChanged();

            var useSelfSize = _currentSpriteFont is null
                || (!SizeToTextWidth && !SizeToTextHeight);
            if (useSelfSize)
                return base.CalculatePreferredSize(exactWidth, exactHeight);

            var size = new Vector2()
            {
                X = exactWidth ?? Size.X,
                Y = exactHeight ?? Size.Y
            };
            var shouldMeasureTextWidth = !exactWidth.HasValue
                && SizeToTextWidth;
            _preferredTextLayout.RebuildIfNeeded(GetRichTextLayoutOptions(size, !shouldMeasureTextWidth), false);

            var textSize = new Vector2()
            {
                X = HorizontalSizingMode is TextBoundsMode.Typographic
                    ? _preferredTextLayout.TypographicSize.X
                    : _preferredTextLayout.GeometricSize.X,
                Y = VerticalSizingMode is TextBoundsMode.Typographic
                    ? _preferredTextLayout.TypographicSize.Y
                    : _preferredTextLayout.GeometricSize.Y,
            };

            return new Vector2()
            {
                X = SizeToTextWidth ? textSize.X : Size.X,
                Y = SizeToTextHeight ? textSize.Y : Size.Y,
            };
        }

        public override void Rebuild(Vector2 size)
        {
            Size = size;

            if (_textLayout.RebuildIfNeeded(GetRichTextLayoutOptions(size, WordWrap), true))
                _isSpriteDrawCommandsDirty = true;
        }

        void IDrawableElement.Draw(IUiRenderer renderer)
        {
            if (_currentSpriteFont is null)
                return;

            // TODO: Add draw here.
            // Test.
            //if (BuiltInSprite.TextField is ISpriteSource spriteSource)
            if (BuiltInSprite.WhitePixel is ISpriteSource spriteSource)
            {
                var sprite = spriteSource.Resolve(Context);
                if (sprite != null)
                {
                    renderer.DrawSprite(sprite, DrawMode.Sliced,
                        BoundingRectangle, ClipMask, Color.Red);
                }
            }

            RebuildSpriteDrawCommandsIfDirty();

            renderer.DrawSpriteGroup(_spriteDrawCommands, ClipMask);
        }

        private void OnTextChanged()
        {
            _isTextDirty = true;
        }
    }

    public enum HorizontalAlignmentMode
    {
        Left,
        Center,
        Right,
        Justified
    }

    public enum VerticalAlignmentMode
    {
        Top,
        Middle,
        Bottom
    }

    public enum TextBoundsMode
    {
        Typographic,
        Geometric
    }
}
