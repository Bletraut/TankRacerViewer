using System;
using System.Collections.Generic;
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

        private readonly ContentManager _contentManager;
        private readonly SpriteBatch _spriteBatch;

        private readonly RasterizerState _scissorRasterizerState = new()
        {
            ScissorTestEnable = true,
        };
        private readonly Dictionary<StandardSkin, Sprite> _standardSkinSprites = [];

        private readonly Texture2D _standardSkinAtlasTexture;

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

            try
            {
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
            catch
            {
            }
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

        // Implicit interfaces.
        // IUiRenderer.
        public void Begin()
        {
            _spriteBatch.GraphicsDevice.SetRenderTarget(null);
        }

        public void End()
        {

        }

        public void DrawSprite(Sprite sprite, DrawMode drawMode,
            Rectangle destinationRectangle, Rectangle? clipMask, Color color)
        {
            RasterizerState rasterizerState = null;
            if (clipMask.HasValue)
            {
                rasterizerState = _scissorRasterizerState;
                _spriteBatch.GraphicsDevice.ScissorRectangle = clipMask.Value;
            }

            _spriteBatch.Begin(samplerState: SamplerState.PointClamp,
                rasterizerState: rasterizerState);

            switch (drawMode)
            {
                case DrawMode.Simple:
                    DrawSimpleSprite(sprite, destinationRectangle, color);
                    break;
                case DrawMode.Sliced:
                    DrawSlicedSprite(sprite, destinationRectangle, color);
                    break;
                default:
                    DrawSimpleSprite(sprite, destinationRectangle, color);
                    break;
            }

            _spriteBatch.End();
        }

        public void DrawSkinnedRectangle(StandardSkin skin, DrawMode drawMode,
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

            DrawSprite(sprite, drawMode, destinationRectangle, clipMask, color);
        }

        public void DrawString(SpriteFont spriteFont, string text,
            Vector2 position, Rectangle? clipMask, Color color)
        {
            RasterizerState rasterizerState = null;
            if (clipMask.HasValue)
            {
                rasterizerState = _scissorRasterizerState;
                _spriteBatch.GraphicsDevice.ScissorRectangle = clipMask.Value;
            }

            _spriteBatch.Begin(samplerState: SamplerState.PointClamp,
                rasterizerState: rasterizerState);

            _spriteBatch.DrawString(spriteFont, text, position, color);

            _spriteBatch.End();
        }
    }
}
