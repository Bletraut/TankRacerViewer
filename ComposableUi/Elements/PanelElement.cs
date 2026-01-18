using ComposableUi.Core;

using Microsoft.Xna.Framework;

namespace ComposableUi
{
    public sealed class PanelElement : Element, IDrawableElement
    {
        public Color Color { get; set; }

        public PanelElement(Vector2 size, Color color)
        {
            ApplySize(size);
            Color = color;
        }

        void IDrawableElement.Draw(IUiRenderer renderer)
        {
            var rectangle = new Rectangle((Position - Size * Pivot).ToPoint(), Size.ToPoint());
            renderer.DrawRectangle(rectangle, Color);
        }
    }
}
