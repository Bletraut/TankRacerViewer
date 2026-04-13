using ComposableUi.Utilities;

using Microsoft.Xna.Framework;

namespace ComposableUi
{
    public class ResizableElement : PointerInputHandlerElement,
        IPointerInputHandler
    {
        public const int DefaultHandleSize = 6;

        public override Rectangle InteractionRectangle
        {
            get
            {
                var boundingRectangle = BoundingRectangle;
                if (!IsInteractable)
                    return boundingRectangle;

                return new Rectangle()
                {
                    X = boundingRectangle.Left - HandleSize,
                    Y = boundingRectangle.Top - HandleSize,
                    Width = boundingRectangle.Width + HandleSize * 2,
                    Height = boundingRectangle.Height + HandleSize * 2
                };
            }
        }

        private int _handleSize;
        public int HandleSize
        {
            get => _handleSize;
            set => SetAndChangeState(ref _handleSize, value);
        }

        private Vector2 _minSize;
        public Vector2 MinSize
        {
            get => _minSize;
            set => SetAndChangeState(ref _minSize, value);
        }

        public bool IsResizing => SizeDirection != Vector2.Zero;

        protected Vector2 SizeDirection { get; private set; } = Vector2.Zero;
        protected Vector2 PositionDirection { get; private set; } = Vector2.Zero;

        public ResizableElement(Element innerElement = default,
            int handleSize = DefaultHandleSize,
            Vector2 minSize = default,
            bool blockInput = true,
            bool isInteractable = true)
            : base(innerElement,
                  blockInput,
                  isInteractable)
        {
            HandleSize = handleSize;
            MinSize = minSize;
        }

        protected (Vector2 SizeDelta, Vector2 PositionDelta) CalculateResizeDeltas(Vector2 delta)
        {
            var size = InnerElement.Size;
            // Prevents a sudden jump in size if the
            // current size is less than the minimum size.
            var minSize = Vector2.Min(size, MinSize);
            var newSize = Vector2.Max(minSize, size + delta * SizeDirection);

            var sizeDelta = size - newSize;
            var localPositionDelta = sizeDelta * PositionDirection;

            return (-sizeDelta, localPositionDelta);
        }

        private void ResolvePointerCursor(IPointer pointer, Point position)
        {
            var cursor = PointerCursor.Arrow;

            var normal = InteractionRectangle.GetEdgeNormal(HandleSize, position);
            if (normal != Vector2.Zero)
            {
                cursor = normal switch
                {
                    { X: not 0, Y: 0 } => PointerCursor.SizeWE,
                    { X: 0, Y: not 0 } => PointerCursor.SizeNS,
                    { X: not 0, Y: not 0 } when normal.X * normal.Y > 0 => PointerCursor.SizeNWSE,
                    { X: not 0, Y: not 0 } when normal.X * normal.Y < 0 => PointerCursor.SizeNESW,
                    _ => PointerCursor.Arrow
                };
            }

            pointer.SetCursor(cursor);
        }

        protected override void OnPointerMove(in PointerEvent pointerEvent)
        {
            base.OnPointerMove(pointerEvent);

            if (pointerEvent.IsPrimaryButtonPressed)
                return;

            ResolvePointerCursor(pointerEvent.Pointer, pointerEvent.Position);
        }

        protected override void OnPointerLeave(in PointerEvent pointerEvent)
        {
            base.OnPointerLeave(pointerEvent);

            if (pointerEvent.IsPrimaryButtonPressed)
                return;

            pointerEvent.Pointer.SetCursor(PointerCursor.Arrow);
        }

        protected override void OnPointerDown(in PointerEvent pointerEvent)
        {
            base.OnPointerDown(pointerEvent);

            SizeDirection = InteractionRectangle.GetEdgeNormal(HandleSize, pointerEvent.Position);
            PositionDirection = Vector2.Max(Vector2.Zero, -SizeDirection) - Pivot;
        }

        protected override void OnPointerUp(in PointerEvent pointerEvent)
        {
            base.OnPointerUp(pointerEvent);

            if (IsResizing)
                ResolvePointerCursor(pointerEvent.Pointer, pointerEvent.Position);

            SizeDirection = Vector2.Zero;
        }

        protected override void OnPointerFixedDrag(in PointerDragEvent pointerEvent)
        {
            base.OnPointerFixedDrag(pointerEvent);

            var deltaVector = pointerEvent.Delta.ToVector2();
            if (HasEnabledInnerElement)
            {
                var (sizeDelta, localPositionDelta) = CalculateResizeDeltas(deltaVector);

                Size += sizeDelta;
                InnerElement.Size = Size;
                LocalPosition += localPositionDelta;
            }
        }
    }
}
