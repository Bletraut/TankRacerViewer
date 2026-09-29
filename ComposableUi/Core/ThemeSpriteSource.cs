namespace ComposableUi
{
    public sealed class ThemeSpriteSource(string name) : ISpriteSource
    {
        private static readonly Sprite UndefinedSprite = new();

        public string Name { get; init; } = name;

        Sprite ISpriteSource.Resolve(Context context)
        {
            if (context is null)
                return null;

            if (!context.Theme.SpriteResolver.TryGetSprite(Name, out var sprite))
                sprite = UndefinedSprite;

            return sprite;
        }
    }
}
