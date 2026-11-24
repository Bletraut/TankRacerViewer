using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace ComposableUi
{
    public sealed class DefaultPointerInputProvider : IPointerInputProvider, IUpdateable
    {
        public MousePointer MousePointer { get; set; } = new MousePointer();

        IPointer IPointerInputProvider.Pointer => MousePointer;

        Point IPointerInputProvider.PointerPosition => _currentMouseState.Position;

        int IPointerInputProvider.ScrollWheelValueDelta
            => _currentMouseState.ScrollWheelValue - _lastMouseState.ScrollWheelValue;
        int IPointerInputProvider.HorizontalScrollWheelValueDelta
            => _currentMouseState.HorizontalScrollWheelValue - _lastMouseState.HorizontalScrollWheelValue;

        bool IPointerInputProvider.IsPrimaryButtonDown
            => _lastMouseState.LeftButton == ButtonState.Released && _currentMouseState.LeftButton == ButtonState.Pressed;
        bool IPointerInputProvider.IsPrimaryButtonPressed
            => _currentMouseState.LeftButton == ButtonState.Pressed;
        bool IPointerInputProvider.IsPrimaryButtonUp
            => _lastMouseState.LeftButton == ButtonState.Pressed && _currentMouseState.LeftButton == ButtonState.Released;

        bool IPointerInputProvider.IsSecondaryButtonDown
            => _lastMouseState.RightButton == ButtonState.Released && _currentMouseState.RightButton == ButtonState.Pressed;
        bool IPointerInputProvider.IsSecondaryButtonPressed
            => _currentMouseState.RightButton == ButtonState.Pressed;
        bool IPointerInputProvider.IsSecondaryButtonUp
            => _lastMouseState.RightButton == ButtonState.Pressed && _currentMouseState.RightButton == ButtonState.Released;

        private MouseState _currentMouseState;
        private MouseState _lastMouseState;

        void IUpdateable.Update(GameTime gameTime)
        {
            _lastMouseState = _currentMouseState;
            _currentMouseState = Mouse.GetState();
        }
    }
}
