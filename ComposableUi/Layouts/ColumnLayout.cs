using Microsoft.Xna.Framework;

using System.Collections.Generic;

namespace ComposableUi
{
    public sealed class ColumnLayout : LineLayout
    {
        public ColumnLayout(IReadOnlyList<Element> children = default,
            Vector2 alignmentFactor = default,
            float spacing = default,
            bool sizeMainAxisToContent = default,
            bool sizeCrossAxisToContent = default,
            bool expandChildrenMainAxisSize = default,
            bool expandChildrenCrossAxisSize = default)
            : base(Vector2.UnitY, Vector2.UnitX,
                  children,
                  alignmentFactor,
                  spacing,
                  sizeMainAxisToContent,
                  sizeCrossAxisToContent,
                  expandChildrenMainAxisSize,
                  expandChildrenCrossAxisSize) { }
    }
}
