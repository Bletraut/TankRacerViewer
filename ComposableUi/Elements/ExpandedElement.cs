using Microsoft.Xna.Framework;

namespace ComposableUi
{
    public sealed class ExpandedElement : HolderElement
    {
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

        private bool _expandWidth = true;
        public bool ExpandWidth
        {
            get => _expandWidth;
            set => SetAndChangeState(ref _expandWidth, value);
        }

        private bool _expandHeight = true;
        public bool ExpandHeight
        {
            get => _expandHeight;
            set => SetAndChangeState(ref _expandHeight, value);
        }

        public ExpandedElement(Element innerElement = default,
            float leftPadding = default,
            float rightPadding = default,
            float topPadding = default,
            float bottomPadding = default,
            bool expandWidth = true,
            bool expandHeight = true) 
            : base(innerElement) 
        {
            LeftPadding = leftPadding;
            RightPadding = rightPadding;
            TopPadding = topPadding;
            BottomPadding = bottomPadding;
            ExpandWidth = expandWidth;
            ExpandHeight = expandHeight;
        }

        public override Vector2 CalculatePreferredSize()
        {
            if (Parent != null)
            {
                var paddings = new Vector2(LeftPadding + RightPadding, TopPadding + BottomPadding);
                var preferredSize = Parent.Size - paddings;

                return new Vector2()
                {
                    X = ExpandWidth ? preferredSize.X : Parent.Size.X,
                    Y = ExpandHeight ? preferredSize.Y : Parent.Size.Y
                };
            }

            return Size;
        }

        public override void Rebuild(Vector2 size)
        {
            Size = size;

            if (HasActiveInnerElement)
            {
                var elementSize = InnerElement.CalculatePreferredSize();

                size = new Vector2()
                {
                    X = ExpandWidth ? size.X : elementSize.X,
                    Y = ExpandHeight ? size.Y : elementSize.Y
                };
                InnerElement.Rebuild(size);

                var elementPosition = InnerElement.LocalPosition;
                var preferredPosition = InnerElement.Size * InnerElement.Pivot - Size * Pivot;

                InnerElement.LocalPosition = new Vector2()
                {
                    X = ExpandWidth ? preferredPosition.X : elementPosition.X,
                    Y = ExpandHeight ? preferredPosition.Y : elementPosition.Y
                };
            }

            if (Parent != null)
            {
                var leftPadding = ExpandWidth ? LeftPadding : 0;
                var topPadding = ExpandHeight ? TopPadding : 0;
                LocalPosition = new Vector2(leftPadding, topPadding) + Size * Pivot - Parent.Size * Parent.Pivot;
            }
        }
    }
}
