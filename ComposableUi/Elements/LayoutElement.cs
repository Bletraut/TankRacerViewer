namespace ComposableUi
{
    public sealed class LayoutElement : HolderElement
    {
        public bool _ignoreLayout;
        public bool IgnoreLayout
        {
            get => _ignoreLayout;
            set => SetAndChangeState(ref _ignoreLayout, value);
        }

        public float _flexFactor = 1f;
        public float FlexFactor
        {
            get => _flexFactor;
            set => SetAndChangeState(ref _flexFactor, value);
        }

        public LayoutElement(Element innerElement = default) 
            : base(innerElement)
        {
        }
    }
}
