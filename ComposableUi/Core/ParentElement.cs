namespace ComposableUi
{
    public abstract class ParentElement : Element
    {
        public abstract int ChildCount { get; }

        public abstract Element GetChildAt(int index);

        public abstract void AddChild(Element child);
        public abstract void RemoveChild(Element child);
    }
}
