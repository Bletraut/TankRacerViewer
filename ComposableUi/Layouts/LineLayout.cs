using Microsoft.Xna.Framework;

using System.Collections.Generic;

namespace ComposableUi
{
    public abstract class LineLayout : ContainerElement
    {
        // Static.
        private static float CalculateExpandingFactor(Vector2 axis, ExpandingMode mode, LayoutElement element)
        {
            var factor = mode switch
            {
                ExpandingMode.FlexFactor => element.FlexFactor,
                ExpandingMode.Size => Vector2.Dot(axis, element.InnerElement.Size),
                _ => throw new System.NotImplementedException()
            };

            return factor;
        }

        private static float CalculateExpandingFactor(Vector2 axis, ExpandingMode mode, Element element)
        {
            var factor = mode switch
            {
                ExpandingMode.FlexFactor => 1,
                ExpandingMode.Size => Vector2.Dot(axis, element.Size),
                _ => throw new System.NotImplementedException()
            };

            return factor;
        }

        // Class.
        private Vector2 _alignmentFactor;
        public Vector2 AlignmentFactor
        {
            get => _alignmentFactor;
            set => SetAndChangeState(ref _alignmentFactor, value);
        }

        private float _spacing;
        public float Spacing
        {
            get => _spacing;
            set => SetAndChangeState(ref _spacing, value);
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

        private bool _sizeMainAxisToContent;
        public bool SizeMainAxisToContent
        {
            get => _sizeMainAxisToContent;
            set => SetAndChangeState(ref _sizeMainAxisToContent, value);
        }

        private bool _sizeCrossAxisToContent;
        public bool SizeCrossAxisToContent
        {
            get => _sizeCrossAxisToContent;
            set => SetAndChangeState(ref _sizeCrossAxisToContent, value);
        }

        private bool _expandChildrenMainAxis;
        public bool ExpandChildrenMainAxis
        {
            get => _expandChildrenMainAxis;
            set => SetAndChangeState(ref _expandChildrenMainAxis, value);
        }

        private bool _expandChildrenCrossAxis;
        public bool ExpandChildrenCrossAxis
        {
            get => _expandChildrenCrossAxis;
            set => SetAndChangeState(ref _expandChildrenCrossAxis, value);
        }

        private ExpandingMode _mainAxisChildrenExpandingMode;
        public ExpandingMode MainAxisChildrenExpandingMode
        {
            get => _mainAxisChildrenExpandingMode;
            set => SetAndChangeState(ref _mainAxisChildrenExpandingMode, value);
        }

        public Vector2 MainAxis { get; }
        public Vector2 CrossAxis { get; }

        public LineLayout(Vector2 mainAxis, Vector2 crossAxis,
            IReadOnlyList<Element> children = default,
            Vector2 alignmentFactor = default,
            float spacing = default,
            float leftPadding = default,
            float rightPadding = default,
            float topPadding = default,
            float bottomPadding = default,
            bool sizeMainAxisToContent = default,
            bool sizeCrossAxisToContent = default,
            bool expandChildrenMainAxis = default,
            bool expandChildrenCrossAxis = default,
            ExpandingMode mainAxisChildrenExpandingMode = default)
            : base(children)
        {
            MainAxis = mainAxis;
            CrossAxis = crossAxis;

            AlignmentFactor = alignmentFactor;

            Spacing = spacing;
            LeftPadding = leftPadding;
            RightPadding = rightPadding;
            TopPadding = topPadding;
            BottomPadding = bottomPadding;

            SizeMainAxisToContent = sizeMainAxisToContent;
            SizeCrossAxisToContent = sizeCrossAxisToContent;
            ExpandChildrenMainAxis = expandChildrenMainAxis;
            ExpandChildrenCrossAxis = expandChildrenCrossAxis;
            MainAxisChildrenExpandingMode = mainAxisChildrenExpandingMode;
        }

        private Vector2 CalculatePreferredChildrenSize()
        {
            var activeChildCount = 0;
            var maxChildSize = Vector2.Zero;
            var preferredChildrenSize = Vector2.Zero;

            for (var i = 0; i < ChildCount; i++)
            {
                var child = GetChildAt(i);
                if (!child.IsEnabled)
                    continue;

                if (child is LayoutElement layoutElement)
                {
                    if (!layoutElement.HasActiveInnerElement)
                        continue;

                    if (layoutElement.IgnoreLayout)
                        continue;

                    child = layoutElement.InnerElement;
                }

                var childSize = child.CalculatePreferredSize();
                preferredChildrenSize += MainAxis * childSize;
                maxChildSize = Vector2.Max(maxChildSize, childSize);

                activeChildCount++;
            }
            preferredChildrenSize += MainAxis * (activeChildCount - 1) * Spacing;
            preferredChildrenSize += CrossAxis * maxChildSize;

            return preferredChildrenSize;
        }

        private (Vector2 Spacing, float ExpandingFactor) CalculateMainAxisSpacingAndExpandingFactor()
        {
            var totalExpandingFactor = 0f;
            var activeChildCount = 0;

            for (var i = 0; i < ChildCount; i++)
            {
                var child = GetChildAt(i);
                if (!child.IsEnabled)
                    continue;

                if (child is LayoutElement layoutElement)
                {
                    if (layoutElement.IgnoreLayout)
                        continue;

                    if (!layoutElement.HasActiveInnerElement)
                        continue;

                    totalExpandingFactor += CalculateExpandingFactor(MainAxis,
                        MainAxisChildrenExpandingMode, layoutElement);
                }
                else
                {
                    totalExpandingFactor += CalculateExpandingFactor(MainAxis,
                        MainAxisChildrenExpandingMode, child);
                }

                activeChildCount++;
            }

            return (MainAxis * (activeChildCount - 1) * Spacing, totalExpandingFactor);
        }

        public override Vector2 CalculatePreferredSize()
        {
            var isSizeToContentEnabled = SizeMainAxisToContent || SizeCrossAxisToContent;

            var size = base.CalculatePreferredSize();
            if (ChildCount == 0)
                return isSizeToContentEnabled ? Vector2.Zero : size;

            if (!isSizeToContentEnabled)
                return size;

            var preferredChildrenSize = CalculatePreferredChildrenSize();
            var preferredSize = (SizeMainAxisToContent ? preferredChildrenSize : size) * MainAxis
                + (SizeCrossAxisToContent ? preferredChildrenSize : size) * CrossAxis
                + new Vector2(LeftPadding + RightPadding, TopPadding + BottomPadding);

            return preferredSize;
        }

        public override void Rebuild(Vector2 size, bool excludeChildren)
        {
            Size = size;

            if (excludeChildren)
                return;

            var paddings = new Vector2(LeftPadding + RightPadding, TopPadding + BottomPadding);
            var (totalSpacing, totalExpandingFactor) = CalculateMainAxisSpacingAndExpandingFactor();
            var mainAxisPreferredChildrenSize = MainAxis * (!ExpandChildrenMainAxis
                ? CalculatePreferredChildrenSize()
                : Size);
            var totalMainAxisLayoutOffset = MainAxis * (PivotOffset - AlignmentFactor * (Size - mainAxisPreferredChildrenSize));

#warning Implement offset correctly. For now it works only for left alignment.
            var offset = new Vector2(LeftPadding, TopPadding);

            for (var i = 0; i < ChildCount; i++)
            {
                var child = GetChildAt(i);
                if (!child.IsEnabled)
                    continue;

                float expandingFactor;
                if (child is LayoutElement layoutElement)
                {
                    if (!layoutElement.HasActiveInnerElement)
                        continue;

                    layoutElement.Rebuild(size);
                    layoutElement.LocalPosition = layoutElement.PivotOffset - PivotOffset;

                    if (layoutElement.IgnoreLayout)
                    {
                        var innerElementSize = layoutElement.InnerElement.CalculatePreferredSize();
                        layoutElement.InnerElement.Rebuild(innerElementSize);

                        continue;
                    }

                    child = layoutElement.InnerElement;
                    expandingFactor = CalculateExpandingFactor(MainAxis,
                        MainAxisChildrenExpandingMode, layoutElement);
                }
                else
                {
                    expandingFactor = CalculateExpandingFactor(MainAxis,
                        MainAxisChildrenExpandingMode, child);
                }

                var childSize = child.CalculatePreferredSize();
                if (ExpandChildrenMainAxis || ExpandChildrenCrossAxis)
                {
                    var mainAxisSize = MainAxis * (ExpandChildrenMainAxis
                        ? (mainAxisPreferredChildrenSize - totalSpacing - paddings) * (expandingFactor / totalExpandingFactor)
                        : childSize);
                    var crossAxisSize = CrossAxis * (ExpandChildrenCrossAxis 
                        ? Size - paddings
                        : childSize);

                    childSize = mainAxisSize + crossAxisSize;
                }
                child.Rebuild(childSize);

                var crossAxisLayoutOffset = CrossAxis * (PivotOffset - AlignmentFactor * (Size - childSize));
                child.LocalPosition = offset + child.PivotOffset
                    - (totalMainAxisLayoutOffset + crossAxisLayoutOffset);

                totalMainAxisLayoutOffset -= MainAxis * (childSize + new Vector2(Spacing));
            }
        }
    }

    public enum ExpandingMode
    {
        FlexFactor,
        Size
    }
}
