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
        public Vector2 TypographicSize { get; private set; }
        public Vector2 GeometricSize { get; private set; }

        public ReadOnlySpan<RichTextCharacter> Characters => CollectionsMarshal.AsSpan(_characters);
        public ReadOnlySpan<RichTextLine> Lines => CollectionsMarshal.AsSpan(_lines);

        private float CharacterSpacing => (_currentOptions.SpriteFont?.Spacing ?? 0) + _currentOptions.CharacterSpacing;
        private float WordSpacing => _currentOptions.WordSpacing;
        private float LineSpacing => (_currentOptions.SpriteFont?.LineSpacing ?? 0) + _currentOptions.LineSpacing;

        private readonly List<RichTextCharacter> _characters = [];
        private readonly List<RichTextLine> _lines = [];

        private RichTextLayoutOptions _currentOptions;

        private Line _currentLine;
        private Line _lineBeforeWordBreak;

        private SpriteFont.Glyph? _defaultGlyph;

        private Vector2 _minTypographicPosition;
        private Vector2 _maxTypographicPosition;
        private Vector2 _minGeometricPosition;
        private Vector2 _maxGeometricPosition;

        public bool RebuildIfNeeded(in RichTextLayoutOptions options,
            bool applyAlignment)
        {
            if (_currentOptions.Equals(options))
                return false;

            _currentOptions = options;

            var spriteFont = _currentOptions.SpriteFont;
            if (spriteFont is null)
                return false;

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

            CalculateLineMetricsAndCharacterPositions();

            if (applyAlignment)
                ApplyAlignment();

            return true;
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
            _currentLine.WhitespaceCount = 0;
            _lineBeforeWordBreak.Length = 0;
            _lineBeforeWordBreak.WhitespaceCount = 0;
            _lineBeforeWordBreak.Width = 0;
        }

        private void CalculateLineMetricsAndCharacterPositions()
        {
            var useTypographicWidth = _currentOptions.HorizontalAlignmentBoundsMode is TextBoundsMode.Typographic;

            var characterSpacing = CharacterSpacing;
            var wordSpacing = WordSpacing;
            var lineSpacing = LineSpacing;

            _minTypographicPosition = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
            _maxTypographicPosition = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
            _minGeometricPosition = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
            _maxGeometricPosition = new Vector2(float.NegativeInfinity, float.NegativeInfinity);

            var offset = Vector2.Zero;

            var characterIndex = 0;
            for (int i = 0; i < _lines.Count; i++)
            {
                RichTextLine line = _lines[i];

                var minLineTypographicPosition = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
                var maxLineTypographicPosition = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
                var minLineGeometricPosition = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
                var maxLineGeometricPosition = new Vector2(float.NegativeInfinity, float.NegativeInfinity);

                offset.X = 0;

                var isFirstCharacterOfLine = true;
                for (var j = 0; j < line.Length; j++, characterIndex++)
                {
                    var character = _characters[characterIndex];
                    var glyph = character.Glyph;

                    minLineTypographicPosition = Vector2.Min(minLineTypographicPosition, offset);
                    maxLineTypographicPosition = Vector2.Max(maxLineTypographicPosition,
                        offset + new Vector2(MathF.Max(0, glyph.WidthIncludingBearings), MathF.Max(lineSpacing, glyph.Cropping.Height)));

                    _minTypographicPosition = Vector2.Min(_minTypographicPosition, minLineTypographicPosition);
                    _maxTypographicPosition = Vector2.Max(_maxTypographicPosition, maxLineTypographicPosition);

                    if (isFirstCharacterOfLine)
                    {
                        isFirstCharacterOfLine = false;

                        if (useTypographicWidth)
                            offset.X = glyph.LeftSideBearing;
                    }
                    else
                    {
                        offset.X += glyph.LeftSideBearing + characterSpacing;
                    }

                    var position = glyph.Cropping.Location.ToVector2() + offset;
                    _characters[characterIndex] = character with
                    {
                        Position = position
                    };

                    var hasWidthOnly = character.TokenType is TokenType.Whitespace or TokenType.LineBreak;
                    if (hasWidthOnly)
                    {
                        minLineGeometricPosition.X = MathF.Min(minLineGeometricPosition.X, offset.X);
                        maxLineGeometricPosition.X = MathF.Max(maxLineGeometricPosition.X, 
                            offset.X + glyph.Width + glyph.RightSideBearing + wordSpacing);
                    }
                    else
                    {
                        minLineGeometricPosition = Vector2.Min(minLineGeometricPosition, position);
                        maxLineGeometricPosition = Vector2.Max(maxLineGeometricPosition,
                            position + glyph.BoundsInTexture.Size.ToVector2());
                    }

                    _minGeometricPosition = Vector2.Min(_minGeometricPosition, minLineGeometricPosition);
                    _maxGeometricPosition = Vector2.Max(_maxGeometricPosition, maxLineGeometricPosition);

                    offset.X += glyph.Width + glyph.RightSideBearing;
                    if (character.TokenType is TokenType.Whitespace)
                        offset.X += wordSpacing;
                }

                offset.Y += lineSpacing;

                _lines[i] = line with
                {
                    TypographicSize = maxLineTypographicPosition - minLineTypographicPosition,
                    GeometricSize = maxLineGeometricPosition - minLineGeometricPosition
                };
            }

            TypographicSize = _maxTypographicPosition - _minTypographicPosition;
            GeometricSize = _maxGeometricPosition - _minGeometricPosition;
        }

        private void ApplyAlignment()
        {
            var height = _currentOptions.VerticalAlignmentBoundsMode is TextBoundsMode.Typographic
                ? TypographicSize.Y
                : GeometricSize.Y;

            var positionOffset = new Vector2()
            {
                X = _currentOptions.VerticalAlignmentBoundsMode is TextBoundsMode.Typographic 
                    ? _minTypographicPosition.X 
                    : _minGeometricPosition.X,
                Y = _currentOptions.VerticalAlignmentBoundsMode is TextBoundsMode.Typographic
                    ? _minTypographicPosition.Y
                    : _minGeometricPosition.Y
            };

            var characterIndex = 0;
            foreach (var line in _lines)
            {
                var lineWidth = _currentOptions.HorizontalAlignmentBoundsMode is TextBoundsMode.Typographic
                    ? line.TypographicSize.X
                    : line.GeometricSize.X;

                var offset = Vector2.Zero;
                offset.X = _currentOptions.HorizontalAlignmentMode switch
                {
                    HorizontalAlignmentMode.Left => 0,
                    HorizontalAlignmentMode.Center => (_currentOptions.Size.X - lineWidth) / 2,
                    HorizontalAlignmentMode.Right => _currentOptions.Size.X - lineWidth,
                    HorizontalAlignmentMode.Justified => 0,
                    _ => throw new NotImplementedException(),
                };
                offset.Y = _currentOptions.VerticalAlignmentMode switch
                {
                    VerticalAlignmentMode.Top => 0,
                    VerticalAlignmentMode.Middle => (_currentOptions.Size.Y - height) / 2,
                    VerticalAlignmentMode.Bottom => _currentOptions.Size.Y - height,
                    _ => throw new NotImplementedException(),
                };
                offset -= positionOffset;

                var isJustified = line.IsJustifiable
                    && _currentOptions.HorizontalAlignmentMode is HorizontalAlignmentMode.Justified;
                var additionalWhitespaceWidth = isJustified
                    ? (_currentOptions.Size.X - lineWidth) / line.WhitespaceCount
                    : 0;

                for (var i = 0; i < line.Length; i++, characterIndex++)
                {
                    var character = _characters[characterIndex];

                    var position = character.Position + offset;
                    _characters[characterIndex] = character with
                    {
                        Position = position
                    };

                    var addAdditionWhitespaceWidth = isJustified
                        && character.TokenType is TokenType.Whitespace;
                    if (addAdditionWhitespaceWidth)
                        offset.X += additionalWhitespaceWidth;
                }
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
