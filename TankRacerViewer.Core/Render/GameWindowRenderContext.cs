using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TankRacerViewer.Core
{
    public sealed class GameWindowRenderContext : IRenderContext
    {
        public RenderTarget2D RenderTarget => null;
        public Point Resolution => _window.ClientBounds.Size;
        public float AspectRatio => (float)Resolution.X / Resolution.Y;

        public event EventHandler<Point> ResolutionChanged;

        private readonly GameWindow _window;

        public GameWindowRenderContext(GameWindow window)
        {
            _window = window;
            _window.ClientSizeChanged += OnClientSizeChanged;
        }

        private void OnClientSizeChanged(object sender, EventArgs arguments)
        {
            ResolutionChanged?.Invoke(this, Resolution);
        }
    }
}
