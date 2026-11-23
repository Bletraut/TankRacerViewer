using Microsoft.Xna.Framework;

namespace ComposableUi
{
    public class ClipMaskElement : SizedToContentHolderElement
    {
        public static Rectangle? CalculateElementSelfClipMask(Element element)
            => CalculateElementSelfClipMask(element, 0, 0, 0, 0);

        public static Rectangle? CalculateElementSelfClipMask(Element element,
            float leftPadding, float rightPadding, float topPadding, float bottomPadding)
        {
            var maskPosition = element.Position - element.PivotOffset;
            var offset = new Vector2(leftPadding, topPadding);

            var clipMask = new Rectangle()
            {
                Location = (maskPosition + offset).ToPoint(),
                Size = (element.Size - offset - new Vector2(rightPadding, bottomPadding)).ToPoint()
            };

            if (element.Parent is null)
                return clipMask;

            var parentMask = element.Parent.ClipMask;
            if (!parentMask.HasValue)
                return clipMask;

            return Rectangle.Intersect(parentMask.Value, clipMask);
        }

        private float _leftPadding;
        public float LeftPadding
        {
            get => _leftPadding;
            set => SetAndChangeState(ref _leftPadding, value);
        }

        private float _rightPadding;
        public float RightPadding
        {
            get => _rightPadding;
            set => SetAndChangeState(ref _rightPadding, value);
        }

        private float _topPadding;
        public float TopPadding
        {
            get => _topPadding;
            set => SetAndChangeState(ref _topPadding, value);
        }

        private float _bottomPadding;
        public float BottomPadding
        {
            get => _bottomPadding;
            set => SetAndChangeState(ref _bottomPadding, value);
        }

        private bool _isMaskEnabled;
        public bool IsMaskEnabled
        {
            get => _isMaskEnabled;
            set => SetAndChangeState(ref _isMaskEnabled, value);
        }

        public ClipMaskElement(Element innerElement = default,
            float leftPadding = default,
            float rightPadding = default,
            float topPadding = default,
            float bottomPadding = default,
            bool enableMask = true)
            : base(innerElement)
        {
            LeftPadding = leftPadding;
            RightPadding = rightPadding;
            TopPadding = topPadding;
            BottomPadding = bottomPadding;

            _isMaskEnabled = enableMask;
        }

        protected internal override Rectangle? CalculateClipMask()
        {
            if (!IsMaskEnabled)
                return Parent?.ClipMask;

            return CalculateElementSelfClipMask(this,
                _leftPadding, _rightPadding, _topPadding, _bottomPadding);
        }
    }
}
