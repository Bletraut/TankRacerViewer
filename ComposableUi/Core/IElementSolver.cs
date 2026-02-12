namespace ComposableUi
{
    public interface IElementSolver
    {
        public void Handle(Element element);
        public void Resolve();
    }
}
