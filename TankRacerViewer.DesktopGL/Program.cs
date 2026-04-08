using DesktopCommon;

using TankRacerViewer.Core;
using TankRacerViewer.DesktopGL;

using var game = new MainWindow(new DesktopStorage("TankRacerViewer"),
    new DesktopUrlOpener(), new NativeFileDialog());
game.Run();
