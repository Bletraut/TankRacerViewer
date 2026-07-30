namespace ComposableUi
{
    public readonly record struct RichTextToken(TokenType TokenType,
        int StartIndex, int Length);

    public enum TokenType
    {
        LetterOrDigit,
        Symbol,
        Space,
        WordBreak,
        NewLine
    }
}
