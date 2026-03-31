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

        private bool _expandWidth;
        public bool ExpandWidth
        {
            get => _expandWidth;
            set => SetAndChangeState(ref _expandWidth, value);
        }

        private bool _expandHeight;
        public bool ExpandHeight
        {
            get => _expandHeight;
            set => SetAndChangeState(ref _expandHeight, value);
        }

        private bool _propagateToInnerElementChildren;
        public bool PropagateToInnerElementChildren
        {
            get => _propagateToInnerElementChildren;
            set => SetAndChangeState(ref _propagateToInnerElementChildren, value);
        }

        public ExpandedElement(Element innerElement = default,
            float leftPadding = default,
            float rightPadding = default,
            float topPadding = default,
            float bottomPadding = default,
            bool expandWidth = true,
            bool expandHeight = true,
            bool propagateToInnerElementChildren = default)
            : base(innerElement)
        {
            LeftPadding = leftPadding;
            RightPadding = rightPadding;
            TopPadding = topPadding;
            BottomPadding = bottomPadding;
            ExpandWidth = expandWidth;
            ExpandHeight = expandHeight;
            PropagateToInnerElementChildren = propagateToInnerElementChildren;
        }

        private void ExpandChild(Element child, Vector2 size, bool skipRebuildChildren)
        {
            var elementSize = child.CalculatePreferredSize();

            size = new Vector2()
            {
                X = ExpandWidth ? size.X : elementSize.X,
                Y = ExpandHeight ? size.Y : elementSize.Y
            };
            if (child is ParentElement parent)
            {
                parent.Rebuild(size, skipRebuildChildren);
            }
            else
            {
                child.Rebuild(size);
            }

            var elementPosition = child.LocalPosition;
            var preferredPosition = child.PivotOffset - child.Parent.PivotOffset;

            child.LocalPosition = new Vector2()
            {
                X = ExpandWidth ? preferredPosition.X : elementPosition.X,
                Y = ExpandHeight ? preferredPosition.Y : elementPosition.Y
            };
        }

        public override Vector2 CalculatePreferredSize()
        {
            if (Parent is not null)
            {
                var paddings = new Vector2(LeftPadding + RightPadding, TopPadding + BottomPadding);
                var preferredSize = Parent.Size - paddings;

                return preferredSize;
            }

            return Size;
        }

        public override void Rebuild(Vector2 size, bool excludeChildren)
        {
            Size = size;

            if (Parent is not null)
                LocalPosition = new Vector2(LeftPadding, TopPadding) + PivotOffset - Parent.PivotOffset;

            var shouldRebuildInnerElement = !excludeChildren && HasEnabledInnerElement;
            if (shouldRebuildInnerElement)
            {
                var skipRebuildChildren = PropagateToInnerElementChildren;
                ExpandChild(InnerElement, size, skipRebuildChildren);

                if (PropagateToInnerElementChildren && InnerElement is ParentElement parent)
                {
                    for (var i = 0; i < parent.ChildCount; i++)
                    {
                        var child = parent.GetChildAt(i);
                        if (!child.IsEnabled)
                            continue;

                        ExpandChild(parent.GetChildAt(i), size, false);
                    }
                }
            }
        }
    }
}
