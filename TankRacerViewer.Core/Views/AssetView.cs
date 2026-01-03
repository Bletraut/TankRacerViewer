using System.IO;

namespace TankRacerViewer.Core
{
    public abstract class AssetView
    {
        public string FullName { get; }
        public string Name { get; }
        public string Extension { get; }

        public AssetView(string fullName)
        {
            FullName = fullName;
            Name = Path.GetFileNameWithoutExtension(FullName);
            Extension = Path.GetExtension(FullName);
        }
    }
}
