using Microsoft.Xna.Framework;

namespace ComposableUi.Core
{
    public interface IPointerInputHandler
    {
        public bool BlockInput { get; set; }

        public void OnScrollWheel(int delta) { }
        public void OnHorizontalScrollWheel(int delta) { }

        public void OnPointerEnter(Point position) { }
        public void OnPointerMove(Point position) { }
        public void OnPointerLeave(Point position) { }

        public void OnPointerDown(Point position) { }
        public void OnPointerUp(Point position) { }

        public void OnPointerSecondaryDown(Point position) { }
        public void OnPointerSecondaryUp(Point position) { }

        public void OnPointerDrag(Point delta) { }

        public void OnPointerClick(Point position) { }
        public void OnPointerSecondaryClick(Point position) { }
    }
}
