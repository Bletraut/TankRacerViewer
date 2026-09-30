using System.IO;
using System.Linq;
using System.Reflection;

using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ComposableUi
{
    public static class BuiltInSprite
    {
        // Colors.
        public static readonly ThemeSpriteSource WhitePixel = new(nameof(WhitePixel));
        public static readonly ThemeSpriteSource GentleWhitePixel = new(nameof(GentleWhitePixel));
        public static readonly ThemeSpriteSource SoftLightPixel = new(nameof(SoftLightPixel));
        public static readonly ThemeSpriteSource SoftDarkPixel = new(nameof(SoftDarkPixel));
        public static readonly ThemeSpriteSource DarkPixel = new(nameof(DarkPixel));
        public static readonly ThemeSpriteSource SolidDarkPixel = new(nameof(SolidDarkPixel));

        public static readonly ThemeSpriteSource HoverStrongDarkPixel = new(nameof(HoverStrongDarkPixel));
        public static readonly ThemeSpriteSource HoverSoftDarkPixel = new(nameof(HoverSoftDarkPixel));
        public static readonly ThemeSpriteSource HoverSoftLightPixel = new(nameof(HoverSoftLightPixel));

        public static readonly ThemeSpriteSource SelectionStrongDarkPixel = new(nameof(SelectionStrongDarkPixel));
        public static readonly ThemeSpriteSource SelectionStrongLightPixel = new(nameof(SelectionStrongLightPixel));
        public static readonly ThemeSpriteSource SelectionSoftDarkPixel = new(nameof(SelectionSoftDarkPixel));

        // Elements.
        public static readonly ThemeSpriteSource RectanglePanel = new(nameof(RectanglePanel));

        public static readonly ThemeSpriteSource ContentPanel = new(nameof(ContentPanel));

        public static readonly ThemeSpriteSource LightRectangle = new(nameof(LightRectangle));
        public static readonly ThemeSpriteSource HoverLightRectangle = new(nameof(HoverLightRectangle));
        public static readonly ThemeSpriteSource DarkRectangle = new(nameof(DarkRectangle));
        public static readonly ThemeSpriteSource HoverDarkRectangle = new(nameof(HoverDarkRectangle));
        public static readonly ThemeSpriteSource SoftDarkRectangle = new(nameof(SoftDarkRectangle));
        public static readonly ThemeSpriteSource HoverSoftDarkRectangle = new(nameof(HoverSoftDarkRectangle));

        public static readonly ThemeSpriteSource ScrollButton = new(nameof(ScrollButton));
        public static readonly ThemeSpriteSource ScrollButtonHover = new(nameof(ScrollButtonHover));

        public static readonly ThemeSpriteSource TextField = new(nameof(TextField));

        public static readonly ThemeSpriteSource RoundedButton = new(nameof(RoundedButton));
        public static readonly ThemeSpriteSource HoverRoundedButton = new(nameof(HoverRoundedButton));
        public static readonly ThemeSpriteSource PressedRoundedButton = new(nameof(PressedRoundedButton));
        public static readonly ThemeSpriteSource DisabledRoundedButton = new(nameof(DisabledRoundedButton));

        public static readonly ThemeSpriteSource RectangleButton = new(nameof(RectangleButton));
        public static readonly ThemeSpriteSource HoverRectangleButton = new(nameof(HoverRectangleButton));
        public static readonly ThemeSpriteSource PressedRectangleButton = new(nameof(PressedRectangleButton));
        public static readonly ThemeSpriteSource DisabledRectangleButton = new(nameof(DisabledRectangleButton));

        public static readonly ThemeSpriteSource LightRectangleButton = new(nameof(LightRectangleButton));
        public static readonly ThemeSpriteSource HoverLightRectangleButton = new(nameof(HoverLightRectangleButton));
        public static readonly ThemeSpriteSource PressedLightRectangleButton = new(nameof(PressedLightRectangleButton));
        public static readonly ThemeSpriteSource DisabledLightRectangleButton = new(nameof(DisabledLightRectangleButton));

        public static readonly ThemeSpriteSource InactiveTab = new(nameof(InactiveTab));
        public static readonly ThemeSpriteSource ActiveTab = new(nameof(ActiveTab));
        public static readonly ThemeSpriteSource SelectedTab = new(nameof(SelectedTab));
        public static readonly ThemeSpriteSource TabButtonsBackground = new(nameof(TabButtonsBackground));
        public static readonly ThemeSpriteSource WindowBody = new(nameof(WindowBody));

        // Icons.
        public static readonly ThemeSpriteSource LeftArrowIcon = new(nameof(LeftArrowIcon));
        public static readonly ThemeSpriteSource RightArrowIcon = new(nameof(RightArrowIcon));
        public static readonly ThemeSpriteSource UpArrowIcon = new(nameof(UpArrowIcon));
        public static readonly ThemeSpriteSource DownArrowIcon = new(nameof(DownArrowIcon));
        public static readonly ThemeSpriteSource CloseIcon = new(nameof(CloseIcon));
        public static readonly ThemeSpriteSource MaximizeWindowIcon = new(nameof(MaximizeWindowIcon));
        public static readonly ThemeSpriteSource RestoreWindowIcon = new(nameof(RestoreWindowIcon));

        public static void Load(ContentManager contentManager, SpriteResolver spriteResolver,
            string spriteSheetAssetName, int defaultSpriteScale = 1)
        {
            var assembly = Assembly.GetExecutingAssembly();

            var atlasResourceName = assembly.GetManifestResourceNames()
                .First(resource => resource.EndsWith("UiElementsAtlas.json"));

            using var stream = assembly.GetManifestResourceStream(atlasResourceName);
            using var reader = new StreamReader(stream);
            var spriteSheetJson = reader.ReadToEnd();

            var spriteSheetTexture = contentManager.Load<Texture2D>(spriteSheetAssetName);
            spriteResolver.AddAsepriteSpriteSheet(spriteSheetTexture, spriteSheetJson, defaultSpriteScale);
        }
    }
}
