using Microsoft.Xna.Framework;

namespace ComposableUi
{
    public interface IPointerInputHandler
    {
        public Rectangle InteractionRectangle { get; }

        public bool BlockInput { get; set; }
        public bool IsInteractable { get; set; }

        public void OnScrollWheel(Point position, int delta) { }
        public void OnHorizontalScrollWheel(Point position, int delta) { }

        public void OnPointerEnter(Point position) { }
        public void OnPointerMove(Point position) { }
        public void OnPointerLeave(Point position) { }

        public void OnPointerDown(Point position) { }
        public void OnPointerUp(Point position) { }

        public void OnPointerSecondaryDown(Point position) { }
        public void OnPointerSecondaryUp(Point position) { }

        public void OnPointerDrag(Point position, Point delta) { }

        public void OnPointerClick(Point position) { }
        public void OnPointerSecondaryClick(Point position) { }
    }
}
