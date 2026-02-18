using Microsoft.Xna.Framework;

using System.Collections.Generic;
using System.Text.Json;

namespace ComposableUi.Utilities
{
    public static class AsepriteUtilities
    {
        private const string MetaPropertyName = "meta";
        private const string SlicesPropertyName = "slices";

        private const string SliceNamePropertyName = "name";
        private const string SliceKeysPropertyName = "keys";
        private const string SliceBoundsPropertyName = "bounds";
        private const string SliceCenterPropertyName = "center";

        public static bool TryGetSlices(string spriteSheetJson,
            out List<SliceData> slices)
        {
            slices = null;

            using var jsonDocument = JsonDocument.Parse(spriteSheetJson);

            if (!jsonDocument.RootElement.TryGetProperty(MetaPropertyName, out var metaElement))
                return false;

            if (!metaElement.TryGetProperty(SlicesPropertyName, out var slicesElement))
                return false;

            slices = [];
            foreach (var sliceElement in slicesElement.EnumerateArray())
            {
                if (!sliceElement.TryGetProperty(SliceNamePropertyName, out var nameElement))
                    continue;

                if (!sliceElement.TryGetProperty(SliceKeysPropertyName, out var keysElement))
                    continue;

                var keyElement = keysElement[0];
                if (!keyElement.TryGetProperty(SliceBoundsPropertyName, out var boundsElement))
                    continue;

                var sourceRectangle = ReadRectangle(boundsElement);
                var centerRectangle = Rectangle.Empty;

                if (keyElement.TryGetProperty(SliceCenterPropertyName, out var centerElement))
                    centerRectangle = ReadRectangle(centerElement);

                slices.Add(new SliceData(nameElement.GetString(), sourceRectangle, centerRectangle));
            }

            return true;
        }

        public static Sprite ToSprite(this SliceData slice)
        {
            return new Sprite()
            {
                SourceRectangle = slice.SourceRectangle,
                LeftBorder = slice.CenterRectangle.Left,
                RightBorder = slice.SourceRectangle.Width - slice.CenterRectangle.Right,
                TopBorder = slice.CenterRectangle.Top,
                BottomBorder = slice.SourceRectangle.Height - slice.CenterRectangle.Bottom
            };
        }

        private static Rectangle ReadRectangle(JsonElement element)
        {
            return new Rectangle()
            {
                X = element.GetProperty("x").GetInt32(),
                Y = element.GetProperty("y").GetInt32(),
                Width = element.GetProperty("w").GetInt32(),
                Height = element.GetProperty("h").GetInt32(),
            };
        }
    }

    public readonly struct SliceData(string name,
        Rectangle sourceRectangle, Rectangle centerRectangle)
    {
        public readonly string Name = name;

        public readonly Rectangle SourceRectangle = sourceRectangle;
        public readonly Rectangle CenterRectangle = centerRectangle;
    }
}

