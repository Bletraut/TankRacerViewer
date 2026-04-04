using Microsoft.Xna.Framework.Graphics;

namespace TankRacerViewer.Core
{
    public readonly record struct UsedTextureData(int Index,
        Texture2D Texture, string TextureName);
}
