using DesktopCommon;

using TankRacerViewer.Core;
using TankRacerViewer.WindowsDX;

DesktopCrashReporter.Run("CrashReport.log", static () =>
{
    using var game = new MainWindow(new DesktopStorage("TankRacerViewer"),
        new DesktopUrlOpener(), new WinFormsFileDialog());
    game.Run();
});
