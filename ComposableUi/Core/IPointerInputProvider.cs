using Microsoft.Xna.Framework;

namespace ComposableUi
{
    public interface IPointerInputProvider
    {
        public Point PointerPosition { get; }

        public int ScrollWheelValueDelta { get; }
        public int HorizontalScrollWheelValueDelta { get; }

        public bool IsPrimaryButtonDown { get; }
        public bool IsPrimaryButtonPressed { get; }
        public bool IsPrimaryButtonUp { get; }

        public bool IsSecondaryButtonDown { get; }
        public bool IsSecondaryButtonPressed { get; }
        public bool IsSecondaryButtonUp { get; }
    }
}
