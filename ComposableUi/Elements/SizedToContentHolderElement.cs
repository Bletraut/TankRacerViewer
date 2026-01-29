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

        public override void Rebuild(Vector2 size)
        {
            Size = size;

            if (HasActiveInnerElement)
            {
                InnerElement.Rebuild(size);
                InnerElement.LocalPosition = InnerElement.Size * InnerElement.Pivot - Size * Pivot;
            }
        }
    }
}
