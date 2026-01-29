using Microsoft.Xna.Framework;

using System.Collections.Generic;

namespace ComposableUi
{
    public abstract class LineLayout : ContainerElement
    {
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

        private bool _expandChildrenMainAxisSize;
        public bool ExpandChildrenMainAxisSize
        {
            get => _expandChildrenMainAxisSize;
            set => SetAndChangeState(ref _expandChildrenMainAxisSize, value);
        }

        private bool _expandChildrenCrossAxisSize;
        public bool ExpandChildrenCrossAxisSize
        {
            get => _expandChildrenCrossAxisSize;
            set => SetAndChangeState(ref _expandChildrenCrossAxisSize, value);
        }

        public Vector2 MainAxis { get; }
        public Vector2 CrossAxis { get; }

        public LineLayout(Vector2 mainAxis, Vector2 crossAxis,
            IReadOnlyList<Element> children = default,
            Vector2 alignmentFactor = default,
            float spacing = default,
            bool sizeMainAxisToContent = default,
            bool sizeCrossAxisToContent = default,
            bool expandChildrenMainAxisSize = default,
            bool expandChildrenCrossAxisSize = default)
            : base(children)
        {
            MainAxis = mainAxis;
            CrossAxis = crossAxis;

            AlignmentFactor = alignmentFactor;
            Spacing = spacing;
            SizeMainAxisToContent = sizeMainAxisToContent;
            SizeCrossAxisToContent = sizeCrossAxisToContent;
            ExpandChildrenMainAxisSize = expandChildrenMainAxisSize;
            ExpandChildrenCrossAxisSize = expandChildrenCrossAxisSize;
        }

        public override Vector2 CalculatePreferredSize()
        {
            var size = base.CalculatePreferredSize();
            if (ChildCount == 0)
                return size;

            var isSizeToContentEnabled = SizeMainAxisToContent || SizeCrossAxisToContent;
            if (!isSizeToContentEnabled)
                return size;

            var preferredChildrenSize = CalculatePreferredChildrenSize();
            var preferredSize = (SizeMainAxisToContent ? preferredChildrenSize : size) * MainAxis
                + (SizeCrossAxisToContent ? preferredChildrenSize : size) * CrossAxis;

            return preferredSize;
        }

        public override void Rebuild(Vector2 size)
        {
            Size = size;

            var (totalSpacing, totalFlexFactor) = CalculateMainAxisSpacingAndFlexFactor();
            var mainAxisPreferredChildrenSize = MainAxis * (!ExpandChildrenMainAxisSize
                ? CalculatePreferredChildrenSize()
                : Size);
            var totalMainAxisLayoutOffset = MainAxis * (Size * Pivot - AlignmentFactor * (Size - mainAxisPreferredChildrenSize));

            for (var i = 0; i < ChildCount; i++)
            {
                var child = GetChildAt(i);
                if (!child.IsEnabled)
                    continue;

                var flexFactor = 1f;
                if (child is LayoutElement layoutElement)
                {
                    if (!layoutElement.HasActiveInnerElement)
                        continue;

                    layoutElement.Rebuild(size);
                    layoutElement.LocalPosition = layoutElement.Size * layoutElement.Pivot - Size * Pivot;

                    if (layoutElement.IgnoreLayout)
                    {
                        var innerElementSize = layoutElement.InnerElement.CalculatePreferredSize();
                        layoutElement.InnerElement.Rebuild(innerElementSize);

                        continue;
                    }

                    child = layoutElement.InnerElement;
                    flexFactor = layoutElement.FlexFactor;
                }

                var childSize = child.CalculatePreferredSize();
                if (ExpandChildrenMainAxisSize || ExpandChildrenCrossAxisSize)
                {
                    var mainAxisSize = MainAxis * (ExpandChildrenMainAxisSize
                        ? (mainAxisPreferredChildrenSize - totalSpacing) * (flexFactor / totalFlexFactor)
                        : childSize);
                    var crossAxisSize = CrossAxis * (ExpandChildrenCrossAxisSize 
                        ? Size 
                        : childSize);

                    childSize = mainAxisSize + crossAxisSize;
                }
                child.Rebuild(childSize);

                var crossAxisLayoutOffset = CrossAxis * (Size * Pivot - AlignmentFactor * (Size - childSize));
                child.LocalPosition = child.Size * child.Pivot 
                    - (totalMainAxisLayoutOffset + crossAxisLayoutOffset);

                totalMainAxisLayoutOffset -= MainAxis * (childSize + new Vector2(Spacing));
            }
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

        private (Vector2 Spacing, float FlexFactor) CalculateMainAxisSpacingAndFlexFactor()
        {
            var totalFlexFactor = 0f;
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

                    totalFlexFactor += layoutElement.FlexFactor;
                }
                else
                {
                    totalFlexFactor += 1;
                }

                activeChildCount++;
            }

            return (MainAxis * (activeChildCount - 1) * Spacing, totalFlexFactor);
        }
    }
}
