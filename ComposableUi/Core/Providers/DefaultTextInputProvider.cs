using System.Text;

using Microsoft.Xna.Framework;

namespace ComposableUi
{
    public sealed class DefaultTextInputProvider : ITextInputProvider,
        IProviderLifecycle,
        IUpdateable
    {
        public bool HasText => _lastBuffer.Length > 0;
        public string Text => _lastBuffer.ToString();

        private readonly GameWindow _gameWindow;

        private StringBuilder _lastBuffer = new();
        private StringBuilder _currentBuffer = new();

        public DefaultTextInputProvider(GameWindow gameWindow) 
        {
            _gameWindow = gameWindow;
        }

        void IProviderLifecycle.OnAdded()
        {
            _gameWindow.TextInput += OnTextInput;
        }

        void IProviderLifecycle.OnRemoved()
        {
            _gameWindow.TextInput -= OnTextInput;
        }

        void IUpdateable.Update(GameTime gameTime)
        {
            (_lastBuffer, _currentBuffer) = (_currentBuffer, _lastBuffer);
            _currentBuffer.Clear();
        }

        private void OnTextInput(object sender, TextInputEventArgs arguments)
        {
            _currentBuffer.Append(arguments.Character);
        }
    }
}
