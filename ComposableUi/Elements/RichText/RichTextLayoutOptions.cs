using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ComposableUi
{
    internal readonly record struct RichTextLayoutOptions(SpriteFont SpriteFont,
        IReadOnlyList<RichTextToken> Tokens, string Text,
        Vector2 Size, bool SizeToTextWidth, bool SizeToTextHeight,
        bool Multiline, bool WordWrap, bool PreserveWhitespace,
        float CharacterSpacing, float WordSpacing, float LineSpacing,
        VerticalAlignmentMode VerticalAlignmentMode,
        HorizontalAlignmentMode HorizontalAlignmentMode);
}
