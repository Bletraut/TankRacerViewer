using Microsoft.Xna.Framework;

using System.Collections.Generic;
using System.Text.Json;

namespace ComposableUi.Utilities
{
    public static class AsepriteUtilities
    {
        public static List<SliceData> GetSlices(string spriteSheetJson)
        {
            var slices = new List<SliceData>();

            return slices;
        }
    }

    public readonly struct SliceData(string name,
        Rectangle sourceRectangle,
        int leftBorder, int rightBorder,
        int topBorder, int bottomBorder)
    {
        public readonly string Name = name;

        public readonly Rectangle SourceRectangle = sourceRectangle;

        public readonly int LeftBorder = leftBorder;
        public readonly int RightBorder = rightBorder;
        public readonly int TopBorder = topBorder;
        public readonly int BottomBorder = bottomBorder;
    }
}

//public static class AsepriteSliceParser
//{
//    public static bool TryGetSlice(
//        string json,
//        string sliceName,
//        out SliceData sliceData)
//    {
//        sliceData = default;

//        using JsonDocument doc = JsonDocument.Parse(json);

//        if (!doc.RootElement.TryGetProperty("meta", out var meta))
//            return false;

//        if (!meta.TryGetProperty("slices", out var slices))
//            return false;

//        foreach (var slice in slices.EnumerateArray())
//        {
//            if (slice.GetProperty("name").GetString() != sliceName)
//                continue;

//            var keys = slice.GetProperty("keys");
//            var key = keys[0]; // берём только frame 0

//            var bounds = ReadRect(key.GetProperty("bounds"));

//            Rect? center = null;
//            if (key.TryGetProperty("center", out var centerElement))
//            {
//                center = ReadRect(centerElement);
//            }

//            sliceData = new SliceData
//            {
//                Bounds = bounds,
//                Center = center
//            };

//            return true;
//        }

//        return false;
//    }

//    private static Rect ReadRect(JsonElement element)
//    {
//        return new Rect
//        {
//            X = element.GetProperty("x").GetInt32(),
//            Y = element.GetProperty("y").GetInt32(),
//            W = element.GetProperty("w").GetInt32(),
//            H = element.GetProperty("h").GetInt32()
//        };
//    }
//}

