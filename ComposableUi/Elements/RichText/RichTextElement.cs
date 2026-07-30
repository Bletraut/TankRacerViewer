using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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
                if (SetAndChangeState(ref _spriteFont, value ?? TextElement.DefaultSpriteFont))
                    OnTextChanged();
            }
        }

        private Vector2 _textAlignmentFactor;
        public Vector2 TextAlignmentFactor
        {
            get => _textAlignmentFactor;
            set
            {
                if (SetAndChangeState(ref _textAlignmentFactor, value))
                    OnTextLayoutChanged();
            }
        }

        public Color Color { get; set; }

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

        private JustificationMode _justificationMode;
        public JustificationMode JustificationMode
        {
            get => _justificationMode;
            set
            {
                if (SetAndChangeState(ref _justificationMode, value))
                    OnTextChanged();
            }
        }

        private WrappingMode _wrappingMode;
        public WrappingMode WrappingMode
        {
            get => _wrappingMode;
            set
            {
                if (SetAndChangeState(ref _wrappingMode, value))
                    OnTextChanged();
            }
        }

        private List<RichTextToken> _tokens = new();
        private IReadOnlyList<RichTextToken> _readOnlyTokens;
        public IReadOnlyList<RichTextToken> Tokens
        {
            get
            {
                TokenizeIfDirty();
                return _readOnlyTokens;
            }
        }

        private readonly StringBuilder _currentWordBuilder = new();
        private readonly List<RichTextWord> _words = [];
        private readonly List<RichTextLine> _lines = [];

        private float _currentLineWidth;

        private bool _isTextDirty;
        private bool _isTextLayoutDirty;

        public RichTextElement(string text = default,
            SpriteFont spriteFont = default,
            Vector2? size = default,
            Vector2? textAlignmentFactor = default,
            Color? color = default,
            bool isMultiline = default,
            JustificationMode justificationMode = default,
            WrappingMode wrappingMode = default)
        {
            _readOnlyTokens = _tokens.AsReadOnly();

            Text = text ?? string.Empty;
            SpriteFont = spriteFont;
            Size = size ?? TextElement.DefaultSize;
            TextAlignmentFactor = textAlignmentFactor ?? Alignment.TopLeft;
            Color = color ?? Color.White;
            IsMultiline = isMultiline;
            JustificationMode = justificationMode;
            WrappingMode = wrappingMode;
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
            if (!_isTextLayoutDirty)
                return;

            _isTextLayoutDirty = true;

            //_words.Clear();
            //_lines.Clear();
            //_currentWordBuilder.Clear();

            //_currentLineWidth = 0;
            //AddNewLine();

            //var glyphs = GetCachedGlyphs(SpriteFont);

            //var lastCharacterWidth = 0f;
            //var wasWordBreakChar = false;
            //var wasWordWrap = false;

            //for (int i = 0; i < Text.Length; i++)
            //{
            //    char character = Text[i];

            //    var isNewLineChar = character == '\n';
            //    var isWordBreakChar = Array.IndexOf(_wordBreakChars, character) >= 0;

            //    var characterWidth = MeasureCharWidth(glyphs, character);
            //    var hasLineOverflow = WrappingMode is WrappingMode.Wrap
            //        && _currentWordBuilder.Length > 0
            //        && _currentLineWidth + characterWidth > Size.X;

            //    var shouldAddCurrentWord = isNewLineChar
            //        || isWordBreakChar
            //        || wasWordBreakChar;
            //    if (shouldAddCurrentWord)
            //    {
            //        AddCurrentWord(i, false, wasWordWrap);

            //        wasWordBreakChar = isWordBreakChar;
            //        wasWordWrap = false;
            //    }
            //    else if (hasLineOverflow)
            //    {
            //        shouldAddCurrentWord = wasWordWrap
            //            || _lines[^1].WordCount == 0;
            //        if (shouldAddCurrentWord)
            //        {
            //            AddCurrentWord(i, true, wasWordWrap);

            //            wasWordWrap = true;
            //        }
            //        // Word overflow.
            //        else if (lastCharacterWidth + characterWidth > Size.X)
            //        {
            //            AddNewLine();
            //            AddCurrentWord(i, true, wasWordWrap);

            //            wasWordWrap = true;
            //        }
            //    }

            //    var shouldAddNewLine = isNewLineChar
            //        || hasLineOverflow;
            //    if (shouldAddNewLine)
            //        AddNewLine();

            //    if (char.IsControl(character))
            //        continue;

            //    lastCharacterWidth = characterWidth;
            //    _currentLineWidth += lastCharacterWidth;
            //    _currentWordBuilder.Append(character);
            //}
            //AddCurrentWord(Text.Length - 1, false, wasWordWrap);
        }

        private void AddCurrentWord(int index,
            bool hasNextPart, bool hasPreviousPart)
        {
            if (_currentWordBuilder.Length <= 0)
                return;

            var text = _currentWordBuilder.ToString();
            var size = SpriteFont.MeasureString(_currentWordBuilder);

            // debug
            var color = Color.LightGreen;
            if (hasPreviousPart && hasNextPart)
            {
                color = Color.Green;
            }
            else if (hasNextPart)
            {
                color = Color.Blue;
            }
            else if (hasPreviousPart)
            {
                color = Color.Red;
            }
            // end
            _words.Add(new RichTextWord(text, size, index - text.Length,
                hasNextPart, hasPreviousPart,
                Vector2.Zero,
                color));

            var line = _lines[^1];
            _lines[^1] = line with { WordCount = line.WordCount + 1 };

            _currentWordBuilder.Clear();
        }

        private void AddNewLine()
        {
            _currentLineWidth = SpriteFont.MeasureString(_currentWordBuilder).X;
            _lines.Add(new RichTextLine(_words.Count, 0));
        }

        public override void Rebuild(Vector2 size)
        {
            Size = size;

            RebuildTextLayoutIfDirty();
        }

        void IDrawableElement.Draw(IUiRenderer renderer)
        {
            if (SpriteFont is null)
                return;

            // TODO: Add draw here.
            renderer.DrawSkinnedRectangle(StandardSkin.TextField, DrawMode.Sliced,
                BoundingRectangle, ClipMask, Color);

            var wordIndex = 0;
            var localPosition = -PivotOffset;

            foreach (var line in _lines)
            {
                for (var i = 0; i < line.WordCount; i++)
                {
                    var word = _words[wordIndex];

                    renderer.DrawString(SpriteFont, word.Text,
                        localPosition + Position, null, word.Color);

                    wordIndex++;
                    localPosition.X += word.Size.X + SpriteFont.Spacing;
                }

                localPosition.X = -PivotOffset.X;
                localPosition.Y += SpriteFont.LineSpacing;
            }
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

    public enum JustificationMode
    {
        Natural,
        Justified,
        Flush
    }

    public enum WrappingMode
    {
        NoWrap,
        Wrap
    }
}
