using FastFileUnpacker;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TankRacerViewer.Core
{
    public sealed class TextureAssetView : AssetView
    {
        public Texture2D Texture { get; }
        public BlendMode BlendMode { get; }

        public TextureAssetView(GraphicsDevice graphicsDevice, TextureAsset textureAsset)
            : this(graphicsDevice, textureAsset.FullName,
                  textureAsset.Width, textureAsset.Height,
                  textureAsset.Colors, textureAsset.BlendMode)
        { }

        public TextureAssetView(GraphicsDevice graphicsDevice, string fullName,
            int width, int height, System.Drawing.Color[] colors, BlendMode blendMode)
            : base(fullName)
        {
            BlendMode = blendMode;

            var convertedColors = new Color[colors.Length];
            for (var i = 0; i < colors.Length; i++)
            {
                var color = colors[i];
                convertedColors[i] = new Color(color.R, color.G, color.B, color.A);
            }

            Texture = new Texture2D(graphicsDevice, width, height);
            Texture.SetData(convertedColors);
        }
    }
}
