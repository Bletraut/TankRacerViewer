using Microsoft.Xna.Framework;

namespace ComposableUi
{
    public class SizedToContentHolderElement : HolderElement
    {
        public SizedToContentHolderElement(Element innerElement = default)
            : base(innerElement)
        {
            if (HasEnabledInnerElement)
                Size = InnerElement.Size;
        }

        public override Vector2 CalculatePreferredSize(float? exactWidth, float? exactHeight)
        {
            if (HasEnabledInnerElement)
                return InnerElement.CalculatePreferredSize(exactWidth, exactHeight);

            return base.CalculatePreferredSize(exactWidth, exactHeight);
        }

        public override void Rebuild(Vector2 size, bool excludeChildren)
        {
            Size = size;

            var shouldRebuildInnerElement = !excludeChildren && HasEnabledInnerElement;
            if (shouldRebuildInnerElement)
            {
                InnerElement.Size = size;
                InnerElement.LocalPosition = InnerElement.PivotOffset - PivotOffset;
                InnerElement.Rebuild(size);
            }
        }
    }
}
