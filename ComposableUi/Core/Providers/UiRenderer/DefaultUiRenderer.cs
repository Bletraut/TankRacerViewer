using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ComposableUi
{
    public sealed class DefaultUiRenderer : IUiRenderer
    {
        // Static.
        private static void SkipClipMaskIfContains(ref Rectangle? clipMask, Rectangle destinationRectangle)
        {
            if (!clipMask.HasValue)
                return;

            if (clipMask.Value.Contains(destinationRectangle))
                clipMask = null;
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
        private readonly UiBatcher _uiBatcher = new();
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

        private void AddDrawSpriteCommand(Sprite sprite, DrawMode drawMode,
            Rectangle destinationRectangle, Rectangle? clipMask, Color color)
        {
            var data = new RenderSpriteData(sprite, drawMode, destinationRectangle, color);
            _renderSpriteDataList.Add(data);

            SkipClipMaskIfContains(ref clipMask, destinationRectangle);

            _uiBatcher.AddRenderCommand(_renderSpriteDataList.Count - 1, (int)RenderCommandType.Sprite,
                destinationRectangle, clipMask, sprite.Texture);
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

            // Top left.
            var sliceSourceRectangle = new Rectangle(sprite.SourceRectangle.Left,
                sprite.SourceRectangle.Top,
                sprite.LeftBorder,
                sprite.TopBorder);
            var sliceDestinationRectangle = new Rectangle(destinationRectangle.Left,
                destinationRectangle.Top,
                sliceSourceRectangle.Width * scale,
                sliceSourceRectangle.Height * scale);
            _spriteBatch.Draw(sprite.Texture, sliceDestinationRectangle,
                sliceSourceRectangle, color);

            // Top right.
            sliceSourceRectangle = new Rectangle(sprite.SourceRectangle.Right - sprite.RightBorder,
                sprite.SourceRectangle.Top,
                sprite.RightBorder,
                sprite.TopBorder);
            sliceDestinationRectangle = new Rectangle(destinationRectangle.Right - sliceSourceRectangle.Width * scale,
                destinationRectangle.Top,
                sliceSourceRectangle.Width * scale,
                sliceSourceRectangle.Height * scale);
            _spriteBatch.Draw(sprite.Texture, sliceDestinationRectangle,
                sliceSourceRectangle, color);

            // Bottom left.
            sliceSourceRectangle = new Rectangle(sprite.SourceRectangle.Left,
                sprite.SourceRectangle.Bottom - sprite.BottomBorder,
                sprite.LeftBorder,
                sprite.BottomBorder);
            sliceDestinationRectangle = new Rectangle(destinationRectangle.Left,
                destinationRectangle.Bottom - sliceSourceRectangle.Height * scale,
                sliceSourceRectangle.Width * scale,
                sliceSourceRectangle.Height * scale);
            _spriteBatch.Draw(sprite.Texture, sliceDestinationRectangle,
                sliceSourceRectangle, color);

            // Bottom right.
            sliceSourceRectangle = new Rectangle(sprite.SourceRectangle.Right - sprite.RightBorder,
                sprite.SourceRectangle.Bottom - sprite.BottomBorder,
                sprite.RightBorder,
                sprite.BottomBorder);
            sliceDestinationRectangle = new Rectangle(destinationRectangle.Right - sliceSourceRectangle.Width * scale,
                destinationRectangle.Bottom - sliceSourceRectangle.Height * scale,
                sliceSourceRectangle.Width * scale,
                sliceSourceRectangle.Height * scale);
            _spriteBatch.Draw(sprite.Texture, sliceDestinationRectangle,
                sliceSourceRectangle, color);

            // Left.
            sliceSourceRectangle = new Rectangle(sprite.SourceRectangle.Left,
                sprite.SourceRectangle.Top + sprite.TopBorder,
                sprite.LeftBorder,
                sprite.SourceRectangle.Height - sprite.TopBorder - sprite.BottomBorder);
            sliceDestinationRectangle = new Rectangle(destinationRectangle.Left,
                destinationRectangle.Top + sprite.TopBorder * scale,
                sliceSourceRectangle.Width * scale,
                destinationRectangle.Height - (sprite.TopBorder + sprite.BottomBorder) * scale);
            _spriteBatch.Draw(sprite.Texture, sliceDestinationRectangle,
                sliceSourceRectangle, color);

            // Right.
            sliceSourceRectangle = new Rectangle(sprite.SourceRectangle.Right - sprite.RightBorder,
                sprite.SourceRectangle.Top + sprite.TopBorder,
                sprite.RightBorder,
                sprite.SourceRectangle.Height - sprite.TopBorder - sprite.BottomBorder);
            sliceDestinationRectangle = new Rectangle(destinationRectangle.Right - sliceSourceRectangle.Width * scale,
                destinationRectangle.Top + sprite.TopBorder * scale,
                sliceSourceRectangle.Width * scale,
                destinationRectangle.Height - (sprite.TopBorder + sprite.BottomBorder) * scale);
            _spriteBatch.Draw(sprite.Texture, sliceDestinationRectangle,
                sliceSourceRectangle, color);

            // Top.
            sliceSourceRectangle = new Rectangle(sprite.SourceRectangle.Left + sprite.LeftBorder,
                sprite.SourceRectangle.Top,
                sprite.SourceRectangle.Width - sprite.LeftBorder - sprite.RightBorder,
                sprite.TopBorder);
            sliceDestinationRectangle = new Rectangle(destinationRectangle.Left + sprite.LeftBorder * scale,
                destinationRectangle.Top,
                destinationRectangle.Width - (sprite.LeftBorder + sprite.RightBorder) * scale,
                sprite.TopBorder * scale);
            _spriteBatch.Draw(sprite.Texture, sliceDestinationRectangle,
                sliceSourceRectangle, color);

            // Bottom.
            sliceSourceRectangle = new Rectangle(sprite.SourceRectangle.Left + sprite.LeftBorder,
                sprite.SourceRectangle.Bottom - sprite.BottomBorder,
                sprite.SourceRectangle.Width - sprite.LeftBorder - sprite.RightBorder,
                sprite.BottomBorder);
            sliceDestinationRectangle = new Rectangle(destinationRectangle.Left + sprite.LeftBorder * scale,
                destinationRectangle.Bottom - sprite.BottomBorder * scale,
                destinationRectangle.Width - (sprite.LeftBorder + sprite.RightBorder) * scale,
                sprite.BottomBorder * scale);
            _spriteBatch.Draw(sprite.Texture, sliceDestinationRectangle,
                sliceSourceRectangle, color);

            // Center.
            sliceSourceRectangle = new Rectangle(sprite.SourceRectangle.Left + sprite.LeftBorder,
                sprite.SourceRectangle.Top + sprite.TopBorder,
                sprite.SourceRectangle.Width - sprite.LeftBorder - sprite.RightBorder,
                sprite.SourceRectangle.Height - sprite.TopBorder - sprite.BottomBorder);
            sliceDestinationRectangle = new Rectangle(destinationRectangle.Left + sprite.LeftBorder * scale,
                destinationRectangle.Top + sprite.TopBorder * scale,
                destinationRectangle.Width - (sprite.LeftBorder + sprite.RightBorder) * scale,
                destinationRectangle.Height - (sprite.TopBorder + sprite.BottomBorder) * scale);
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
            for (var i = 0; i < _uiBatcher.BatchedCommands.Count; i++)
            {
                var renderCommand = _uiBatcher.BatchedCommands[i];

                var renderCommandType = (RenderCommandType)renderCommand.Type;
                switch(renderCommandType)
                {
                    case RenderCommandType.Sprite:
                        RunDrawSpriteCommand(renderCommand);
                        break;
                    case RenderCommandType.Text:
                        RunDrawTextCommand(renderCommand);
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

            AddDrawSpriteCommand(sprite, drawMode, destinationRectangle, clipMask, color);
        }

        void IUiRenderer.DrawString(SpriteFont spriteFont, string text,
            Vector2 position, Rectangle? clipMask, Color color)
        {
            var data = new RenderTextData(spriteFont, text, position, color);
            _renderTextDataList.Add(data);

            var size = spriteFont.MeasureString(text);
            var destinationRectangle = new Rectangle(position.ToPoint(), size.ToPoint());

            SkipClipMaskIfContains(ref clipMask, destinationRectangle);

            _uiBatcher.AddRenderCommand(_renderTextDataList.Count - 1, (int)RenderCommandType.Text,
                destinationRectangle, clipMask, spriteFont.Texture);
        }

        private enum RenderCommandType
        {
            Sprite,
            Text
        }

        private readonly record struct RenderSpriteData(Sprite Sprite,
            DrawMode DrawMode, Rectangle DestinationRectangle, Color Color);

        private readonly record struct RenderTextData(SpriteFont SpriteFont,
            string Text, Vector2 Position, Color Color);
    }
}
