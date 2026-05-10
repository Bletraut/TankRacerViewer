namespace ComposableUi
{
    public interface ITextInputProvider
    {
        public bool HasText { get; }
        public string Text { get; }
    }
}
