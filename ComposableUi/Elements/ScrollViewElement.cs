using Microsoft.Xna.Framework;

namespace ComposableUi
{
    public class ScrollViewElement : ContainerElement
    {
        public static Vector2 DefaultSize => new(300, 300);

        public ScrollViewElement(Vector2? size = default)
        {
            ApplySize(size ?? DefaultSize);

            AddChild(new ExpandedElement(
                innerElement: new SpriteElement(
                    skin: StandardSkin.RectanglePanel)));
        }
    }
}
