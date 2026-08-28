using FastFileUnpacker;

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
            int width, int height, Rgba8888[] colors, BlendMode blendMode)
            : base(fullName)
        {
            BlendMode = blendMode;

            Texture = new Texture2D(graphicsDevice, width, height);
            Texture.SetData(colors);
        }
    }
}
