using System.Threading;
using System.Windows.Forms;

using TankRacerViewer.Core.Ui;

namespace TankRacerViewer.WindowsDX
{
    public sealed class WinFormsFileDialog : IFileDialogProvider
    {
        string IFileDialogProvider.OpenFileDialog(string filterList, string defaultPath)
        {
            var result = string.Empty;

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
