using System.Buffers;
using System.Drawing;

namespace FastFileUnpacker
{
    public sealed class TextureAsset : Asset
    {
        private const int PaletteFlagOffset = 48;
        private const int RleEncodedFlagOffset = 282;

        private const byte RleControlByte = 0xC0;
        private const byte RleCountMask = 0x3F;
        private const int PaletteSize = 256;

        private const int WidthOffset = 26;
        private const int WidthSize = 2;
        private const int HeightOffset = 28;
        private const int HeightSize = 2;
        private const int BlendModeOffset = 36;
        private const int DataOffset = 284;

        // Static.
        private static readonly Color[] _palette = new Color[PaletteSize];

        private static void Decode(byte[] data, int startIndex, bool useRle,
            Action<int, byte> outputCallback)
        {
            var index = 0;
            for (var i = startIndex; i < data.Length; i++)
            {
                var dataByte = data[i];
                if (useRle && dataByte >= RleControlByte)
                {
                    var count = data[i] & RleCountMask;
                    dataByte = data[++i];

                    for (var j = 0; j < count; j++)
                    {
                        outputCallback(index, dataByte);
                        index++;
                    }
                }
                else
                {
                    outputCallback(index, dataByte);
                    index++;
                }
            }
        }

        // Class.
        public int Width { get; }
        public int Height { get; }
        public Color[] Colors { get; }
        public BlendMode BlendMode { get; }

        public TextureAsset(string fullName, byte[] data) : base(fullName, data)
        {
            Width = BitConverter.ToInt16(data.AsSpan(WidthOffset, WidthSize));
            Height = BitConverter.ToInt16(data.AsSpan(HeightOffset, HeightSize));

            Colors = new Color[Width * Height];

            BlendMode = data[BlendModeOffset] switch
            {
                0 => BlendMode.Opaque,
                1 => BlendMode.AlphaTest,
                2 => BlendMode.Transparent,
                _ => throw new NotImplementedException(),
            };

            var hasPalette = data[PaletteFlagOffset] > 0;
            var isRleEncoded = data[RleEncodedFlagOffset] > 0;

            if (hasPalette)
            {
                for (var i = 0; i < PaletteSize; i++)
                {
                    var red = data[DataOffset + i];
                    var green = data[DataOffset + PaletteSize + i];
                    var blue = data[DataOffset + PaletteSize * 2 + i];
                    _palette[i] = Color.FromArgb(byte.MaxValue, red, green, blue);
                }

                if (BlendMode != BlendMode.Opaque)
                    _palette[0] = Color.FromArgb(0, _palette[0]);

                Decode(data, DataOffset + PaletteSize * 3, isRleEncoded, (index, value) =>
                {
                    Colors[index] = _palette[value];
                });
            }
            else
            {
                var length = Colors.Length * 4;
                var colorBytes = ArrayPool<byte>.Shared.Rent(length);

                Decode(data, DataOffset, isRleEncoded, (index, value) =>
                {
                    colorBytes[index] = value;
                });

                for (var i = 0; i < Colors.Length; i++)
                {
                    var blue = colorBytes[i];
                    var green = colorBytes[i + Colors.Length];
                    var red = colorBytes[i + Colors.Length * 2];
                    var alpha = colorBytes[i + Colors.Length * 3];
                    Colors[i] = Color.FromArgb(alpha, red, green, blue);
                }

                ArrayPool<byte>.Shared.Return(colorBytes);
            }
        }
    }
}
