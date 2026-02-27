using Microsoft.Xna.Framework;

namespace ComposableUi
{
    public abstract class ParentElement : Element
    {
        public abstract int ChildCount { get; }

        public sealed override void Rebuild(Vector2 size)
            => Rebuild(size, false);

        public abstract Element GetChildAt(int index);

        public abstract void AddChild(Element child);
        public abstract void RemoveChild(Element child);

        public abstract void Rebuild(Vector2 size, bool excludeChildren);
    }
}
