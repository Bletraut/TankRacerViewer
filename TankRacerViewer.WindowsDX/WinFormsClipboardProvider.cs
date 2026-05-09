using System;
using System.Threading;
using System.Windows.Forms;

using ComposableUi;

namespace TankRacerViewer.WindowsDX
{
    public sealed class WinFormsClipboardProvider : IClipboardProvider
    {
        string IClipboardProvider.GetText()
        {
            var result = string.Empty;
            RunOnSta(() =>
            {
                try { result = Clipboard.GetText(); }
                catch (Exception) { }
            });
            return result;
        }

        void IClipboardProvider.SetText(string text)
        {
            if (string.IsNullOrEmpty(text)) return;
            RunOnSta(() =>
            {
                try
                {
                    // copy: true calls OleFlushClipboard so the data persists after this thread exits.
                    // Clipboard.SetText uses delayed rendering and loses data when the owner thread dies.
                    Clipboard.SetDataObject(text, copy: true);
                }
                catch (Exception) { }
            });
        }

        private static void RunOnSta(Action action)
        {
            var thread = new Thread(() => action());
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
        }
    }
}
