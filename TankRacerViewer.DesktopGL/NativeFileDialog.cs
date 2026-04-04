using TankRacerViewer.Core.Ui;
using NativeFileDialogSharp;

namespace TankRacerViewer.DesktopGL
{
    public class NativeFileDialog : IFileDialogProvider
    {
        string IFileDialogProvider.OpenFileDialog(string filerList, string defaultPath)
        {
            var result = Dialog.FileOpen(filerList, defaultPath);

            if (result.IsOk)
                return result.Path;

            return string.Empty;
        }

        string IFileDialogProvider.OpenFolderDialog(string defaultPath)
        {
            var result = Dialog.FolderPicker(defaultPath);

            if (result.IsOk)
                return result.Path;

            return string.Empty;
        }
    }
}
