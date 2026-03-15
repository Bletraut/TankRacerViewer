using System;
using System.Collections.Generic;

using ComposableUi;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TankRacerViewer.Core
{
    public sealed class RenderContextElement : Element,
        IDrawableElement,
        IRenderContext
    {
        public static readonly Point DefaultResolution = new(1920, 1080);

        private Point _resolution;
        public Point Resolution
        {
            get => _resolution;
            set
            {
                if (SetAndChangeState(ref _resolution, value))
                {
                    RecreateRenderTarget();
                    ResolutionChanged?.Invoke(this, Resolution);
                }
            }
        }

        public float AspectRatio => (float)_resolution.X / _resolution.Y;

        public RenderTarget2D RenderTarget => _renderTarget;

        public event EventHandler<Point> ResolutionChanged;

        private readonly GraphicsDevice _graphicsDevice;
        private readonly Sprite _sprite = new();

        private RenderTarget2D _renderTarget;

        public RenderContextElement(GraphicsDevice graphicsDevice,
            Point? resolution = default)
        {
            _graphicsDevice = graphicsDevice;
            Resolution = resolution ?? DefaultResolution;
        }

        private void RecreateRenderTarget()
        {
            _renderTarget?.Dispose();

            _renderTarget = new RenderTarget2D(_graphicsDevice, Resolution.X, Resolution.Y,
                false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);

            _sprite.Texture = _renderTarget;
            _sprite.SourceRectangle = _renderTarget.Bounds;
        }

        public override Vector2 CalculatePreferredSize()
            => Resolution.ToVector2();

        void IDrawableElement.Draw(IUiRenderer renderer)
        {
            renderer.DrawSprite(_sprite, DrawMode.Simple,
                BoundingRectangle, ClipMask, Color.White);
        }
    }
}
