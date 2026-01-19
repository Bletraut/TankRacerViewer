using Microsoft.Xna.Framework;

namespace ComposableUi.Core
{
    public interface IPointerInputHandler
    {
        public bool BlockInput { get; set; }

        public void OnPointerOver(Vector2 position) { }
        public void OnPointerMove(Vector2 position) { }
        public void OnPointerOut(Vector2 position) { }

        public void OnPointerDrag(Vector2 delta) { }

        public void OnPointerDown(Vector2 position) { }
        public void OnPointerUp(Vector2 position) { }

        public void OnPointerClick(Vector2 position) { }
    }
}
