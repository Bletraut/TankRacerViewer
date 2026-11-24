using System.Diagnostics;

using Microsoft.Xna.Framework;

namespace ComposableUi
{
    public sealed class DefaultKeyboardInputProvider : IKeyboardInputProvider,
        IProviderLifecycle
    {
        private readonly GameWindow _gameWindow;

        public DefaultKeyboardInputProvider(GameWindow gameWindow) 
        {
            _gameWindow = gameWindow;
        }

        private void OnKeyDown(object sender, InputKeyEventArgs arguments)
        {
        }

        void IProviderLifecycle.OnAdded()
        {
            _gameWindow.KeyDown += OnKeyDown;
        }

        void IProviderLifecycle.OnRemoved()
        {
            _gameWindow.KeyDown += OnKeyDown;
        }
    }
}
