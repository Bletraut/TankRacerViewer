using System;
using System.Collections.Generic;

using FastFileUnpacker;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TankRacerViewer.Core
{
    public sealed class WorldRenderer
    {
        private const int TransparentLayerCount = 5;

        private const float DefaultWorldScale = 0.01f;
        private const float DefaultBackgroundSize = 0.75f;

        // Static.
        private static Texture2D _whitePixelTexture;
        private static Texture2D _grayPixelTexture;

        // Class.
        public IRenderContext RenderContext { get; private set; }

        private bool HasRenderContext => RenderContext != null;

        private readonly GraphicsDevice _graphicsDevice;
        private readonly SpriteBatch _spriteBatch;

        private readonly Effect _modelEffect;
        private readonly Effect _compositeEffect;
        private readonly Effect _backgroundEffect;
        private readonly Effect _clearEffect;

        private RenderTarget2D _opaqueRenderTarget;
        private RenderTarget2D _depthRenderTarget;
        private RenderTarget2D[] _transparentLayerRenderTargets;
        private RenderTarget2D _currentTransparentDepthRenderTarget;
        private RenderTarget2D _nextTransparentDepthRenderTarget;
        private RenderTarget2D _tempTransparentDepthRenderTarget;

        private readonly RenderTargetBinding[] _renderTargetBindings = new RenderTargetBinding[2];

        private readonly Matrix _worldScaleMatrix;
        private readonly List<(ModelAssetView modelAssetView, Matrix matrix, Camera Camera)> _renderQueue = [];
        private Camera _lastCamera;

        public WorldRenderer(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch,
            ContentManager contentManager)
        {
            _graphicsDevice = graphicsDevice;
            _spriteBatch = spriteBatch;

            _modelEffect = contentManager.Load<Effect>("Effects\\ModelEffect");
            _compositeEffect = contentManager.Load<Effect>("Effects\\CompositeEffect");
            _backgroundEffect = contentManager.Load<Effect>("Effects\\BackgroundEffect");
            _clearEffect = contentManager.Load<Effect>("Effects\\ClearEffect");

            if (_whitePixelTexture == null)
            {
                _whitePixelTexture = new Texture2D(_graphicsDevice, 1, 1);
                _whitePixelTexture.SetData([Color.White]);
            }
            if (_grayPixelTexture == null)
            {
                _grayPixelTexture = new Texture2D(_graphicsDevice, 1, 1);
                _grayPixelTexture.SetData([Color.Gray]);
            }

            _worldScaleMatrix = Matrix.CreateScale(DefaultWorldScale);
        }

        public void ApplyRenderContext(IRenderContext context)
        {
            if (RenderContext == context)
                return;

            if (RenderContext is not null)
                RenderContext.SizeChanged -= OnRenderContextSizeChanged;

            RenderContext = context;
            RenderContext.SizeChanged += OnRenderContextSizeChanged;

            RecreateRenderTargets();
        }

        public void Begin(Color clearColor = default, float depth = 1)
        {
            if (!HasRenderContext)
                return;

            _renderTargetBindings[0] = _opaqueRenderTarget;
            _renderTargetBindings[1] = _depthRenderTarget;

            _graphicsDevice.SetRenderTargets(_renderTargetBindings);

            _clearEffect.Parameters["DepthValue"].SetValue(depth);
            _spriteBatch.Begin(effect:_clearEffect);
            _spriteBatch.Draw(_whitePixelTexture, new Rectangle(0, 0, _graphicsDevice.Viewport.Width, _graphicsDevice.Viewport.Height), clearColor);
            _spriteBatch.End();
        }

        public void End()
        {
            if (!HasRenderContext)
                return;

            DrawRenderQueue();
            Present();
        }

        public void Draw(BackgroundAssetView backgroundAssetView, Camera camera)
        {
            if (!HasRenderContext)
                return;

            _graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;

            var yaw = camera.Yaw;
            var pitch = camera.Pitch;
            var aspectRatio = _graphicsDevice.Viewport.AspectRatio;
            var scale = new Vector3(1f / aspectRatio, 1f, 1f) * DefaultBackgroundSize;

            var scaleMatrix = Matrix.CreateScale(scale);

            var angle = MathHelper.TwoPi / backgroundAssetView.ModelAssetViews.Count;
            var length = backgroundAssetView.ModelAssetViews.Count * scale.X * 2;
            var halfLength = length / 2;

            var maxPitch = MathHelper.ToRadians(0);
            var minPitch = MathHelper.ToRadians(-45);
            var pitchProgress = MathHelper.Clamp((pitch - minPitch) / (maxPitch - minPitch), 0f, 1f);
            var y = MathHelper.Lerp(scale.Y * 3 - 1f, 1f - scale.Y, pitchProgress);

            for (var i = 0; i < backgroundAssetView.ModelAssetViews.Count; i++)
            {
                var modelAssetView = backgroundAssetView.ModelAssetViews[i];

                var x = scale.X + scale.X * 2 * (i + yaw / angle);
                if (x > halfLength)
                    x -= length;

                var translationMatrix = Matrix.CreateTranslation(x, y, 0);
                _backgroundEffect.Parameters["ModelMatrix"]?.SetValue(scaleMatrix * translationMatrix);

                DrawMeshParts(modelAssetView.Opaque,
                    RasterizerState.CullClockwise, BlendState.NonPremultiplied, _backgroundEffect, 0, _whitePixelTexture);
                DrawMeshParts(modelAssetView.OpaqueDoubleSided,
                    RasterizerState.CullNone, BlendState.NonPremultiplied, _backgroundEffect, 0, _whitePixelTexture);
                DrawMeshParts(modelAssetView.Transparent,
                    RasterizerState.CullClockwise, BlendState.NonPremultiplied, _backgroundEffect, 0, _whitePixelTexture);
                DrawMeshParts(modelAssetView.TransparentDoubleSided,
                    RasterizerState.CullNone, BlendState.NonPremultiplied, _backgroundEffect, 0, _whitePixelTexture);
            }
        }

        public void Draw(LevelObjectContainer levelObjectContainer, Camera camera)
        {
            if (!HasRenderContext)
                return;

            for (int i = 0; i < levelObjectContainer.LevelObjects.Count; i++)
            {
                var levelObject = levelObjectContainer.LevelObjects[i];

                if (!levelObject.IsEnabled)
                    continue;

                Draw(levelObject.ModelAssetView, levelObject.ModelMatrix, camera);
            }
        }

        public void Draw(ModelAssetView modelAssetView, Matrix modelMatrix, Camera camera)
        {
            if (!HasRenderContext)
                return;

            _renderQueue.Add((modelAssetView, modelMatrix * _worldScaleMatrix, camera));
        }

        private void DrawRenderQueue()
        {
            _graphicsDevice.DepthStencilState = DepthStencilState.Default;

            DrawAll(DrawOpaqueMeshParts);

            _graphicsDevice.SetRenderTarget(_nextTransparentDepthRenderTarget);
            _graphicsDevice.Clear(Color.Transparent);

            _modelEffect.Parameters["OpaqueDepthSampler"].SetValue(_depthRenderTarget);
            for (var i = 0; i < TransparentLayerCount; i++)
            {
                _renderTargetBindings[0] = _transparentLayerRenderTargets[i];
                _renderTargetBindings[1] = _currentTransparentDepthRenderTarget;

                _graphicsDevice.SetRenderTargets(_renderTargetBindings);
                _graphicsDevice.Clear(Color.Transparent);

                _graphicsDevice.DepthStencilState = DepthStencilState.Default;
                _modelEffect.Parameters["TransparentDepthSampler"]?.SetValue(_nextTransparentDepthRenderTarget);

                DrawAll(DrawTransparentMeshParts);

                _graphicsDevice.SetRenderTarget(_tempTransparentDepthRenderTarget);
                _graphicsDevice.Clear(Color.Transparent);

                _spriteBatch.Begin(blendState: BlendState.Opaque, effect: _compositeEffect);
                _spriteBatch.Draw(_nextTransparentDepthRenderTarget, Vector2.Zero, Color.White);
                _spriteBatch.Draw(_currentTransparentDepthRenderTarget, Vector2.Zero, Color.White);
                _spriteBatch.End();
                (_currentTransparentDepthRenderTarget, _tempTransparentDepthRenderTarget)
                    = (_tempTransparentDepthRenderTarget, _currentTransparentDepthRenderTarget);

                (_currentTransparentDepthRenderTarget, _nextTransparentDepthRenderTarget)
                    = (_nextTransparentDepthRenderTarget, _currentTransparentDepthRenderTarget);
            }

            _lastCamera = null;
            _renderQueue.Clear();
        }

        private void Present()
        {
            _graphicsDevice.SetRenderTarget(RenderContext.RenderTarget);

            _spriteBatch.Begin();
            _spriteBatch.Draw(_opaqueRenderTarget, Vector2.Zero, Color.White);
            for (var i = TransparentLayerCount; i > 0; i--)
                _spriteBatch.Draw(_transparentLayerRenderTargets[i - 1], Vector2.Zero, Color.White);
            _spriteBatch.End();
        }

        private void DrawAll(Action<ModelAssetView> renderHandler)
        {
            foreach (var (modelAssetView, modelMatrix, camera) in _renderQueue)
            {
                if (_lastCamera != camera)
                {
                    _lastCamera = camera;

                    _modelEffect.Parameters["CameraYaw"]?.SetValue(camera.Yaw);
                    _modelEffect.Parameters["ViewProjectionMatrix"]?.SetValue(camera.ViewProjectionMatrix);
                }
                _modelEffect.Parameters["ModelMatrix"]?.SetValue(modelMatrix);
                _modelEffect.Parameters["ModelViewProjectionMatrix"]?.SetValue(modelMatrix * camera.ViewProjectionMatrix);

                renderHandler(modelAssetView);
            }
        }

        private void DrawOpaqueMeshParts(ModelAssetView modelAssetView)
        {
            DrawMeshParts(modelAssetView.Opaque,
                RasterizerState.CullClockwise, BlendState.Opaque, _modelEffect, 0, _grayPixelTexture);
            DrawMeshParts(modelAssetView.OpaqueDoubleSided,
                RasterizerState.CullNone, BlendState.Opaque, _modelEffect, 0, _grayPixelTexture);
        }

        private void DrawTransparentMeshParts(ModelAssetView modelAssetView)
        {
            DrawMeshParts(modelAssetView.Transparent,
                RasterizerState.CullClockwise, BlendState.Opaque, _modelEffect, 1, _grayPixelTexture);
            DrawMeshParts(modelAssetView.TransparentDoubleSided,
                RasterizerState.CullNone, BlendState.Opaque, _modelEffect, 1, _grayPixelTexture);
        }

        private void DrawMeshParts(IReadOnlyList<MeshPart> meshParts,
            RasterizerState rasterizerState, BlendState blendState,
            Effect effect, int passIndex, Texture2D fallbackTexture)
        {
            if (meshParts.Count <= 0)
                return;

            _graphicsDevice.RasterizerState = rasterizerState;
            _graphicsDevice.BlendState = blendState;

            for (int i = 0; i < meshParts.Count; i++)
            {
                var meshPart = meshParts[i];
                var alphaThreshold = meshPart.BlendMode switch
                {
                    BlendMode.Opaque => 0f,
                    BlendMode.AlphaTest => 1f,
                    BlendMode.Transparent => 0.01f,
                    _ => throw new NotImplementedException()
                };

                effect.Parameters["BaseColorSampler"]?.SetValue(meshPart.Texture ?? fallbackTexture);
                effect.Parameters["AlphaThreshold"]?.SetValue(alphaThreshold);
                effect.CurrentTechnique.Passes[passIndex].Apply();

                _graphicsDevice.SetVertexBuffer(meshPart.VertexBuffer);
                _graphicsDevice.Indices = meshPart.IndexBuffer;

                _graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, meshPart.PrimitiveCount);
            }
        }

        private void RecreateRenderTargets()
        {
            var width = RenderContext.Size.X;
            var height = RenderContext.Size.Y;

            _opaqueRenderTarget?.Dispose();
            _depthRenderTarget?.Dispose();

            _opaqueRenderTarget = new RenderTarget2D(_graphicsDevice, width, height,
                false, SurfaceFormat.Color, DepthFormat.Depth24);
            _depthRenderTarget = new RenderTarget2D(_graphicsDevice, width, height,
                false, SurfaceFormat.Single, DepthFormat.None);

            _transparentLayerRenderTargets = new RenderTarget2D[TransparentLayerCount];
            for (var i = 0; i < TransparentLayerCount; i++)
            {
                _transparentLayerRenderTargets[i]?.Dispose();

                _transparentLayerRenderTargets[i] = new RenderTarget2D(_graphicsDevice, width, height,
                    false, SurfaceFormat.Color, DepthFormat.Depth24);
            }

            _currentTransparentDepthRenderTarget?.Dispose();
            _nextTransparentDepthRenderTarget?.Dispose();
            _tempTransparentDepthRenderTarget?.Dispose();

            _currentTransparentDepthRenderTarget = new RenderTarget2D(_graphicsDevice, width, height,
                false, SurfaceFormat.Single, DepthFormat.None);
            _nextTransparentDepthRenderTarget = new RenderTarget2D(_graphicsDevice, width, height,
                false, SurfaceFormat.Single, DepthFormat.None);
            _tempTransparentDepthRenderTarget = new RenderTarget2D(_graphicsDevice, width, height,
                false, SurfaceFormat.Single, DepthFormat.None);

            _renderTargetBindings[0] = _opaqueRenderTarget;
            _renderTargetBindings[1] = _depthRenderTarget;
        }

        private void OnRenderContextSizeChanged(object sender, Point size)
        {
            RecreateRenderTargets();
        }
    }
}
