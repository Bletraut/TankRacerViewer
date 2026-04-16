using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using ComposableUi;
using ComposableUi.Utilities;

using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TankRacerViewer.Core
{
    public static class IconCollection
    {
        private static readonly Dictionary<string, Sprite> _cache = [];

        public static void Initialize(ContentManager contentManager)
        {
            var iconAtlas = contentManager.Load<Texture2D>("Images\\Icons");
            PrepareIconSprites(iconAtlas);
        }

        public static Sprite Get(string name)
            => _cache.GetValueOrDefault(name);

        private static void PrepareIconSprites(Texture2D atlas)
        {
            var assembly = Assembly.GetExecutingAssembly();

            try
            {
                var atlasResourceName = assembly.GetManifestResourceNames()
                    .First(resource => resource.EndsWith("Icons.json"));

                using var stream = assembly.GetManifestResourceStream(atlasResourceName);
                using var reader = new StreamReader(stream);
                var atlasJson = reader.ReadToEnd();

                if (AsepriteUtilities.TryGetSlices(atlasJson, out var slices))
                {
                    foreach (var slice in slices)
                    {
                        var sprite = slice.ToSprite();
                        sprite.Texture = atlas;

                        _cache.Add(slice.Name, sprite);
                    }
                }
            }
            catch
            {
            }
        }
    }

    public static class IconName
    {
        public const string Explorer = "Explorer";
        public const string Viewer = "Viewer";
        public const string Inspector = "Inspector";
        public const string Console = "Console";
        public const string About = "About";

        public const string Folder = "Folder";
        public const string AssetGroup = "AssetGroup";
        public const string FastFile = "FastFile";
        public const string Texture = "Texture";
        public const string Model = "Model";
        public const string Background = "Background";
        public const string Level = "Level";
        public const string Tank = "Tank";
        public const string Data = "Data";
        public const string Unsupported = "Unsupported";
    }
}
