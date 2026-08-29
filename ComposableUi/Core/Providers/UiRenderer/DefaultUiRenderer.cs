using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

using ComposableUi.Utilities;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ComposableUi
{
    public sealed class DefaultUiRenderer : IUiRenderer
    {
        public const int DefaultNineSlicedScale = 2;

        public static Texture2D FallbackTexture { get; private set; }
        public static Sprite FallbackSprite { get; private set; }

        public int NineSlicedScale = DefaultNineSlicedScale;

        public RenderTarget2D RenderTarget { get; set; }

        private readonly ContentManager _contentManager;
        private readonly SpriteBatch _spriteBatch;

        private readonly RasterizerState _scissorRasterizerState = new()
        {
            ScissorTestEnable = true,
        };
        private readonly Dictionary<StandardSkin, Sprite> _standardSkinSprites = [];
        private readonly Texture2D _standardSkinAtlasTexture;

        private readonly UiBatcher _uiBatcher = new();
        private readonly List<RenderSpriteData> _renderSpriteDataList = [];
        private readonly List<RenderTextData> _renderTextDataList = [];

        private bool _isBeginCalled;
        private Rectangle? _currentClipMask;

        private long _drawCount;
        private long _startTimestamp;

        public DefaultUiRenderer(ContentManager contentManager, SpriteBatch spriteBatch)
        {
            _contentManager = contentManager;
            _spriteBatch = spriteBatch;

            if (FallbackTexture is null)
            {
                FallbackTexture = new Texture2D(spriteBatch.GraphicsDevice, 2, 2);
                FallbackTexture.SetData([Color.Pink, Color.DeepPink, Color.DeepPink, Color.Pink]);

                FallbackSprite = new Sprite()
                {
                    Texture = FallbackTexture,
                    SourceRectangle = new Rectangle(0, 0, FallbackTexture.Width, FallbackTexture.Height)
                };
            }

            _standardSkinAtlasTexture = _contentManager.Load<Texture2D>("ComposableUi\\UiElementsAtlas");

            PrepareStandardSkinSprites();
        }

        private void PrepareStandardSkinSprites()
        {
            var assembly = Assembly.GetExecutingAssembly();

            var atlasResourceName = assembly.GetManifestResourceNames()
                .First(resource => resource.EndsWith("UiElementsAtlas.json"));

            using var stream = assembly.GetManifestResourceStream(atlasResourceName);
            using var reader = new StreamReader(stream);
            var atlasJson = reader.ReadToEnd();

            if (AsepriteUtilities.TryGetSlices(atlasJson, out var slices))
            {
                foreach (var slice in slices)
                {
                    if (Enum.TryParse<StandardSkin>(slice.Name, out var standardSkin))
                        _standardSkinSprites[standardSkin] = slice.ToSprite();
                }
            }
        }

        private void AddDrawSpriteCommand(Sprite sprite, DrawMode drawMode,
            Rectangle destinationRectangle, Rectangle? clipMask, Color color)
        {
            var data = new RenderSpriteData(sprite, drawMode, destinationRectangle, color);
            _renderSpriteDataList.Add(data);

            var isFullyWithinClipMask = !clipMask.HasValue
                || Rectangle.Union(destinationRectangle, clipMask.Value) == clipMask.Value;
            if (isFullyWithinClipMask)
                clipMask = null;

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

            // Top left.
            var sliceSourceRectangle = new Rectangle(sprite.SourceRectangle.Left,
                sprite.SourceRectangle.Top,
                sprite.LeftBorder,
                sprite.TopBorder);
            var sliceDestinationRectangle = new Rectangle(destinationRectangle.Left,
                destinationRectangle.Top,
                sliceSourceRectangle.Width * NineSlicedScale,
                sliceSourceRectangle.Height * NineSlicedScale);
            _spriteBatch.Draw(sprite.Texture, sliceDestinationRectangle,
                sliceSourceRectangle, color);

            // Top right.
            sliceSourceRectangle = new Rectangle(sprite.SourceRectangle.Right - sprite.RightBorder,
                sprite.SourceRectangle.Top,
                sprite.RightBorder,
                sprite.TopBorder);
            sliceDestinationRectangle = new Rectangle(destinationRectangle.Right - sliceSourceRectangle.Width * NineSlicedScale,
                destinationRectangle.Top,
                sliceSourceRectangle.Width * NineSlicedScale,
                sliceSourceRectangle.Height * NineSlicedScale);
            _spriteBatch.Draw(sprite.Texture, sliceDestinationRectangle,
                sliceSourceRectangle, color);

            // Bottom left.
            sliceSourceRectangle = new Rectangle(sprite.SourceRectangle.Left,
                sprite.SourceRectangle.Bottom - sprite.BottomBorder,
                sprite.LeftBorder,
                sprite.BottomBorder);
            sliceDestinationRectangle = new Rectangle(destinationRectangle.Left,
                destinationRectangle.Bottom - sliceSourceRectangle.Height * NineSlicedScale,
                sliceSourceRectangle.Width * NineSlicedScale,
                sliceSourceRectangle.Height * NineSlicedScale);
            _spriteBatch.Draw(sprite.Texture, sliceDestinationRectangle,
                sliceSourceRectangle, color);

            // Bottom right.
            sliceSourceRectangle = new Rectangle(sprite.SourceRectangle.Right - sprite.RightBorder,
                sprite.SourceRectangle.Bottom - sprite.BottomBorder,
                sprite.RightBorder,
                sprite.BottomBorder);
            sliceDestinationRectangle = new Rectangle(destinationRectangle.Right - sliceSourceRectangle.Width * NineSlicedScale,
                destinationRectangle.Bottom - sliceSourceRectangle.Height * NineSlicedScale,
                sliceSourceRectangle.Width * NineSlicedScale,
                sliceSourceRectangle.Height * NineSlicedScale);
            _spriteBatch.Draw(sprite.Texture, sliceDestinationRectangle,
                sliceSourceRectangle, color);

            // Left.
            sliceSourceRectangle = new Rectangle(sprite.SourceRectangle.Left,
                sprite.SourceRectangle.Top + sprite.TopBorder,
                sprite.LeftBorder,
                sprite.SourceRectangle.Height - sprite.TopBorder - sprite.BottomBorder);
            sliceDestinationRectangle = new Rectangle(destinationRectangle.Left,
                destinationRectangle.Top + sprite.TopBorder * NineSlicedScale,
                sliceSourceRectangle.Width * NineSlicedScale,
                destinationRectangle.Height - (sprite.TopBorder + sprite.BottomBorder) * NineSlicedScale);
            _spriteBatch.Draw(sprite.Texture, sliceDestinationRectangle,
                sliceSourceRectangle, color);

            // Right.
            sliceSourceRectangle = new Rectangle(sprite.SourceRectangle.Right - sprite.RightBorder,
                sprite.SourceRectangle.Top + sprite.TopBorder,
                sprite.RightBorder,
                sprite.SourceRectangle.Height - sprite.TopBorder - sprite.BottomBorder);
            sliceDestinationRectangle = new Rectangle(destinationRectangle.Right - sliceSourceRectangle.Width * NineSlicedScale,
                destinationRectangle.Top + sprite.TopBorder * NineSlicedScale,
                sliceSourceRectangle.Width * NineSlicedScale,
                destinationRectangle.Height - (sprite.TopBorder + sprite.BottomBorder) * NineSlicedScale);
            _spriteBatch.Draw(sprite.Texture, sliceDestinationRectangle,
                sliceSourceRectangle, color);

            // Top.
            sliceSourceRectangle = new Rectangle(sprite.SourceRectangle.Left + sprite.LeftBorder,
                sprite.SourceRectangle.Top,
                sprite.SourceRectangle.Width - sprite.LeftBorder - sprite.RightBorder,
                sprite.TopBorder);
            sliceDestinationRectangle = new Rectangle(destinationRectangle.Left + sprite.LeftBorder * NineSlicedScale,
                destinationRectangle.Top,
                destinationRectangle.Width - (sprite.LeftBorder + sprite.RightBorder) * NineSlicedScale,
                sprite.TopBorder * NineSlicedScale);
            _spriteBatch.Draw(sprite.Texture, sliceDestinationRectangle,
                sliceSourceRectangle, color);

            // Bottom.
            sliceSourceRectangle = new Rectangle(sprite.SourceRectangle.Left + sprite.LeftBorder,
                sprite.SourceRectangle.Bottom - sprite.BottomBorder,
                sprite.SourceRectangle.Width - sprite.LeftBorder - sprite.RightBorder,
                sprite.BottomBorder);
            sliceDestinationRectangle = new Rectangle(destinationRectangle.Left + sprite.LeftBorder * NineSlicedScale,
                destinationRectangle.Bottom - sprite.BottomBorder * NineSlicedScale,
                destinationRectangle.Width - (sprite.LeftBorder + sprite.RightBorder) * NineSlicedScale,
                sprite.BottomBorder * NineSlicedScale);
            _spriteBatch.Draw(sprite.Texture, sliceDestinationRectangle,
                sliceSourceRectangle, color);

            // Center.
            sliceSourceRectangle = new Rectangle(sprite.SourceRectangle.Left + sprite.LeftBorder,
                sprite.SourceRectangle.Top + sprite.TopBorder,
                sprite.SourceRectangle.Width - sprite.LeftBorder - sprite.RightBorder,
                sprite.SourceRectangle.Height - sprite.TopBorder - sprite.BottomBorder);
            sliceDestinationRectangle = new Rectangle(destinationRectangle.Left + sprite.LeftBorder * NineSlicedScale,
                destinationRectangle.Top + sprite.TopBorder * NineSlicedScale,
                destinationRectangle.Width - (sprite.LeftBorder + sprite.RightBorder) * NineSlicedScale,
                destinationRectangle.Height - (sprite.TopBorder + sprite.BottomBorder) * NineSlicedScale);
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
            _renderSpriteDataList.Clear();
            _renderTextDataList.Clear();

            _spriteBatch.GraphicsDevice.SetRenderTarget(RenderTarget);

            _drawCount = _spriteBatch.GraphicsDevice.Metrics.DrawCount;
            _startTimestamp = Stopwatch.GetTimestamp();
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

            _drawCount = _spriteBatch.GraphicsDevice.Metrics.DrawCount - _drawCount;
            var elapsedTime = Stopwatch.GetElapsedTime(_startTimestamp);

            var info = $"{_drawCount}, {elapsedTime}, {1f / 75}";
            var infoSize = TextElement.DefaultSpriteFont.MeasureString(info);

            _spriteBatch.Begin();
            _spriteBatch.Draw(FallbackTexture, new Rectangle(Point.Zero, infoSize.ToPoint()),  null, Color.Black);
            _spriteBatch.DrawString(TextElement.DefaultSpriteFont, info, Vector2.Zero, Color.White);
            _spriteBatch.End();
        }

        void IUiRenderer.DrawSprite(Sprite sprite, DrawMode drawMode,
            Rectangle destinationRectangle, Rectangle? clipMask, Color color)
        {
            AddDrawSpriteCommand(sprite, drawMode, destinationRectangle, clipMask, color);
        }

        void IUiRenderer.DrawSkinnedRectangle(StandardSkin skin, DrawMode drawMode,
            Rectangle destinationRectangle, Rectangle? clipMask, Color color)
        {
            if (skin is StandardSkin.None)
                return;

            if (_standardSkinSprites.TryGetValue(skin, out var sprite))
            {
                sprite.Texture = _standardSkinAtlasTexture;
            }
            else
            {
                sprite = FallbackSprite;
            }

            AddDrawSpriteCommand(sprite, drawMode, destinationRectangle, clipMask, color);
        }

        void IUiRenderer.DrawString(SpriteFont spriteFont, string text,
            Vector2 position, Rectangle? clipMask, Color color)
        {
            var data = new RenderTextData(spriteFont, text, position, color);
            _renderTextDataList.Add(data);

            var size = spriteFont.MeasureString(text);
            var destinationRectangle = new Rectangle(position.ToPoint(), size.ToPoint());

            var isFullyWithinClipMask = !clipMask.HasValue
                || Rectangle.Union(destinationRectangle, clipMask.Value) == clipMask.Value;
            if (isFullyWithinClipMask)
                clipMask = null;

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
