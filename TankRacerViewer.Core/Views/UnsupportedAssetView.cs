namespace TankRacerViewer.Core
{
    public sealed class UnsupportedAssetView : AssetView
    {
        public string Description { get; }

        public UnsupportedAssetView(string fullName, string description)
            : base(fullName)
        {
            Description = description;
        }
    }
}
