namespace ComposableUi
{
    public sealed class RootElement : ContainerElement
    {
        public event ElementEventHandler<RootElement> StateChanged;

        protected internal override void OnStateChanged()
        {
            StateChanged?.Invoke(this);
        }
    }
}
