using Microsoft.Xna.Framework;

namespace ComposableUi.Elements
{
    public class ResizableElement : PointerInputHandlerElement
    {
        public Element Target { get; set; }
        public int HandleSize { get; set; }

        public ResizableElement(Element target,
            int handleSize = 3,
            bool blockInput = true,
            Element innerElement = default,
            bool isInteractable = true)
            : base(blockInput,
                  innerElement,
                  isInteractable)
        {
            Target = target;
            HandleSize = handleSize;
        }

        protected override void OnPointerDrag(Point position, Point delta)
        {
            base.OnPointerDrag(position, delta);

            var boundingRectangle = BoundingRectangle;
        }
    }
}
