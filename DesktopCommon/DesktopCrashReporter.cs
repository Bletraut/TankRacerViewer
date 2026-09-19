namespace DesktopCommon
{
    public static class DesktopCrashReporter
    {
        public static void Run(string fileName, Action action)
        {
            try
            {
                action.Invoke();
            }
            catch (Exception exception)
            {
#if !DEBUG
                File.AppendAllText(fileName, $"[{DateTime.Now}] {exception}\n");
#endif
                throw;
            }
        }
    }
}
