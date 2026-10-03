using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ComposableUi
{
    public sealed class DefaultUiRenderer : IUiRenderer
    {
        // Static.
        private static void SkipClipMaskIfContainsOrCropDestination(ref Rectangle? clipMask,
            ref Rectangle destinationRectangle)
        {
            if (!clipMask.HasValue)
                return;

            if (clipMask.Value.Contains(destinationRectangle))
            {
                clipMask = null;
            }
            else
            {
                destinationRectangle = Rectangle.Intersect(clipMask.Value, destinationRectangle);
            }
        }

        // Class.
        public static Texture2D FallbackTexture { get; private set; }
        public static Sprite FallbackSprite { get; private set; }

        public RenderTarget2D RenderTarget { get; set; }

        public long DrawCount { get; private set; }

        private readonly RasterizerState _scissorRasterizerState = new()
        {
            ScissorTestEnable = true,
        };

        private readonly SpriteBatch _spriteBatch;
        private readonly UiBatcher<RenderCommand> _uiBatcher = new();
        private readonly List<RenderSpriteData> _renderSpriteDataList = [];
        private readonly List<RenderTextData> _renderTextDataList = [];

        private bool _isBeginCalled;
        private Rectangle? _currentClipMask;

        public DefaultUiRenderer(SpriteBatch spriteBatch)
        {
            _spriteBatch = spriteBatch;

            if (FallbackTexture is null)
            {
                var colors = new Color[]
                {
                    Color.Pink, Color.DeepPink, Color.Pink, Color.DeepPink,
                    Color.DeepPink, Color.Pink, Color.DeepPink, Color.Pink,
                    Color.Pink, Color.DeepPink, Color.Pink, Color.DeepPink,
                    Color.DeepPink, Color.Pink, Color.DeepPink, Color.Pink
                };
                FallbackTexture = new Texture2D(spriteBatch.GraphicsDevice, 4, 4);
                FallbackTexture.SetData(colors);

                FallbackSprite = new Sprite()
                {
                    Texture = FallbackTexture,
                    SourceRectangle = new Rectangle(0, 0, FallbackTexture.Width, FallbackTexture.Height),
                    Scale = 2,
                    LeftBorder = 1,
                    RightBorder = 1,
                    TopBorder = 1,
                    BottomBorder = 1
                };
            }
        }

        private void RunDrawSpriteCommand(in RenderCommand command)
        {
            ApplyDrawState(command.ClipMask);

            var data = _renderSpriteDataList[command.Id];
            switch (data.DrawMode)
            {
                case DrawMode.Simple:
                    DrawSimpleSprite(data.Sprite, data.DestinationRectangle, data.Color);
                    break;
                case DrawMode.Sliced:
                    DrawSlicedSprite(data.Sprite, data.DestinationRectangle, data.Color);
                    break;
                default:
                    DrawSimpleSprite(data.Sprite, data.DestinationRectangle, data.Color);
                    break;
            }
        }

        private void RunDrawTextCommand(in RenderCommand command)
        {
            ApplyDrawState(command.ClipMask);

            var data = _renderTextDataList[command.Id];
            _spriteBatch.DrawString(data.SpriteFont, data.Text, data.Position, data.Color);
        }

        private void RunDrawGeometryCommand(in RenderCommand command)
        {
            // TODO: Implement draw logic.
        }

        private void DrawSimpleSprite(Sprite sprite,
            Rectangle destinationRectangle, Color color)
        {
            _spriteBatch.Draw(sprite.Texture, destinationRectangle,
                sprite.SourceRectangle, color);
        }

        private void DrawSlicedSprite(Sprite sprite,
            Rectangle destinationRectangle, Color color)
        {
            if (!sprite.IsSliced)
            {
                DrawSimpleSprite(sprite, destinationRectangle, color);
                return;
            }

            var scale = sprite.Scale;
            var sourceRectangle = sprite.SourceRectangle;

            var leftBorder = sprite.LeftBorder;
            var rightBorder = sprite.RightBorder;
            var topBorder = sprite.TopBorder;
            var bottomBorder = sprite.BottomBorder;

            if (leftBorder + rightBorder > destinationRectangle.Width)
                leftBorder = rightBorder = 0;

            if (topBorder + bottomBorder > destinationRectangle.Height)
                topBorder = bottomBorder = 0;

            // Top left.
            var sliceSourceRectangle = new Rectangle(sourceRectangle.Left,
                sourceRectangle.Top,
                leftBorder,
                topBorder);
            var sliceDestinationRectangle = new Rectangle(destinationRectangle.Left,
                destinationRectangle.Top,
                sliceSourceRectangle.Width * scale,
                sliceSourceRectangle.Height * scale);
            _spriteBatch.Draw(sprite.Texture, sliceDestinationRectangle,
                sliceSourceRectangle, color);

            // Top right.
            sliceSourceRectangle = new Rectangle(sourceRectangle.Right - rightBorder,
                sourceRectangle.Top,
                rightBorder,
                topBorder);
            sliceDestinationRectangle = new Rectangle(destinationRectangle.Right - sliceSourceRectangle.Width * scale,
                destinationRectangle.Top,
                sliceSourceRectangle.Width * scale,
                sliceSourceRectangle.Height * scale);
            _spriteBatch.Draw(sprite.Texture, sliceDestinationRectangle,
                sliceSourceRectangle, color);

            // Bottom left.
            sliceSourceRectangle = new Rectangle(sourceRectangle.Left,
                sourceRectangle.Bottom - bottomBorder,
                leftBorder,
                bottomBorder);
            sliceDestinationRectangle = new Rectangle(destinationRectangle.Left,
                destinationRectangle.Bottom - sliceSourceRectangle.Height * scale,
                sliceSourceRectangle.Width * scale,
                sliceSourceRectangle.Height * scale);
            _spriteBatch.Draw(sprite.Texture, sliceDestinationRectangle,
                sliceSourceRectangle, color);

            // Bottom right.
            sliceSourceRectangle = new Rectangle(sourceRectangle.Right - rightBorder,
                sourceRectangle.Bottom - bottomBorder,
                rightBorder,
                bottomBorder);
            sliceDestinationRectangle = new Rectangle(destinationRectangle.Right - sliceSourceRectangle.Width * scale,
                destinationRectangle.Bottom - sliceSourceRectangle.Height * scale,
                sliceSourceRectangle.Width * scale,
                sliceSourceRectangle.Height * scale);
            _spriteBatch.Draw(sprite.Texture, sliceDestinationRectangle,
                sliceSourceRectangle, color);

            // Left.
            sliceSourceRectangle = new Rectangle(sourceRectangle.Left,
                sourceRectangle.Top + topBorder,
                leftBorder,
                sourceRectangle.Height - topBorder - bottomBorder);
            sliceDestinationRectangle = new Rectangle(destinationRectangle.Left,
                destinationRectangle.Top + topBorder * scale,
                sliceSourceRectangle.Width * scale,
                destinationRectangle.Height - (topBorder + bottomBorder) * scale);
            _spriteBatch.Draw(sprite.Texture, sliceDestinationRectangle,
                sliceSourceRectangle, color);

            // Right.
            sliceSourceRectangle = new Rectangle(sourceRectangle.Right - rightBorder,
                sourceRectangle.Top + topBorder,
                rightBorder,
                sourceRectangle.Height - topBorder - bottomBorder);
            sliceDestinationRectangle = new Rectangle(destinationRectangle.Right - sliceSourceRectangle.Width * scale,
                destinationRectangle.Top + topBorder * scale,
                sliceSourceRectangle.Width * scale,
                destinationRectangle.Height - (topBorder + bottomBorder) * scale);
            _spriteBatch.Draw(sprite.Texture, sliceDestinationRectangle,
                sliceSourceRectangle, color);

            // Top.
            sliceSourceRectangle = new Rectangle(sourceRectangle.Left + leftBorder,
                sourceRectangle.Top,
                sourceRectangle.Width - leftBorder - rightBorder,
                topBorder);
            sliceDestinationRectangle = new Rectangle(destinationRectangle.Left + leftBorder * scale,
                destinationRectangle.Top,
                destinationRectangle.Width - (leftBorder + rightBorder) * scale,
                topBorder * scale);
            _spriteBatch.Draw(sprite.Texture, sliceDestinationRectangle,
                sliceSourceRectangle, color);

            // Bottom.
            sliceSourceRectangle = new Rectangle(sourceRectangle.Left + leftBorder,
                sourceRectangle.Bottom - bottomBorder,
                sourceRectangle.Width - leftBorder - rightBorder,
                bottomBorder);
            sliceDestinationRectangle = new Rectangle(destinationRectangle.Left + leftBorder * scale,
                destinationRectangle.Bottom - bottomBorder * scale,
                destinationRectangle.Width - (leftBorder + rightBorder) * scale,
                bottomBorder * scale);
            _spriteBatch.Draw(sprite.Texture, sliceDestinationRectangle,
                sliceSourceRectangle, color);

            // Center.
            sliceSourceRectangle = new Rectangle(sourceRectangle.Left + leftBorder,
                sourceRectangle.Top + topBorder,
                sourceRectangle.Width - leftBorder - rightBorder,
                sourceRectangle.Height - topBorder - bottomBorder);
            sliceDestinationRectangle = new Rectangle(destinationRectangle.Left + leftBorder * scale,
                destinationRectangle.Top + topBorder * scale,
                destinationRectangle.Width - (leftBorder + rightBorder) * scale,
                destinationRectangle.Height - (topBorder + bottomBorder) * scale);
            _spriteBatch.Draw(sprite.Texture, sliceDestinationRectangle,
                sliceSourceRectangle, color);
        }

        private void ApplyDrawState(Rectangle? clipMask)
        {
            var isStateNotChanged = _isBeginCalled
                && _currentClipMask == clipMask;
            if (isStateNotChanged)
                return;

            EndDrawState();

            _currentClipMask = clipMask;

            RasterizerState rasterizerState = null;
            if (_currentClipMask.HasValue)
            {
                rasterizerState = _scissorRasterizerState;
                _spriteBatch.GraphicsDevice.ScissorRectangle = _currentClipMask.Value;
            }

            _isBeginCalled = true;
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp,
                rasterizerState: rasterizerState);
        }

        private void EndDrawState()
        {
            _currentClipMask = null;

            if (!_isBeginCalled)
                return;

            _isBeginCalled = false;
            _spriteBatch.End();
        }

        // Explicit interfaces.
        void IUiRenderer.Begin()
        {
            DrawCount = _spriteBatch.GraphicsDevice.Metrics.DrawCount;

            _renderSpriteDataList.Clear();
            _renderTextDataList.Clear();

            _spriteBatch.GraphicsDevice.SetRenderTarget(RenderTarget);
        }

        void IUiRenderer.End()
        {
            _uiBatcher.Batch();
            foreach (ref readonly var renderCommand in _uiBatcher.BatchedCommands)
            {
                switch(renderCommand.Type)
                {
                    case RenderCommandType.Sprite:
                        RunDrawSpriteCommand(renderCommand);
                        break;
                    case RenderCommandType.Text:
                        RunDrawTextCommand(renderCommand);
                        break;
                    case RenderCommandType.Geometry:
                        RunDrawGeometryCommand(renderCommand);
                        break;
                }
            }

            EndDrawState();

            DrawCount = _spriteBatch.GraphicsDevice.Metrics.DrawCount - DrawCount;
        }

        void IUiRenderer.DrawSprite(Sprite sprite, DrawMode drawMode,
            Rectangle destinationRectangle, Rectangle? clipMask, Color color)
        {
            if (sprite == Sprite.Empty)
                return;

            if (sprite.Texture is null)
                sprite = FallbackSprite;

            var data = new RenderSpriteData(sprite, drawMode, destinationRectangle, color);
            _renderSpriteDataList.Add(data);

            SkipClipMaskIfContainsOrCropDestination(ref clipMask, ref destinationRectangle);

            _uiBatcher.AddRenderCommand(new RenderCommand()
            {
                Id = _renderSpriteDataList.Count - 1,
                Type = RenderCommandType.Sprite,
                BoundingRectangle = destinationRectangle,
                ClipMask = clipMask,
                Texture = sprite.Texture,
            });
        }

        void IUiRenderer.DrawString(SpriteFont spriteFont, string text,
            Vector2 position, Rectangle? clipMask, Color color)
        {
            var data = new RenderTextData(spriteFont, text, position, color);
            _renderTextDataList.Add(data);

            var size = spriteFont.MeasureString(text);
            var destinationRectangle = new Rectangle(position.ToPoint(), size.ToPoint());

            SkipClipMaskIfContainsOrCropDestination(ref clipMask, ref destinationRectangle);

            _uiBatcher.AddRenderCommand(new RenderCommand()
            {
                Id = _renderTextDataList.Count - 1,
                Type = RenderCommandType.Text,
                BoundingRectangle = destinationRectangle,
                ClipMask = clipMask,
                Texture = spriteFont.Texture,
            });
        }

        void IUiRenderer.DrawGeometry(Geometry geometry,
            Vector2 position, Rectangle? clipMask, Color color)
        {
            // TODO: Implement draw geometry logic.
            throw new System.NotImplementedException();
        }

        private readonly record struct RenderSpriteData(Sprite Sprite,
            DrawMode DrawMode, Rectangle DestinationRectangle, Color Color);

        private readonly record struct RenderTextData(SpriteFont SpriteFont,
            string Text, Vector2 Position, Color Color);
    }
}
