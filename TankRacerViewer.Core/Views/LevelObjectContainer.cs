using System.Collections.Generic;

namespace TankRacerViewer.Core
{
    public sealed class LevelObjectContainer(string fullName,
        IReadOnlyList<LevelObject> levelObjects)
    {
        public string FullName { get; } = fullName;
        public IReadOnlyList<LevelObject> LevelObjects { get; } = levelObjects;
    }
}
