using System.Diagnostics;
using System.Text;

namespace FastFileUnpacker
{
    public sealed class FastFile
    {
        private const string SupportedVersion = "FASTF101";

        private const int HeaderSize = 28;
        private const int VersionSize = 8;
        private const int AssetCountSize = 4;

        private const int AssetFullNameSize = 12;
        private const int AssetPaddingSize = 4;

        // Static.
        public static bool FromStream(Stream stream, out FastFile? fastFile)
        {
            using var binaryReader = new BinaryReader(stream);

            var header = binaryReader.ReadBytes(HeaderSize);
            var version = Encoding.Latin1.GetString(header.AsSpan(0, VersionSize));
            var assetCount = BitConverter.ToInt32(header.AsSpan(HeaderSize - AssetCountSize, AssetCountSize));

            if (!string.Equals(version, SupportedVersion, StringComparison.Ordinal))
            {
                fastFile = default;
                return false;
            }

            var assets = new Asset[assetCount];
            fastFile = new FastFile(version, assets);

            for (var i = 0; i < assetCount; i++)
            {
                var assetFullName = Encoding.Latin1.GetString(binaryReader.ReadBytes(AssetFullNameSize)).TrimEnd('\0');
                var assetOffset = binaryReader.ReadUInt32();
                var assetSize = binaryReader.ReadUInt32();
                stream.Seek(AssetPaddingSize, SeekOrigin.Current);

                var position = stream.Position;
                stream.Seek(assetOffset, SeekOrigin.Begin);
                assets[i] = CreateAsset(assetFullName, binaryReader.ReadBytes((int)assetSize));
                stream.Position = position;
            }

            return true;
        }

        private static Asset CreateAsset(string fullName, byte[] data)
        {
            var extension = Path.GetExtension(fullName).ToLowerInvariant();

            return extension switch
            {
                ".sur" => new TextureAsset(fullName, data),
                ".ss" => new ModelContainerAsset(fullName, data),
                ".sm" => new ModelAsset(fullName, ModelContainerAsset.ExtractModelData(data, 0)),
                ".bsm" => new MapAsset(fullName, data),
                ".bgn" => new BackgroundAsset(fullName, data),
                ".dat" or ".txt" => new DataAsset(fullName, data),
                _ => new UnsupportedAsset(fullName, data)
            };
        }

        // Class.
        public string Version { get; }

        private readonly Asset[] _assets;
        public IReadOnlyList<Asset> Assets { get; }

        private FastFile(string version, Asset[] assets)
        {
            Version = version;

            _assets = assets;
            Assets = assets.AsReadOnly();
        }
    }
}
