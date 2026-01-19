using Microsoft.Xna.Framework;

namespace ComposableUi
{
    public class ClipMaskElement : SizedToContentHolderElement
    {
        public ClipMaskElement(Element innerElement = default)
            : base(innerElement) { }

        protected internal override Rectangle? CalculateClipMask()
        {
            var clipMask = new Rectangle((Position - Size * Pivot).ToPoint(), Size.ToPoint());

            if (Parent == null)
                return clipMask;

            var parentMask = Parent.ClipMask;
            if (!parentMask.HasValue)
                return clipMask;

            return Rectangle.Intersect(parentMask.Value, clipMask);
        }
    }
}
