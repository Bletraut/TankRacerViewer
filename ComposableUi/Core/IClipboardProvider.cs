namespace ComposableUi
{
    public interface IClipboardProvider
    {
        string GetText();
        void SetText(string text);
    }
}
