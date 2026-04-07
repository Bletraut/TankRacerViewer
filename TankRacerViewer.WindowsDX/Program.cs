using DesktopCommon;

using TankRacerViewer.Core;
using TankRacerViewer.WindowsDX;

using var game = new MainWindow(new DesktopStorage("TankRacerViewer"),
    new WinFormsFileDialog());
game.Run();
