using Microsoft.Xna.Framework;

namespace ComposableUi
{
    public class AspectRatioFitterElement : HolderElement
    {
        public const float DefaultAspectRatio = 16f / 9;

        private float _aspectRatio;
        public float AspectRatio
        {
            get => _aspectRatio;
            set => SetAndChangeState(ref _aspectRatio, value);
        }

        private AspectRatioMode _aspectRatioMode;
        public AspectRatioMode AspectRatioMode
        {
            get => _aspectRatioMode;
            set => SetAndChangeState(ref _aspectRatioMode, value);
        }

        public AspectRatioFitterElement(Element innerElement = default,
            float aspectRatio = DefaultAspectRatio,
            AspectRatioMode aspectRatioMode = AspectRatioMode.WidthControlsHeight,
            Vector2? size = default,
            Vector2? pivot = default)
            : base(innerElement, size, pivot)
        {
            AspectRatio = aspectRatio;
            AspectRatioMode = aspectRatioMode;
        }

        private void AlignInnerElementToCenter(Vector2 preferredSize)
        {
            InnerElement.LocalPosition = Size * Alignment.Center - PivotOffset
                - preferredSize * (Alignment.Center - InnerElement.Pivot);
        }

        public override Vector2 CalculatePreferredSize()
        {
            if (Parent is not null)
                return Parent.Size;

            return Size;
        }

        public override void Rebuild(Vector2 size, bool excludeChildren)
        {
            Size = size;

            if (Parent is not null)
                LocalPosition = PivotOffset - Parent.PivotOffset;

            if (excludeChildren)
                return;

            if (!HasActiveInnerElement)
                return;

            var elementPreferredSize = InnerElement.CalculatePreferredSize();
            var preferredSize = Vector2.Zero;
            switch (AspectRatioMode)
            {
                case AspectRatioMode.WidthControlsHeight:
                    preferredSize = new Vector2(elementPreferredSize.X, elementPreferredSize.X / AspectRatio);
                    break;
                case AspectRatioMode.HeightControlsWidth:
                    preferredSize = new Vector2(elementPreferredSize.Y * AspectRatio, elementPreferredSize.Y);
                    break;
                case AspectRatioMode.FitInParent:
                    preferredSize = new Vector2(size.X, size.X / AspectRatio);
                    if (preferredSize.Y > size.Y)
                        preferredSize = new Vector2(size.Y * AspectRatio, size.Y);
                    AlignInnerElementToCenter(preferredSize);
                    break;
                case AspectRatioMode.EnvelopeParent:
                    preferredSize = new Vector2(size.X, size.X / AspectRatio);
                    if (preferredSize.Y < size.Y)
                        preferredSize = new Vector2(size.Y * AspectRatio, size.Y);
                    AlignInnerElementToCenter(preferredSize);
                    break;
            }
            InnerElement.Rebuild(preferredSize);
        }
    }

    public enum AspectRatioMode
    {
        WidthControlsHeight,
        HeightControlsWidth,
        FitInParent,
        EnvelopeParent
    }
}
