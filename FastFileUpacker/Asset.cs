namespace FastFileUnpacker
{
    public abstract class Asset(string fullName, byte[] data)
    {
        public string FullName { get; } = fullName;

        protected byte[] Data = data;
    }
}
