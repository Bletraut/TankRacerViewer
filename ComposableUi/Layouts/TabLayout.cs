namespace ComposableUi
{
    public sealed class TabLayout : ContainerElement
    {
        public void AddTab(TabElement tab)
        {
            AddChild(tab);
        }
    }
}
