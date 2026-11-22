using Microsoft.Xna.Framework;

namespace ComposableUi
{
    public readonly record struct SpriteDrawCommand(Sprite Sprite, DrawMode DrawMode,
        Rectangle DestinationRectangle, Color Color);
}
