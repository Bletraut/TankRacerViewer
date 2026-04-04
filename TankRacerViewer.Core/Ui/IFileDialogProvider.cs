namespace TankRacerViewer.Core.Ui
{
    public interface IFileDialogProvider
    {
        public string OpenFileDialog(string filterList = default,
            string defaultPath = default);
        public string OpenFolderDialog(string defaultPath = default);
    }
}
