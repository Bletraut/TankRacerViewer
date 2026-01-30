namespace ComposableUi
{
    public interface IElementSolver
    {
        public void Handle(Element pointerInputHandler);
        public void Resolve();
    }
}
