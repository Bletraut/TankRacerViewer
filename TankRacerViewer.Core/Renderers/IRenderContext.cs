using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TankRacerViewer.Core
{
    public interface IRenderContext
    {
        public RenderTarget2D RenderTarget { get; }
        public Point Size { get; }

        public event EventHandler<Point> SizeChanged;
    }
}
