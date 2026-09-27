using System.Collections.Generic;

using ComposableUi.Utilities;

using Microsoft.Xna.Framework.Graphics;

namespace ComposableUi
{
    public sealed class SpriteResolver
    {
        private readonly Dictionary<string, Sprite> _sprites = [];

        public void AddAsepriteSpriteSheet(Texture2D texture, string spriteSheetJson)
        {
            if (AsepriteUtilities.TryGetSlices(spriteSheetJson, out var slices))
            {
                foreach (var slice in slices)
                {
                    var sprite = slice.ToSprite();
                    sprite.Texture = texture;
                    _sprites[slice.Name] = sprite;
                }
            }
        }

        public bool TryGetSprite(string spriteName, out Sprite sprite)
            => _sprites.TryGetValue(spriteName, out sprite);
    }
}
