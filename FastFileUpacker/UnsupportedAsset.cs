namespace FastFileUnpacker
{
    public sealed class UnsupportedAsset(string fullName, byte[] data) : Asset(fullName, data)
    {
    }
}
