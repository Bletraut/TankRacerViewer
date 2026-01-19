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

        public override void ApplySize(Vector2 size)
        {
            base.ApplySize(size);

            if (HasActiveInnerElement)
            {
                InnerElement.ApplySize(size);
                InnerElement.LocalPosition = InnerElement.Size * InnerElement.Pivot - Size * Pivot;
            }
        }
    }
}
