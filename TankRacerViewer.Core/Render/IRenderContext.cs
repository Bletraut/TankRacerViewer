using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TankRacerViewer.Core
{
    public interface IRenderContext
    {
        public RenderTarget2D RenderTarget { get; }
        public Point Resolution { get; }
        public float AspectRatio { get; }

        public event EventHandler<Point> ResolutionChanged;
    }
}
