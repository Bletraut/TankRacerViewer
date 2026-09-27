namespace ComposableUi
{
    public sealed class ThemeSpriteSource(string name) : ISpriteSource
    {
        public string Name { get; init; } = name;

        Sprite ISpriteSource.Resolve(Context context)
        {
            if (context is null)
                return null;

            context.Theme.SpriteResolver.TryGetSprite(Name, out var sprite);
            return sprite;
        }
    }
}
