using Microsoft.Xna.Framework;

namespace ComposableUi
{
    public class ClipMaskElement : SizedToContentHolderElement
    {
        public static Rectangle? CalculateElementSelfClipMask(Element element)
        {
            var maskPosition = element.Position - element.Size * element.Pivot;
            var clipMask = new Rectangle(maskPosition.ToPoint(), element.Size.ToPoint());

            if (element.Parent is null)
                return clipMask;

            var parentMask = element.Parent.ClipMask;
            if (!parentMask.HasValue)
                return clipMask;

            return Rectangle.Intersect(parentMask.Value, clipMask);
        }

        private bool _isMaskEnabled;
        public bool IsMaskEnabled
        {
            get => _isMaskEnabled;
            set => SetAndChangeState(ref _isMaskEnabled, value);
        }

        public ClipMaskElement(Element innerElement = default,
            bool enableMask = true)
            : base(innerElement)
        {
            _isMaskEnabled = enableMask;
        }

        protected internal override Rectangle? CalculateClipMask()
        {
            if (!IsMaskEnabled)
                return Parent?.ClipMask;

            return CalculateElementSelfClipMask(this);
        }
    }
}
