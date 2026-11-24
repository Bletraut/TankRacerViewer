namespace ComposableUi
{
    public interface IKeyboardInputHandler
    {
        public void Handle(IKeyboardInputProvider keyboardInputProvider);

        public void OnKeyDown();

        public void OnKeyUp();
    }
}
