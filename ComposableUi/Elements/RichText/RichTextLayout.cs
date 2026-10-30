using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ComposableUi
{
    internal sealed class RichTextLayout
    {
        // Static.
        private static readonly Dictionary<SpriteFont, Dictionary<char, SpriteFont.Glyph>> _glyphCache = [];

        // Class.
        public float Width { get; private set; }
        public float Height { get; private set; }

        public ReadOnlySpan<RichTextCharacter> Characters => CollectionsMarshal.AsSpan(_characters);
        public ReadOnlySpan<RichTextLine> Lines => CollectionsMarshal.AsSpan(_lines);

        private float CharacterSpacing => _currentOptions.SpriteFont?.Spacing ?? 0 + _currentOptions.CharacterSpacing;
        private float WordSpacing => _currentOptions.WordSpacing;
        private float LineSpacing => _currentOptions.SpriteFont?.LineSpacing ?? 0 + _currentOptions.LineSpacing;

        private readonly List<RichTextCharacter> _characters = [];
        private readonly List<RichTextLine> _lines = [];

        private RichTextLayoutOptions _currentOptions;

        private Line _currentLine;
        private Line _lineBeforeWordBreak;

        private SpriteFont.Glyph? _defaultGlyph;

        public void RebuildIfNeeded(in RichTextLayoutOptions options)
        {
            if (_currentOptions.Equals(options))
                return;

            _currentOptions = options;

            var spriteFont = _currentOptions.SpriteFont;
            if (spriteFont is null)
                return;

            var isNewParagraph = true;

            _characters.Clear();
            _lines.Clear();

            _currentLine = Line.Empty;
            _lineBeforeWordBreak = Line.Empty;

            var useTypographicWidth = _currentOptions.HorizontalAlignmentBoundsMode is TextBoundsMode.Typographic;
            var characterSpacing = CharacterSpacing;
            var wordSpacing = WordSpacing;

            if (!_glyphCache.TryGetValue(spriteFont, out var glyphs))
            {
                glyphs = spriteFont.GetGlyphs();
                _glyphCache.Add(spriteFont, glyphs);
            }   

            if (spriteFont.DefaultCharacter.HasValue)
                _defaultGlyph = glyphs[spriteFont.DefaultCharacter.Value];

            var tokens = _currentOptions.Tokens;
            for (int i = 0; i < tokens.Count; i++)
            {
                var token = tokens[i];
                switch (token.TokenType)
                {
                    case TokenType.LetterOrDigit or TokenType.Symbol or TokenType.Whitespace:
                        var isWhitespace = token.TokenType is TokenType.Whitespace;
                        var skipLeadingWhitespace = isWhitespace
                            && !_currentOptions.PreserveWhitespace
                            && !isNewParagraph
                            && _currentLine.Length == 0;
                        if (skipLeadingWhitespace)
                            continue;

                        var character = _currentOptions.Text[token.StartIndex];
                        if (!TryGetGlyph(glyphs, character, out var glyph))
                            continue;

                        var glyphWidth = glyph.Width;
                        if (isWhitespace)
                            glyphWidth += wordSpacing;

                        var additionalGlyphWidth = glyph.LeftSideBearing + glyphWidth;
                        if (useTypographicWidth)
                            additionalGlyphWidth += glyph.RightSideBearing;

                        var shouldWrapWord = _currentOptions.WordWrap
                            && _currentOptions.Multiline
                            && _currentLine.Length > 0
                            && _currentLine.Width + additionalGlyphWidth > _currentOptions.Size.X;
                        if (shouldWrapWord)
                        {
                            isNewParagraph = false;

                            if (_lineBeforeWordBreak.Length > 0)
                            {
                                _currentLine.Width -= _lineBeforeWordBreak.Width;
                                AddNewLine(_lineBeforeWordBreak.Length, true);

                                if (_currentLine.Length > 0)
                                {
                                    var firstGlyphOfLine = _characters[^_currentLine.Length].Glyph;
                                    if (!useTypographicWidth)
                                        _currentLine.Width -= firstGlyphOfLine.LeftSideBearing;
                                }
                            }
                            else
                            {
                                AddNewLine(_currentLine.Length, true);
                            }

                            i--;
                            continue;
                        }
                        else if (_currentLine.Length <= 0)
                        {
                            _currentLine.Width = useTypographicWidth 
                                ? glyph.LeftSideBearing 
                                : 0;
                        }
                        else
                        {
                            _currentLine.Width += characterSpacing + glyph.LeftSideBearing;
                        }

                        _characters.Add(new RichTextCharacter()
                        {
                            TokenIndex = i,
                            TokenType = token.TokenType,
                            Position = Vector2.Zero,
                            FontTexture = spriteFont.Texture,
                            Glyph = glyph,
                        });
                        _currentLine.Length++;

                        if (isWhitespace)
                            _currentLine.WhitespaceCount++;

                        _currentLine.Width += glyphWidth + glyph.RightSideBearing;
                        break;
                    case TokenType.WordBreak:
                        if (!_currentOptions.WordWrap)
                            continue;

                        _lineBeforeWordBreak.Width = _currentLine.Width;
                        _lineBeforeWordBreak.Length = _currentLine.Length;
                        break;
                    case TokenType.LineBreak:
                        if (!_currentOptions.Multiline)
                            continue;

                        _characters.Add(new RichTextCharacter()
                        {
                            TokenIndex = i,
                            TokenType = token.TokenType,
                            Position = Vector2.Zero,
                            FontTexture = spriteFont.Texture,
                            Glyph = default,
                        });
                        _currentLine.Length++;

                        AddNewLine(_currentLine.Length, false);

                        isNewParagraph = true;
                        break;
                }
            }
            AddNewLine(_currentLine.Length, false);

            MeasureLines();
            //ApplyAlignment();
        }

        private bool TryGetGlyph(Dictionary<char, SpriteFont.Glyph> glyphs, char character,
            out SpriteFont.Glyph glyph)
        {
            if (!glyphs.TryGetValue(character, out glyph))
            {
                if (!_defaultGlyph.HasValue)
                    return false;

                glyph = _defaultGlyph.Value;
            }

            return true;
        }

        private void AddNewLine(int length, bool isJustifiable)
        {
            if (length <= 0)
                return;

            var newLength = length;
            if (!_currentOptions.PreserveWhitespace)
            {
                var minWhitespaceIndex = int.MaxValue;
                var maxWhitespaceIndex = int.MinValue;
                for (var i = 0; i < length; i++)
                {
                    var index = _characters.Count - _currentLine.Length + length - i - 1;
                    var character = _characters[index];

                    if (character.TokenType is TokenType.LineBreak)
                        continue;

                    if (character.TokenType is not TokenType.Whitespace)
                        break;

                    minWhitespaceIndex = Math.Min(minWhitespaceIndex, index);
                    maxWhitespaceIndex = Math.Max(maxWhitespaceIndex, index);
                }

                if (maxWhitespaceIndex >= minWhitespaceIndex)
                {
                    var count = maxWhitespaceIndex - minWhitespaceIndex + 1;
                    _characters.RemoveRange(minWhitespaceIndex, count);

                    newLength -= count;
                    _currentLine.WhitespaceCount -= count;
                }
            }
            _lines.Add(new RichTextLine(newLength, _currentLine.WhitespaceCount,
                isJustifiable, Vector2.Zero, Vector2.Zero));

            _currentLine.Length -= length;
            _lineBeforeWordBreak.Length = 0;
            _lineBeforeWordBreak.WhitespaceCount = 0;
            _lineBeforeWordBreak.Width = 0;
        }

        private void MeasureLines()
        {
            var characterSpacing = CharacterSpacing;
            var wordSpacing = WordSpacing;
            var lineSpacing = LineSpacing;

            var minTypographicPosition = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
            var maxTypographicPosition = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
            var minGeometricPosition = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
            var maxGeometricPosition = new Vector2(float.NegativeInfinity, float.NegativeInfinity);

            var offset = Vector2.Zero;

            var characterIndex = 0;
            foreach (var line in _lines)
            {
                var isFirstCharacterOfLine = true;
                for (var i = 0; i < line.Length; i++, characterIndex++)
                {
                    var character = _characters[characterIndex];

                    if (isFirstCharacterOfLine)
                    {
                        isFirstCharacterOfLine = false;
                        offset.X = MathF.Max(0, character.Glyph.LeftSideBearing);
                    }
                    else
                    {
                        offset.X += characterSpacing + character.Glyph.LeftSideBearing;
                    }

                    var glyphPosition = character.Glyph.Cropping.Location.ToVector2();
                    var position = glyphPosition + offset;
                    _characters[characterIndex] = character with
                    {
                        Position = position
                    };

                    offset.X += character.Glyph.Width + character.Glyph.RightSideBearing;
                }

                offset.Y += lineSpacing;
            }
        }

        private void ApplyAlignment()
        {
            var characterSpacing = CharacterSpacing;
            var wordSpacing = WordSpacing;
            var lineSpacing = LineSpacing;

            var offset = Vector2.Zero;

            var characterIndex = 0;
            foreach (var line in _lines)
            {
                var isFirstCharacterOfLine = true;
                for (var i = 0; i < line.Length; i++)
                {
                    var character = _characters[characterIndex];

                    if (isFirstCharacterOfLine)
                    {
                        isFirstCharacterOfLine = false;
                        offset.X = MathF.Max(0, character.Glyph.LeftSideBearing);
                    }
                    else
                    {
                        offset.X += characterSpacing + character.Glyph.LeftSideBearing;
                    }

                    _characters[characterIndex] = character with
                    {
                        Position = character.Position + offset
                    };

                    offset.X += character.Glyph.Width + character.Glyph.RightSideBearing;

                    characterIndex++;
                }

                offset.Y += lineSpacing;
            }
        }

        private struct Line
        {
            public static Line Empty => new();

            public int Length;

            public int WhitespaceCount;

            public float Width;
        }
    }
}
