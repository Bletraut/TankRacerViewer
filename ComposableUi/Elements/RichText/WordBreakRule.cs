namespace ComposableUi
{
    public record WordBreakRule(char[] TriggerCharacters,
        char[] ConditionalCharacters,
        bool NegateCondition);
}
