using Microsoft.Xna.Framework;

using System.Collections.Generic;

namespace ComposableUi
{
    public sealed class RowLayout : LineLayout
    {
        public RowLayout(IReadOnlyList<Element> children = default,
            Vector2 alignmentFactor = default,
            float spacing = default,
            bool sizeMainAxisToContent = default,
            bool sizeCrossAxisToContent = default,
            bool expandChildrenMainAxisSize = default,
            bool expandChildrenCrossAxisSize = default)
            : base(Vector2.UnitX, Vector2.UnitY,
                  children,
                  alignmentFactor,
                  spacing,
                  sizeMainAxisToContent,
                  sizeCrossAxisToContent,
                  expandChildrenMainAxisSize,
                  expandChildrenCrossAxisSize) { }
    }
}
