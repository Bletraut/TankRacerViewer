using System.Collections.Generic;

using Microsoft.Xna.Framework;

namespace ComposableUi
{
    public sealed class ColumnLayout : LineLayout
    {
        public ColumnLayout(IReadOnlyList<Element> children = default,
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
            : base(Vector2.UnitY, Vector2.UnitX,
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
                  mainAxisChildrenExpandingMode)
        { }
    }
}
