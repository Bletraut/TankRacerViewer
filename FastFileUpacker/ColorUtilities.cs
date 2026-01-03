using System.Drawing;

namespace FastFileUnpacker
{
    public static class ColorUtilities
    {
        public static Color Bgra5551ToColor(ushort bgra5551)
        {
            // 0x_BBBBB_GGGGG_RRRRR_A
            var blue5bit = (bgra5551 & 0b_11111_00000_00000_0) >> 11;
            var green5bit = (bgra5551 & 0b_00000_11111_00000_0) >> 6;
            var red5bit = (bgra5551 & 0b_00000_00000_11111_0) >> 1;

            var red = red5bit << 3;
            var green = green5bit << 3;
            var blue = blue5bit << 3;

            return Color.FromArgb(byte.MaxValue, red, green, blue);
        }
    }
}
