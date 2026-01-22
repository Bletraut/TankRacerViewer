using Microsoft.Xna.Framework;

namespace ComposableUi
{
    public class ClipMaskElement : SizedToContentHolderElement
    {
        private bool _enableMask;
        public bool EnableMask
        {
            get => _enableMask;
            set => SetAndChangeState(ref _enableMask, value);
        }

        public ClipMaskElement(Element innerElement = default,
            bool enableMask = true)
            : base(innerElement) 
        { 
            _enableMask = enableMask;
        }

        protected internal override Rectangle? CalculateClipMask()
        {
            if (!EnableMask)
                return Parent?.ClipMask;

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
