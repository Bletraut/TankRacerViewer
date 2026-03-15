using Microsoft.Xna.Framework;

namespace ComposableUi
{
    public class SizedToContentHolderElement : HolderElement
    {
        public SizedToContentHolderElement(Element innerElement = default)
            : base(innerElement) { }

        public override Vector2 CalculatePreferredSize()
        {
            if (HasActiveInnerElement)
                return InnerElement.CalculatePreferredSize();

            return base.CalculatePreferredSize();
        }

        public override void Rebuild(Vector2 size, bool excludeChildren)
        {
            Size = size;

            var shouldRebuildInnerElement = !excludeChildren && HasActiveInnerElement;
            if (shouldRebuildInnerElement)
            {
                InnerElement.Rebuild(size);
                InnerElement.LocalPosition = InnerElement.PivotOffset - PivotOffset;
            }
        }
    }
}
