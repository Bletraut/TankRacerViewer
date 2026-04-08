using System.Linq;

using NativeFileDialogSharp;

using TankRacerViewer.Core;

namespace TankRacerViewer.DesktopGL
{
    public class NativeFileDialog : IFileDialogProvider
    {
        string IFileDialogProvider.OpenFileDialog(ItemFilter[] filters, string defaultPath)
        {
            var filterList = filters is not null
                ? string.Join(";", filters.Where(item => item.Extension != "*").Select(item => item.Extension))
                : string.Empty;

            var result = Dialog.FileOpen(filterList, defaultPath);

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
