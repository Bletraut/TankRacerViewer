namespace TankRacerViewer.Core
{
    public sealed class DataAssetView : AssetView
    {
        public string Text { get; }

        public DataAssetView(string fullName, string text)
            : base(fullName)
        {
            Text = text;
        }
    }
}
