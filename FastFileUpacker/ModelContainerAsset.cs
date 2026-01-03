using System.Text;

namespace FastFileUnpacker
{
    public sealed class ModelContainerAsset : Asset
    {
        private const int HeaderSize = 8;
        private const int ModelCountOffset = 4;
        private const int ModelCountSize = 2;
        private const int ModelHeaderSize = 12;
        private const int ModelNameSize = 8;
        private const int ModelDataOffsetSize = 4;
        private const int ModelDataSize = 4;

        // Static.
        public static byte[] ExtractModelData(byte[] data, int offset)
        {
            var modelDataSize = BitConverter.ToInt32(data.AsSpan(offset, ModelDataSize));
            var modelData = data.AsSpan(offset + ModelDataSize, modelDataSize).ToArray();

            return modelData;
        }

        // Class.
        private readonly ModelAsset[] _models;
        public IReadOnlyList<ModelAsset> Models { get; }

        public ModelContainerAsset(string fullName, byte[] data) : base(fullName, data)
        {
            var modelCount = BitConverter.ToInt16(data.AsSpan(ModelCountOffset, ModelCountSize));

            _models = new ModelAsset[modelCount];
            Models = _models.AsReadOnly();

            for (var i = 0; i < modelCount; i++)
            {
                var offset = HeaderSize + ModelHeaderSize * i;
                var modelName = Encoding.ASCII.GetString(data.AsSpan(offset, ModelNameSize)).Split('\0')[0];

                offset += ModelNameSize;
                var modelDataOffset = BitConverter.ToInt32(data.AsSpan(offset, ModelDataOffsetSize));
                var modelData = ExtractModelData(data, modelDataOffset).ToArray();

                var modelAsset = new ModelAsset(modelName, modelData);
                _models[i] = modelAsset;
            }
        }
    }
}
