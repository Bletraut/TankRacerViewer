using ComposableUi;

namespace TankRacerViewer.Core
{
    public sealed class FoldableFileGroupElement : FoldableGroupElement<FoldableFileGroupElement>
    {
        public string Path { get; }
        public object File { get; }

        public FoldableFileGroupElement(string path,
            object file = default,
            Sprite iconSprite = default,
            StandardSkin iconSkin = default,
            string name = default,
            bool isFolded = default)
            : base(iconSprite,
                  iconSkin,
                  name,
                  null,
                  isFolded)
        {
            Path = path;
            File = file;
        }
    }
}
