using System;
using System.Collections.Generic;
using System.Diagnostics;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using static Microsoft.Xna.Framework.Graphics.SpriteFont;

namespace ComposableUi
{
    public sealed class RichTextElement : Element,
        IDrawableElement
    {
        // Static.
        private static readonly Dictionary<SpriteFont, Dictionary<char, SpriteFont.Glyph>> _glyphCache = [];
        private static readonly WordBreakRule[] _wordBreakRules =
        [
            new(['!', '?', '-', '}', '/', '|'],
                [')', ']', '.', ',', '"', '\'', ';', ':'],
                true),
            new ([')', ']', '.', ',', ';', ':', '$', '%', '\\', '+', '№'],
                ['(', '[', '{', '$', '%', '\\', '+', '№'],
                false)
        ];

        public static Dictionary<char, SpriteFont.Glyph> GetCachedGlyphs(SpriteFont spriteFont)
        {
            if (!_glyphCache.TryGetValue(spriteFont, out var glyphs))
            {
                glyphs = spriteFont.GetGlyphs();
                _glyphCache[spriteFont] = glyphs;
            }

            return glyphs;
        }

        public static float MeasureCharWidth(Dictionary<char, SpriteFont.Glyph> glyphs, char character)
        {
            if (!glyphs.TryGetValue(character, out SpriteFont.Glyph glyph))
                return 0;

            var width = glyph.Width + MathF.Max(0, glyph.LeftSideBearing) + MathF.Max(0, glyph.RightSideBearing);
            return width;
        }

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

        public Color Color { get; set; }

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

        public float _wordSpacing;
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

        private readonly List<RichTextCharacter> _characters = [];
        private readonly List<int> _lines = [];

        private SpriteFont _currentSpriteFont;

        private bool _isTextDirty;
        private bool _isTextLayoutDirty;

        public RichTextElement(string text = default,
            SpriteFont spriteFont = default,
            Vector2? size = default,
            bool sizeToTextWidth = default,
            bool sizeToTextHeight = default,
            Color? color = default,
            bool multiline = true,
            bool wordWrap = true,
            bool preserveWhiteSpace = default,
            float characterSpacing = default,
            float wordSpacing = default,
            float lineSpacing = default,
            HorizontalAlignmentMode horizontalAlignmentMode = HorizontalAlignmentMode.Left,
            VerticalAlignmentMode verticalAlignmentMode = VerticalAlignmentMode.Top)
        {
            _readOnlyTokens = _tokens.AsReadOnly();

            Text = text ?? string.Empty;
            SpriteFont = spriteFont;
            Size = size ?? TextElement.DefaultSize;
            SizeToTextWidth = sizeToTextWidth;
            SizeToTextHeight = sizeToTextHeight;
            Color = color ?? Color.White;

            Multiline = multiline;
            WordWrap = wordWrap;
            PreserveWhitespace = preserveWhiteSpace;

            CharacterSpacing = characterSpacing;
            WordSpacing = wordSpacing;
            LineSpacing = lineSpacing;

            HorizontalAlignmentMode = horizontalAlignmentMode;
            VerticalAlignmentMode = verticalAlignmentMode;
        }

        private void TokenizeIfDirty()
        {
            if (!_isTextDirty)
                return;

            _isTextDirty = false;

            _tokens.Clear();

            for (var i = 0; i < Text.Length; i++)
            {
                var currentIndex = i;
                var length = 1;

                var currentCharacter = Text[i];
                char? nextCharacter = currentIndex < Text.Length - 1
                    ? Text[currentIndex + 1]
                    : null;

                var isCaretReturn = currentCharacter == '\r';
                if (isCaretReturn)
                {
                    var hasNextNewLine = nextCharacter.HasValue
                        && nextCharacter.Value == '\n';
                    if (hasNextNewLine)
                    {
                        i++;
                        length++;
                    }

                    _tokens.Add(new RichTextToken(TokenType.NewLine, currentIndex, length));
                    continue;
                }

                var isNewLine = currentCharacter == '\n';
                if (isNewLine)
                {
                    _tokens.Add(new RichTextToken(TokenType.NewLine, currentIndex, length));
                    continue;
                }

                if (char.IsWhiteSpace(currentCharacter))
                {
                    _tokens.Add(new RichTextToken(TokenType.Space, currentIndex, length));
                    _tokens.Add(new RichTextToken(TokenType.WordBreak, currentIndex, 0));
                    continue;
                }

                if (char.IsControl(currentCharacter))
                    continue;

                if (char.IsLetterOrDigit(currentCharacter))
                {
                    _tokens.Add(new RichTextToken(TokenType.LetterOrDigit, currentIndex, length));
                    continue;
                }

                _tokens.Add(new RichTextToken(TokenType.Symbol, currentIndex, length));

                if (!nextCharacter.HasValue)
                    break;

                foreach (var wordBreakRule in _wordBreakRules)
                {
                    if (Array.IndexOf(wordBreakRule.TriggerCharacters, currentCharacter) < 0)
                        continue;
                    
                    var nextCharacterMatchesCondition = Array.IndexOf(wordBreakRule.ConditionalCharacters, nextCharacter.Value) >= 0;
                    if (nextCharacterMatchesCondition == !wordBreakRule.NegateCondition)
                    {
                        _tokens.Add(new RichTextToken(TokenType.WordBreak, currentIndex, 0));
                        break;
                    }
                }
            }
        }

        private void RebuildTextLayoutIfDirty()
        {
            if (_currentSpriteFont is null)
                return;

            if (!_isTextLayoutDirty)
                return;

            _isTextLayoutDirty = false;

            _characters.Clear();
            _lines.Clear();

            if (!_glyphCache.TryGetValue(_currentSpriteFont, out var glyphs))
            {
                glyphs = _currentSpriteFont.GetGlyphs();
                _glyphCache.Add(_currentSpriteFont, glyphs);
            }

            Glyph? defaultGlyph = null;
            if (_currentSpriteFont.DefaultCharacter.HasValue)
                defaultGlyph = glyphs[_currentSpriteFont.DefaultCharacter.Value];

            var lineWidth = 0f;
            var lineWidthBeforeWordBreak = 0f;
            var lineLength = 0;
            var lineLengthBeforeWordBreak = 0;
            var isFirstCharacterOfLine = true;

            var tokens = Tokens;
            for (int i = 0; i < tokens.Count; i++)
            {
                var token = Tokens[i];
                switch (token.TokenType)
                {
                    case TokenType.LetterOrDigit or TokenType.Symbol or TokenType.Space:
                        var character = _text[token.StartIndex];
                        if (!glyphs.TryGetValue(character, out var glyph))
                        {
                            if (!defaultGlyph.HasValue)
                                continue;

                            glyph = defaultGlyph.Value;
                        }

                        var shouldWrapWord = WordWrap
                            && lineWidth + glyph.LeftSideBearing + glyph.Width > Size.X;
                        var shouldAddNewLine = Multiline
                            && (isFirstCharacterOfLine || shouldWrapWord);
                        if (shouldAddNewLine)
                        {
                            isFirstCharacterOfLine = false;

                            if (lineLengthBeforeWordBreak > 0)
                            {
                                lineWidth -= lineWidthBeforeWordBreak;
                                AddNewLine(lineLengthBeforeWordBreak);

                                if (lineLength > 0)
                                {
                                    var firstGlyphOfLine = _characters[^lineLength].Glyph;
                                    lineWidth -= MathF.Min(0, firstGlyphOfLine.LeftSideBearing);
                                }
                            }
                            else
                            {
                                lineWidth = MathF.Max(0, glyph.LeftSideBearing);
                                AddNewLine(lineLength);
                            }
                        }
                        else
                        {
                            lineWidth += _currentSpriteFont.Spacing + CharacterSpacing + glyph.LeftSideBearing;
                        }

                        _characters.Add(new RichTextCharacter()
                        {
                            TokenIndex = i,
                            TokenType = token.TokenType,
                            Position = new Vector2(glyph.Cropping.X, glyph.Cropping.Y),
                            FontTexture = _currentSpriteFont.Texture,
                            Glyph = glyph,
                        });
                        lineLength++;

                        lineWidth += glyph.Width + glyph.RightSideBearing;
                        break;
                    case TokenType.WordBreak:
                        if (!WordWrap)
                            continue;

                        lineWidthBeforeWordBreak = lineWidth;
                        lineLengthBeforeWordBreak = lineLength;
                        break;
                    case TokenType.NewLine:
                        if (!Multiline)
                            continue;

                        isFirstCharacterOfLine = true;

                        _characters.Add(new RichTextCharacter()
                        {
                            TokenIndex = i,
                            Position = Vector2.Zero,
                            FontTexture = _currentSpriteFont.Texture,
                            Glyph = default,
                        });
                        lineLength++;

                        AddNewLine(lineLength);
                        break;
                }
            }
            AddNewLine(lineLength);

            ApplyAlignment();

            void AddNewLine(int length)
            {
                if (length <= 0)
                    return;

                _lines.Add(length);

                lineLength -= length;
                lineWidthBeforeWordBreak = 0;
                lineLengthBeforeWordBreak = 0;
            }
        }

        private void ApplyAlignment()
        {
            var offset = Vector2.Zero;

            var characterIndex = 0;
            foreach (var characterCount in _lines)
            {
                var isFirstCharacterOfLine = true;
                for (var i = 0; i < characterCount; i++)
                {
                    var character = _characters[characterIndex];

                    if (isFirstCharacterOfLine)
                    {
                        isFirstCharacterOfLine = false;
                        offset.X = MathF.Max(0, character.Glyph.LeftSideBearing);
                    }
                    else
                    {
                        offset.X += _currentSpriteFont.Spacing + CharacterSpacing + character.Glyph.LeftSideBearing;
                    }

                    _characters[characterIndex] = character with
                    {
                        Position = character.Position + offset
                    };

                    offset.X += character.Glyph.Width + character.Glyph.RightSideBearing;

                    characterIndex++;
                }

                offset.Y += _currentSpriteFont.LineSpacing + LineSpacing;
            }
        }

        public override Vector2 CalculatePreferredSize()
        {
            if (SetAndChangeState(ref _currentSpriteFont, SpriteFont ?? Context?.Theme.DefaultSpriteFont))
                OnTextChanged();

            return base.CalculatePreferredSize();
        }

        public override void Rebuild(Vector2 size)
        {
            Size = size;
            OnTextLayoutChanged();

            RebuildTextLayoutIfDirty();
        }

        void IDrawableElement.Draw(IUiRenderer renderer)
        {
            if (_currentSpriteFont is null)
                return;

            // Test.
            if (BuiltInSprite.TextField is ISpriteSource spriteSource)
            {
                var sprite = spriteSource.Resolve(Context);
                if (sprite != null)
                {
                    renderer.DrawSprite(sprite, DrawMode.Sliced,
                        BoundingRectangle, ClipMask, Color.DarkGray);
                }
            }

            var position = Position + Size * Alignment.TopLeft - PivotOffset;
            foreach (var character in _characters)
            {
                var sprite = new Sprite()
                {
                    Texture = character.FontTexture,
                    SourceRectangle = character.Glyph.BoundsInTexture
                };
                renderer.DrawSprite(sprite, DrawMode.Simple,
                    new Rectangle((character.Position + position).ToPoint(), character.Glyph.BoundsInTexture.Size),
                    ClipMask, Color);
            }

            // TODO: Add draw here.
            //renderer.DrawSprite(BuiltInSprite.TextField, DrawMode.Sliced,
            //    BoundingRectangle, ClipMask, Color);

            //var wordIndex = 0;
            //var localPosition = -PivotOffset;

            //foreach (var line in _lines)
            //{
            //    for (var i = 0; i < line.WordCount; i++)
            //    {
            //        var word = _words[wordIndex];

            //        renderer.DrawString(SpriteFont, word.Text,
            //            localPosition + Position, null, word.Color);

            //        wordIndex++;
            //        localPosition.X += word.Size.X + SpriteFont.Spacing;
            //    }

            //    localPosition.X = -PivotOffset.X;
            //    localPosition.Y += SpriteFont.LineSpacing;
            //}
        }

        private void OnTextChanged()
        {
            _isTextDirty = true;
            OnTextLayoutChanged();
        }

        private void OnTextLayoutChanged()
        {
            _isTextLayoutDirty = true;
        }
    }

    public enum HorizontalAlignmentMode
    {
        Left,
        Center,
        Right,
        Justified,
        Flush
    }

    public enum VerticalAlignmentMode
    {
        Top,
        Middle,
        Bottom
    }
}
