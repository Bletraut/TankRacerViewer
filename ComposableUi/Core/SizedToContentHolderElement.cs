using System.Diagnostics;

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

        public override Vector2 CalculatePreferredSize()
        {
            if (HasEnabledInnerElement)
                return InnerElement.CalculatePreferredSize();

            return base.CalculatePreferredSize();
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
