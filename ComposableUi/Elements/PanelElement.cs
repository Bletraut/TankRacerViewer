using ComposableUi.Core;
using ComposableUi.Elements;

using Microsoft.Xna.Framework;

using System;
using System.Diagnostics;

namespace ComposableUi
{
    public sealed class PanelElement : PointerInputHandlerElement, IDrawableElement
    {
        public Color Color { get; set; }

        public Color DefaultColor { get; }
        public Color HoverColor => Color.Coral;
        public Color ClickColor => Color.Black;

        private bool _isHover;
        private bool _isPressed;

        public PanelElement(Vector2 size, Color color)
        {
            ApplySize(size);
            Color = DefaultColor = color;
        }

        void IDrawableElement.Draw(IUiRenderer renderer)
        {
            renderer.DrawRectangle(BoundingRectangle, ClipMask, Color);
        }

        public override void OnPointerEnter(Point position)
        {
            base.OnPointerEnter(position);

            _isHover = true;
            Color = _isPressed ? ClickColor : HoverColor;
        }

        public override void OnPointerLeave(Point position)
        {
            base.OnPointerLeave(position);

            _isHover = false;
            Color = DefaultColor;
        }

        public override void OnPointerDown(Point position)
        {
            base.OnPointerDown(position);

            _isPressed = true;
            Color = ClickColor;
        }

        public override void OnPointerUp(Point position)
        {
            base.OnPointerUp(position);

            _isPressed = false;
            Color = _isHover ? HoverColor : DefaultColor;
        }

        public override void OnPointerClick(Point position)
        {
            base.OnPointerClick(position);

            ApplySize(Size * 1.2f);
        }

        public override void OnPointerSecondaryClick(Point position)
        {
            base.OnPointerSecondaryClick(position);

            ApplySize(Size * 0.8f);
        }

        public override void OnPointerDrag(Point delta)
        {
            base.OnPointerDrag(delta);

            LocalPosition += delta.ToVector2();
        }
    }
}
