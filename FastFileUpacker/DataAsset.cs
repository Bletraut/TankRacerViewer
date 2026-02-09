using System.Text;

namespace FastFileUnpacker
{
    public sealed class DataAsset : Asset
    {
        public string Text { get; }

        public DataAsset(string fullName, byte[] data) : base(fullName, data)
        {
            Text = Encoding.Latin1.GetString(data);
        }
    }
}
