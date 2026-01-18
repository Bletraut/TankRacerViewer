using Microsoft.Xna.Framework;

using System.Collections.Generic;

namespace ComposableUi
{
    public sealed class ColumnLayout : LineLayout
    {
        protected override Vector2 MainAxis => Vector2.UnitY;
        protected override Vector2 CrossAxis => Vector2.UnitX;

        public ColumnLayout(IReadOnlyList<Element> children = default,
            Vector2 alignmentFactor = default,
            float spacing = default,
            bool sizeMainAxisToContent = default,
            bool sizeCrossAxisToContent = default,
            bool expandChildrenMainAxisSize = default,
            bool expandChildrenCrossAxisSize = default)
            : base(children,
                  alignmentFactor,
                  spacing,
                  sizeMainAxisToContent,
                  sizeCrossAxisToContent,
                  expandChildrenMainAxisSize,
                  expandChildrenCrossAxisSize) { }
    }
}
