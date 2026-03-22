using Microsoft.Xna.Framework;

namespace ComposableUi
{
    public class LazyListViewElement : ContainerElement
    {
        private readonly SpriteElement _block;

        public LazyListViewElement()
        {
            Size = new Vector2(300, 8_000);
            Pivot = Alignment.TopLeft;

            var background = new ExpandedElement(
                innerElement: new SpriteElement(
                    skin: StandardSkin.WhitePixel,
                    color: Color.Aqua
                )
            );
            AddChild(background);

            _block = new SpriteElement(
                size: new Vector2(100),
                skin: StandardSkin.WhitePixel,
                color: Color.Black
            )
            {
                Pivot = Alignment.TopLeft
            };
            AddChild(_block);
        }

        public override void Rebuild(Vector2 size, bool excludeChildren)
        {
            Size = size;

            if (excludeChildren)
                return;

            for (var i = 0; i < ChildCount; i++)
            {
                var child = GetChildAt(i);
                if (!child.IsEnabled)
                    continue;

                var childSize = child.CalculatePreferredSize();
                child.Rebuild(childSize);
            }

            var visibleRectangle = ClipMask ?? BoundingRectangle;
            _block.Position = visibleRectangle.Location.ToVector2();
        }
    }
}
