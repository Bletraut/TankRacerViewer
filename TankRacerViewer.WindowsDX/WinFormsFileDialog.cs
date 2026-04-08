using System.Linq;
using System.Threading;
using System.Windows.Forms;

using TankRacerViewer.Core;

namespace TankRacerViewer.WindowsDX
{
    public sealed class WinFormsFileDialog : IFileDialogProvider
    {
        string IFileDialogProvider.OpenFileDialog(ItemFilter[] filters, string defaultPath)
        {
            var result = string.Empty;

            var filterList = filters is not null
                ? string.Join("|", filters.Select(item => $"{item.Name} (*.{item.Extension})|*.{item.Extension}"))
                : string.Empty;

            var thread = new Thread(() =>
            {
                using var dialog = new OpenFileDialog
                {
                    InitialDirectory = defaultPath,
                    Filter = filterList,
                };

                if (dialog.ShowDialog() == DialogResult.OK)
                    result = dialog.FileName;
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            return result;
        }

        string IFileDialogProvider.OpenFolderDialog(string defaultPath)
        {
            var result = string.Empty;

            var thread = new Thread(() =>
            {
                using var dialog = new FolderBrowserDialog
                {
                    InitialDirectory = defaultPath
                };

                if (dialog.ShowDialog() == DialogResult.OK)
                    result = dialog.SelectedPath;
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            return result;
        }
    }
}
