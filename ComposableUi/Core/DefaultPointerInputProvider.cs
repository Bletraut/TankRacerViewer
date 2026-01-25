using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace ComposableUi
{
    public sealed class DefaultPointerInputProvider : IPointerInputProvider, IUpdateable
    {
        Point IPointerInputProvider.PointerPosition => _currentMouseState.Position;

        int IPointerInputProvider.ScrollWheelValue => _currentMouseState.ScrollWheelValue;
        int IPointerInputProvider.HorizontalScrollWheelValue => _currentMouseState.HorizontalScrollWheelValue;

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
