using System.Collections.Generic;

namespace ComposableUi
{
    public abstract class ParentElement : Element
    {
        public IReadOnlyList<Element> Children { get; protected set; }

        public abstract void AddChild(Element child);
        public abstract void RemoveChild(Element child);
    }
}
