using System.Collections.Generic;

using Microsoft.Xna.Framework;

namespace ComposableUi
{
    public class LineLayout2 : ContainerElement
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

        private static (T, T) SwapIf<T>(bool condition, T a, T b)
            => condition ? (b, a) : (a, b);

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

        private bool IsMainAxisVertical { get; }

        internal LineLayout2(Vector2 mainAxis,
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
            CrossAxis = new Vector2(mainAxis.Y, mainAxis.X);
            IsMainAxisVertical = MainAxis == Vector2.UnitY;

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

        private Vector2 CalculatePreferredChildrenSize(float? exactMainAxisExtent, float? exactCrossAxisExtent,
            float totalExpandingFactor)
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
                    if (!layoutElement.HasEnabledInnerElement)
                        continue;

                    if (layoutElement.IgnoreLayout)
                        continue;

                    child = layoutElement.InnerElement;
                }

                var (exactWidth, exactHeight) = (exactMainAxisExtent, exactCrossAxisExtent);
                if (exactMainAxisExtent.HasValue)
                {
                    var expandingFactor = CalculateExpandingFactor(MainAxis,
                        MainAxisChildrenExpandingMode, child);
                    exactWidth = exactMainAxisExtent.Value * (expandingFactor /  totalExpandingFactor);
                }
                (exactWidth, exactHeight) = SwapIf(IsMainAxisVertical, exactWidth, exactHeight);

                var childSize = child.CalculatePreferredSize(exactWidth, exactHeight);
                preferredChildrenSize += MainAxis * childSize;
                maxChildSize = Vector2.Max(maxChildSize, childSize);

                activeChildCount++;
            }
            preferredChildrenSize += CrossAxis * maxChildSize;

            return preferredChildrenSize;
        }

        private (float Spacing, float ExpandingFactor) CalculateMainAxisSpacingAndExpandingFactor()
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

                    if (!layoutElement.HasEnabledInnerElement)
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

            return ((activeChildCount - 1) * Spacing, totalExpandingFactor);
        }

        public override Vector2 CalculatePreferredSize(float? exactWidth, float? exactHeight)
        {
            var size = base.CalculatePreferredSize(exactWidth, exactHeight);
            var (mainAxisExtent, crossAxisExtent) = SwapIf(IsMainAxisVertical, size.X, size.Y);

            if (ChildCount <= 0)
            {
                if (SizeMainAxisToContent)
                    mainAxisExtent = 0;

                if (SizeCrossAxisToContent)
                    crossAxisExtent = 0;
            }
            else
            {
                var paddings = new Vector2(LeftPadding + RightPadding, TopPadding + BottomPadding);
                var (mainAxisPaddings, crossAxisPaddings) = SwapIf(IsMainAxisVertical, paddings.X, paddings.Y);
                var (exactMainAxisExtent, exactCrossAxisExtent) = SwapIf(IsMainAxisVertical, exactWidth, exactHeight);

                var (totalSpacing, totalExpandingFactor) = CalculateMainAxisSpacingAndExpandingFactor();

                // If the main axis extent is exact and children should expand,
                // calculate their preferred size using the exact main axis extent.
                // Paddings and spacings are included.
                var shouldExpandMainAxis = exactMainAxisExtent.HasValue && ExpandChildrenMainAxis;
                if (shouldExpandMainAxis)
                    exactMainAxisExtent -= mainAxisPaddings + totalSpacing;

                var shouldExpandCrossAxis = exactCrossAxisExtent.HasValue && ExpandChildrenCrossAxis;
                if (shouldExpandCrossAxis)
                    exactCrossAxisExtent -= crossAxisPaddings;

                var preferredChildrenSize = CalculatePreferredChildrenSize(exactMainAxisExtent, exactCrossAxisExtent,
                    totalExpandingFactor);

                // Add paddings and spacings if children are not expanded.
                if (!shouldExpandMainAxis)
                    preferredChildrenSize += MainAxis * totalSpacing + MainAxis * paddings;

                if (!shouldExpandCrossAxis)
                    preferredChildrenSize += CrossAxis * paddings;

                if (SizeMainAxisToContent)
                    mainAxisExtent = Vector2.Dot(MainAxis, preferredChildrenSize);

                if (SizeCrossAxisToContent)
                    crossAxisExtent = Vector2.Dot(CrossAxis, preferredChildrenSize);
            }

            (size.X, size.Y) = SwapIf(IsMainAxisVertical, mainAxisExtent, crossAxisExtent);
            return size;
        }

        public override void Rebuild(Vector2 size, bool excludeChildren)
        {
            Size = size;

            if (excludeChildren)
                return;
        }

        public enum ExpandingMode
        {
            FlexFactor,
            Size
        }
    }
}
