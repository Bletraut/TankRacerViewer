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

        private const string AssetNotSupportedMessage = "This asset is not supported.";

        // Static.
        public static bool FromStream(Stream stream, out FastFile? fastFile)
        {
            fastFile = default;

            if (stream.Length < HeaderSize)
                return false;

            using var binaryReader = new BinaryReader(stream);
            var header = binaryReader.ReadBytes(HeaderSize);
            var version = Encoding.Latin1.GetString(header.AsSpan(0, VersionSize));
            var assetCount = BitConverter.ToInt32(header.AsSpan(HeaderSize - AssetCountSize, AssetCountSize));

            if (!string.Equals(version, SupportedVersion, StringComparison.Ordinal))
                return false;

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
                ".ts" => new UnsupportedAsset(fullName, data, $"Presumably a file for a level editor.\n{AssetNotSupportedMessage}"),
                ".gui" => new UnsupportedAsset(fullName, data, $"Presumably a file for a user interface.\n{AssetNotSupportedMessage}"),
                _ => new UnsupportedAsset(fullName, data, AssetNotSupportedMessage)
            };
        }

        // Class.
        public string Version { get; }

        private readonly Asset[] _assets;
        public ReadOnlySpan<Asset> Assets => _assets;

        private FastFile(string version, Asset[] assets)
        {
            Version = version;
            _assets = assets;
        }

        public T? FindFirstAssetOfType<T>() where T : Asset
        {
            foreach (var asset in _assets)
            {
                if (asset is T typedAsset)
                    return typedAsset;
            }

            return default;
        }
    }
}
