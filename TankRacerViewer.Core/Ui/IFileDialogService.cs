namespace TankRacerViewer.Core.Ui
{
    public interface IFileDialogService
    {
        public string OpenFileDialog(string filterList = default,
            string defaultPath = default);
        public string OpenFolderDialog(string defaultPath = default);
    }
}
