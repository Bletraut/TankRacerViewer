using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ComposableUi
{
    internal sealed class RichTextLayout
    {
        // Static.
        private static readonly Dictionary<SpriteFont, Dictionary<char, SpriteFont.Glyph>> _glyphCache = [];

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
        public ReadOnlySpan<RichTextCharacter> Characters => CollectionsMarshal.AsSpan(_characters);
        public ReadOnlySpan<int> Lines => CollectionsMarshal.AsSpan(_lines);

        private readonly List<RichTextCharacter> _characters = [];
        private readonly List<int> _lines = [];

        private RichTextLayoutOptions _currentOptions;

        public void RebuildIfNeeded(in RichTextLayoutOptions options)
        {
            if (_currentOptions.Equals(options))
                return;

            _currentOptions = options;

            var spriteFont = _currentOptions.SpriteFont;
            if (spriteFont is null)
                return;

            _characters.Clear();
            _lines.Clear();

            if (!_glyphCache.TryGetValue(spriteFont, out var glyphs))
            {
                glyphs = spriteFont.GetGlyphs();
                _glyphCache.Add(spriteFont, glyphs);
            }

            SpriteFont.Glyph? defaultGlyph = null;
            if (spriteFont.DefaultCharacter.HasValue)
                defaultGlyph = glyphs[spriteFont.DefaultCharacter.Value];

            var isNewParagraph = true;

            var lineWidth = 0f;
            var lineWidthBeforeWordBreak = 0f;
            var lineLength = 0;
            var lineLengthBeforeWordBreak = 0;

            var tokens = _currentOptions.Tokens;
            for (int i = 0; i < tokens.Count; i++)
            {
                var token = tokens[i];
                switch (token.TokenType)
                {
                    case TokenType.LetterOrDigit or TokenType.Symbol or TokenType.Whitespace:
                        var skipLeadingWhitespace = token.TokenType is TokenType.Whitespace
                            && !_currentOptions.PreserveWhitespace
                            && !isNewParagraph
                            && lineLength == 0;
                        if (skipLeadingWhitespace)
                            continue;

                        var character = _currentOptions.Text[token.StartIndex];
                        if (!TryGetGlyph(character, out var glyph))
                            continue;

                        var shouldWrapWord = _currentOptions.WordWrap
                            && _currentOptions.Multiline
                            && lineLength > 0
                            && lineWidth + glyph.LeftSideBearing + glyph.Width > _currentOptions.Size.X;
                        if (shouldWrapWord)
                        {
                            isNewParagraph = false;

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
                                AddNewLine(lineLength);
                            }

                            i--;
                            continue;
                        }
                        else if (lineLength <= 0)
                        {
                            lineWidth = MathF.Max(0, glyph.LeftSideBearing);
                        }
                        else
                        {
                            lineWidth += spriteFont.Spacing + _currentOptions.CharacterSpacing + glyph.LeftSideBearing;
                        }

                        _characters.Add(new RichTextCharacter()
                        {
                            TokenIndex = i,
                            TokenType = token.TokenType,
                            Position = new Vector2(glyph.Cropping.X, glyph.Cropping.Y),
                            FontTexture = spriteFont.Texture,
                            Glyph = glyph,
                        });
                        lineLength++;

                        lineWidth += glyph.Width + glyph.RightSideBearing;
                        break;
                    case TokenType.WordBreak:
                        if (!_currentOptions.WordWrap)
                            continue;

                        lineWidthBeforeWordBreak = lineWidth;
                        lineLengthBeforeWordBreak = lineLength;
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
                        lineLength++;

                        AddNewLine(lineLength);

                        isNewParagraph = true;
                        break;
                }
            }
            AddNewLine(lineLength);

            ApplyAlignment();

            bool TryGetGlyph(char character, out SpriteFont.Glyph glyph)
            {
                if (!glyphs.TryGetValue(character, out glyph))
                {
                    if (!defaultGlyph.HasValue)
                        return false;

                    glyph = defaultGlyph.Value;
                }

                return true;
            }

            void AddNewLine(int length)
            {
                if (length <= 0)
                    return;

                var characterCount = length;
                if (!_currentOptions.PreserveWhitespace)
                {
                    var minWhitespaceIndex = int.MaxValue;
                    var maxWhitespaceIndex = int.MinValue;
                    for (var i = 0; i < length; i++)
                    {
                        var index = _characters.Count - lineLength + length - i - 1;
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

                        characterCount -= count;
                    }
                }
                _lines.Add(characterCount);

                lineLength -= length;
                lineWidthBeforeWordBreak = 0;
                lineLengthBeforeWordBreak = 0;
            }
        }

        private void ApplyAlignment()
        {
            var spriteFont = _currentOptions.SpriteFont;

            var characterSpacing = spriteFont.Spacing + _currentOptions.CharacterSpacing;
            var lineSpacing = spriteFont.LineSpacing + _currentOptions.LineSpacing;

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
    }
}
