namespace ComposableUi
{
    public sealed class Context
    {
        public RootElement Root { get; }

        public Theme Theme { get; internal set; }

        public Context(Theme theme)
        {
            Root = new RootElement
            {
                Pivot = Alignment.TopLeft
            };
            Root.ApplyContext(this);

            Theme = theme;
        }
    }
}
