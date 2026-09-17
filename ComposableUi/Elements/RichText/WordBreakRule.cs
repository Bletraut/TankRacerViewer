namespace ComposableUi
{
    public class WordBreakRule(char[] triggerCharacters,
        char[] conditionalCharacters,
        bool negateCondition)
    {
        public char[] TriggerCharacters { get; } = triggerCharacters;
        public char[] ConditionalCharacters { get; } = conditionalCharacters;
        public bool NegateCondition { get; } = negateCondition;
    }
}
