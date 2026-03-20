namespace FastFileUnpacker
{
    public sealed class UnsupportedAsset(string fullName, byte[] data, string description)
        : Asset(fullName, data)
    {
        public string Description { get; } = description;
    }
}
