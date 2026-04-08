namespace TankRacerViewer.Core
{
    public interface IFileDialogProvider
    {
        public string OpenFileDialog(ItemFilter[] filters = default,
            string defaultPath = default);
        public string OpenFolderDialog(string defaultPath = default);
    }

    public readonly struct ItemFilter(string name, string Extension)
    {
        public readonly string Name = name;
        public readonly string Extension = Extension;
    }
}
