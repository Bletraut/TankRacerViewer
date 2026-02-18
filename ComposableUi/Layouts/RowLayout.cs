using Microsoft.Xna.Framework;

using System.Collections.Generic;

namespace ComposableUi
{
    public sealed class RowLayout : LineLayout
    {
        public RowLayout(IReadOnlyList<Element> children = default,
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
            : base(Vector2.UnitX, Vector2.UnitY,
                  children,
                  alignmentFactor,
                  spacing,
                  leftPadding,
                  rightPadding,
                  topPadding,
                  bottomPadding,
                  sizeMainAxisToContent,
                  sizeCrossAxisToContent,
                  expandChildrenMainAxis,
                  expandChildrenCrossAxis,
                  mainAxisChildrenExpandingMode) { }
    }
}
