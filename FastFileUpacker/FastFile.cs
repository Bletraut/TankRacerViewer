using System.Diagnostics;
using System.Text;

namespace FastFileUnpacker
{
    public sealed class FastFile
    {
        private const int HeaderSize = 28;
        private const int VersionSize = 8;
        private const int AssetCountSize = 4;

        private const int AssetFullNameSize = 12;
        private const int AssetPaddingSize = 4;

        // Static.
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
        private readonly Asset[] _assets;
        public IReadOnlyList<Asset> Assets { get; }

        public FastFile(Stream stream)
        {
            using var binaryReader = new BinaryReader(stream);

            var header = binaryReader.ReadBytes(HeaderSize);
            var version = Encoding.Latin1.GetString(header.AsSpan(0, VersionSize));
            var assetCount = BitConverter.ToInt32(header.AsSpan(HeaderSize - AssetCountSize, AssetCountSize));

            Debug.WriteLine($"Version: {version}, Assets: {assetCount}");

            _assets = new Asset[assetCount];
            Assets = _assets.AsReadOnly();

            for (var i = 0; i < assetCount; i++)
            {
                var assetFullName = Encoding.Latin1.GetString(binaryReader.ReadBytes(AssetFullNameSize)).TrimEnd('\0');
                var assetOffset = binaryReader.ReadUInt32();
                var assetSize = binaryReader.ReadUInt32();
                stream.Seek(AssetPaddingSize, SeekOrigin.Current);

                var position = stream.Position;
                stream.Seek(assetOffset, SeekOrigin.Begin);
                _assets[i] = CreateAsset(assetFullName, binaryReader.ReadBytes((int)assetSize));
                stream.Position = position;
            }
        }
    }
}
