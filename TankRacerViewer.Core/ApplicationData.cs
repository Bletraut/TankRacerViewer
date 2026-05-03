using System.Collections.Generic;

namespace TankRacerViewer
{
    public sealed class ApplicationData
    {
        // Static.
        public static ApplicationData CreateEmpty()
        {
            var data = new ApplicationData();
            data.RecentPaths = [];

            return data;
        }

        // Class.
        public List<string> RecentPaths { get; set; }
    }
}
