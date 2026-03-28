using System;
using System.Collections.Generic;
using System.Diagnostics;

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
        public static readonly Color BoundingBoxColor = Color.Fuchsia;

        private static readonly VertexPositionColor[] _unitCubeVertices = [
            // Back.
            // Top.
            new(new Vector3(-1, -1, -1), BoundingBoxColor),
            new(new Vector3(1, -1, -1), BoundingBoxColor),
            // Bottom.
            new(new Vector3(-1, 1, -1), BoundingBoxColor),
            new(new Vector3(1, 1, -1), BoundingBoxColor),
            // Left.
            new(new Vector3(-1, -1, -1), BoundingBoxColor),
            new(new Vector3(-1, 1, -1), BoundingBoxColor),
            // Right.
            new(new Vector3(1, -1, -1), BoundingBoxColor),
            new(new Vector3(1, 1, -1), BoundingBoxColor),

            // Front.
            // Top.
            new(new Vector3(-1, -1, 1), BoundingBoxColor),
            new(new Vector3(1, -1, 1), BoundingBoxColor),
            // Bottom.
            new(new Vector3(-1, 1, 1), BoundingBoxColor),
            new(new Vector3(1, 1, 1), BoundingBoxColor),
            // Left.
            new(new Vector3(-1, -1, 1), BoundingBoxColor),
            new(new Vector3(-1, 1, 1), BoundingBoxColor),
            // Right.
            new(new Vector3(1, -1, 1), BoundingBoxColor),
            new(new Vector3(1, 1, 1), BoundingBoxColor),

            // Left.
            // Top.
            new(new Vector3(-1, -1, -1), BoundingBoxColor),
            new(new Vector3(-1, -1, 1), BoundingBoxColor),
            // Bottom.
            new(new Vector3(-1, 1, -1), BoundingBoxColor),
            new(new Vector3(-1, 1, 1), BoundingBoxColor),

            // Right.
            // Top.
            new(new Vector3(1, -1, -1), BoundingBoxColor),
            new(new Vector3(1, -1, 1), BoundingBoxColor),
            // Bottom.
            new(new Vector3(1, 1, -1), BoundingBoxColor),
            new(new Vector3(1, 1, 1), BoundingBoxColor),
        ];

        private static Texture2D _whitePixelTexture;
        private static Texture2D _grayPixelTexture;

        // Class.
        public IRenderContext RenderContext { get; private set; }

        public Matrix WorldScaleMatrix { get; private set; }

        private bool HasRenderContext => RenderContext is not null;

        private readonly GraphicsDevice _graphicsDevice;
        private readonly SpriteBatch _spriteBatch;

        private readonly BasicEffect _basicEffect;
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

        private readonly List<(ModelAssetView ModelAssetView, Matrix Matrix, Camera Camera)> _renderQueue = [];
        private readonly List<(Matrix Matrix, Camera Camera)> _boundingBoxes = [];
        private Camera _lastCamera;

        private readonly Action<ModelAssetView> _drawOpaqueRenderHandler;
        private readonly Action<ModelAssetView> _drawTransparentRenderHandler;

        public WorldRenderer(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch,
            ContentManager contentManager)
        {
            _graphicsDevice = graphicsDevice;
            _spriteBatch = spriteBatch;

            _basicEffect = new BasicEffect(graphicsDevice)
            {
                VertexColorEnabled = true,
                TextureEnabled = false,
                LightingEnabled = false
            };

            _modelEffect = contentManager.Load<Effect>("Effects\\ModelEffect");
            _compositeEffect = contentManager.Load<Effect>("Effects\\CompositeEffect");
            _backgroundEffect = contentManager.Load<Effect>("Effects\\BackgroundEffect");
            _clearEffect = contentManager.Load<Effect>("Effects\\ClearEffect");

            if (_whitePixelTexture is null)
            {
                _whitePixelTexture = new Texture2D(_graphicsDevice, 1, 1);
                _whitePixelTexture.SetData([Color.White]);
            }
            if (_grayPixelTexture is null)
            {
                _grayPixelTexture = new Texture2D(_graphicsDevice, 1, 1);
                _grayPixelTexture.SetData([Color.Gray]);
            }

            WorldScaleMatrix = Matrix.CreateScale(DefaultWorldScale);

            _drawOpaqueRenderHandler = DrawOpaqueMeshParts;
            _drawTransparentRenderHandler = DrawTransparentMeshParts;
        }

        public void ApplyRenderContext(IRenderContext context)
        {
            if (RenderContext == context)
                return;

            if (RenderContext is not null)
                RenderContext.ResolutionChanged -= OnRenderContextResolutionChanged;

            RenderContext = context;
            RenderContext.ResolutionChanged += OnRenderContextResolutionChanged;

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
            _spriteBatch.Begin(effect: _clearEffect);
            _spriteBatch.Draw(_whitePixelTexture, _graphicsDevice.Viewport.Bounds, clearColor);
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

                if (levelObject.IsBoundingBoxEnabled)
                {
                    var boundingBox = levelObject.ModelAssetView.BoundingBox;
                    var halfSize = (boundingBox.Max - boundingBox.Min) / 2;
                    var center = boundingBox.Min + halfSize;

                    var worldPosition = Vector3.Transform(center, levelObject.ModelMatrix) * DefaultWorldScale;

                    var worldMatrix = Matrix.CreateScale(halfSize * DefaultWorldScale)
                        * Matrix.CreateFromYawPitchRoll(levelObject.Rotation.Y, levelObject.Rotation.X, levelObject.Rotation.Z)
                        * Matrix.CreateTranslation(worldPosition);

                    _boundingBoxes.Add((worldMatrix, camera));
                }
            }
        }

        public void Draw(ModelAssetView modelAssetView, Matrix modelMatrix, Camera camera)
        {
            if (!HasRenderContext)
                return;

            _renderQueue.Add((modelAssetView, modelMatrix * WorldScaleMatrix, camera));
        }

        private void DrawRenderQueue()
        {
            _graphicsDevice.DepthStencilState = DepthStencilState.Default;

            DrawAll(_drawOpaqueRenderHandler);

            DrawBoundingBoxes();

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

                DrawAll(_drawTransparentRenderHandler);

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
            _boundingBoxes.Clear();
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
                effect.Parameters["HighlightColor"]?.SetValue(meshPart.HighlightColor.ToVector4());
                effect.Parameters["AlphaThreshold"]?.SetValue(alphaThreshold);
                effect.CurrentTechnique.Passes[passIndex].Apply();

                _graphicsDevice.SetVertexBuffer(meshPart.VertexBuffer);
                _graphicsDevice.Indices = meshPart.IndexBuffer;

                _graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, meshPart.PrimitiveCount);
            }
        }

        private void DrawBoundingBoxes()
        {
            if (_boundingBoxes.Count <= 0)
                return;

            foreach (var (matrix, camera) in _boundingBoxes)
            {
                _basicEffect.View = matrix * camera.ViewMatrix;
                _basicEffect.Projection = camera.ProjectionMatrix;

                foreach (var pass in _basicEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();

                    _graphicsDevice.DrawUserPrimitives(PrimitiveType.LineList,
                        _unitCubeVertices, 0, _unitCubeVertices.Length / 2);
                }
            }
        }

        private void RecreateRenderTargets()
        {
            var width = RenderContext.Resolution.X;
            var height = RenderContext.Resolution.Y;

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

        private void OnRenderContextResolutionChanged(object sender, Point size)
        {
            RecreateRenderTargets();
        }
    }
}
