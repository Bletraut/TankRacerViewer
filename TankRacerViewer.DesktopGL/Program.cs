using System;
#if !DEBUG
using System.IO;
#endif

using DesktopCommon;

using Microsoft.Xna.Framework.Content;

using TankRacerViewer.Core;
using TankRacerViewer.DesktopGL;

try
{
    // Workaround for Monogame trimming.
    var listReaderFullName = typeof(ListReader<char>).FullName.Replace("9.0", "8.0");
    ContentTypeReaderManager.AddTypeCreator(listReaderFullName, () => new ListReader<char>());

    using var game = new MainWindow(new DesktopStorage("TankRacerViewer"),
    new DesktopUrlOpener(), new NativeFileDialog());
    game.Run();
}
catch (Exception exception)
{
#if !DEBUG
    File.WriteAllText("crash.log", exception.ToString());
#endif
    throw;
}
