using Microsoft.Xna.Framework;

namespace ComposableUi
{
    public record struct RichTextWord(string Text, Vector2 Size,
        int StartIndex, bool HasNextPart, bool HasPreviousPart,
        Vector2 Position,
        Color Color);
}
