using ComposableUi;

using Microsoft.Xna.Framework;

namespace TankRacerViewer.Core
{
    public sealed class MenuBar : SizedToContentHolderElement
    {
        public const float DefaultHeight = 30;

        public readonly Color DefaultBackgroundColor = Color.RosyBrown;

        public SpriteElement Background { get; private set; }

        private readonly RowLayout _itemsLayout;

        public MenuBar(float height = DefaultHeight)
        {
            _itemsLayout = new RowLayout(
                alignmentFactor: Alignment.MiddleLeft,
                expandChildrenCrossAxis: true
            )
            {
                Size = new Microsoft.Xna.Framework.Vector2(height, DefaultHeight)
            };

            Background = new SpriteElement(
                skin: StandardSkin.WhitePixel,
                color: DefaultBackgroundColor
            );
            _itemsLayout.AddChild(new LayoutElement(
                ignoreLayout: true,
                innerElement: new ExpandedElement(Background)
            ));

            InnerElement = _itemsLayout;
        }
    }
}
