using Microsoft.Xna.Framework;

namespace ComposableUi
{
    public readonly record struct RichTextLine(int Length, int WhitespaceCount,
        bool IsJustifiable, Vector2 TypographicSize, Vector2 GeometricSize);
}
