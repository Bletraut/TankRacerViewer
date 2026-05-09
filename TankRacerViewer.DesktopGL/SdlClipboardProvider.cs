using System;
using System.Runtime.InteropServices;
using System.Text;

using ComposableUi;

namespace TankRacerViewer.DesktopGL
{
    public sealed class SdlClipboardProvider : IClipboardProvider
    {
        string IClipboardProvider.GetText()
        {
            var ptr = SDL_GetClipboardText();
            if (ptr == IntPtr.Zero) return string.Empty;
            try { return Marshal.PtrToStringUTF8(ptr) ?? string.Empty; }
            finally { SDL_free(ptr); }
        }

        void IClipboardProvider.SetText(string text)
        {
            if (string.IsNullOrEmpty(text)) return;
            var bytes = Encoding.UTF8.GetBytes(text + "\0");
            SDL_SetClipboardText(bytes);
        }

        [DllImport("SDL2", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr SDL_GetClipboardText();

        [DllImport("SDL2", CallingConvention = CallingConvention.Cdecl)]
        private static extern int SDL_SetClipboardText(byte[] text);

        [DllImport("SDL2", CallingConvention = CallingConvention.Cdecl)]
        private static extern void SDL_free(IntPtr ptr);
    }
}
