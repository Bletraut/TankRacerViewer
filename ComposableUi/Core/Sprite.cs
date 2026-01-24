using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ComposableUi
{
    public sealed class Sprite
    {
        public Texture2D Texture { get; set; }
        public Rectangle SourceRectangle { get; set; }
        public int LeftBorder { get; set; }
        public int RightBorder { get; set; }
        public int TopBorder { get; set; }
        public int BottomBorder { get; set; }
        public bool IsSliced => LeftBorder != 0 && RightBorder != 0 && TopBorder != 0 && BottomBorder != 0;
    }
}
