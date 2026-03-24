using System;

using Microsoft.Xna.Framework.Graphics;

namespace TankRacerViewer.Core
{
    public readonly struct UsedTextureData : IEquatable<UsedTextureData>
    {
        // Static.
        public static bool operator ==(UsedTextureData left, UsedTextureData right)
            => left.Equals(right);

        public static bool operator !=(UsedTextureData left, UsedTextureData right)
            => !(left == right);

        // Struct.
        public readonly int Index;
        public readonly Texture2D Texture;
        public readonly string TextureName;

        public UsedTextureData(int index, Texture2D texture, string textureName)
        {
            Index = index;
            Texture = texture;
            TextureName = textureName;
        }

        public bool Equals(UsedTextureData other)
        {
            return Index == other.Index 
                && TextureName == other.TextureName
                && Texture == other.Texture;
        }

        public override bool Equals(object obj)
            => obj is UsedTextureData other && Equals(other);

        public override int GetHashCode()
            => (Index, TextureName, Texture).GetHashCode();
    }
}
