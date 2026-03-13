using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TankRacerViewer.Core
{
    public sealed class GameWindowRenderContext : IRenderContext
    {
        public RenderTarget2D RenderTarget => null;
        public Point Size => _window.ClientBounds.Size;

        public event EventHandler<Point> SizeChanged;

        private readonly GameWindow _window;

        public GameWindowRenderContext(GameWindow window)
        {
            _window = window;
            _window.ClientSizeChanged += OnClientSizeChanged;
        }

        private void OnClientSizeChanged(object sender, EventArgs arguments)
        {
            SizeChanged?.Invoke(this, Size);
        }
    }
}
