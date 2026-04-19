using System.Linq;
using System.Reflection;

using ComposableUi;

using Microsoft.Xna.Framework;

namespace TankRacerViewer.Core
{
    public sealed class AboutWindow : WindowElement
    {
        public const float DefaultContentTopPadding = 8;
        public const float DefaultContentHorizontalPaddings = 8;

        private readonly IPlatformUrlOpener _urlOpener;

        private readonly string _repositoryUrl;
        private readonly ContentButtonElement _repositoryLinkButton;
        private readonly ContentButtonElement _closeButton;

        public AboutWindow(IPlatformUrlOpener urlOpener) : base("About")
        {
            this.SetScaledIcon(IconName.About, UiElementFactory.DefaultSpriteScale);

            _urlOpener = urlOpener;

            var assembly = Assembly.GetExecutingAssembly();

            var version = assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version;
            var product = assembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product;
            var copyright = assembly.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright;
            var description = assembly.GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description;

            _repositoryUrl = assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
                .FirstOrDefault(metadata => metadata.Key == "RepositoryUrl")?.Value;

            _repositoryLinkButton = new ContentButtonElement(
                text: _repositoryUrl,
                normalSkin: StandardSkin.None,
                hoverSkin: StandardSkin.None,
                pressedSkin: StandardSkin.None,
                normalTextColor: Color.DeepSkyBlue,
                hoverTextColor: Color.Coral,
                pressedTextColor: Color.DarkBlue
            );
            _repositoryLinkButton.ContentLayout.LeftPadding = 0;
            _repositoryLinkButton.ContentLayout.RightPadding = 0;
            _repositoryLinkButton.ContentLayout.TopPadding = 0;
            _repositoryLinkButton.ContentLayout.BottomPadding = 0;
            _repositoryLinkButton.Icon.IsEnabled = false;
            _repositoryLinkButton.PointerClick += OnRepositoryLinkButtonClicked;

            _closeButton = new ContentButtonElement(
                text: "Close",
                normalSkin: StandardSkin.RoundedButton,
                hoverSkin: StandardSkin.HoverRoundedButton,
                pressedSkin: StandardSkin.PressedRoundedButton,
                normalTextColor: Color.Black,
                hoverTextColor: Color.Black,
                pressedTextColor: Color.Black
            );
            _closeButton.ContentLayout.BottomPadding = 8;
            _closeButton.Icon.IsEnabled = false;
            _closeButton.PointerClick += OnCloseButtonClicked;

            ContentContainer.AddChild(new ExpandedElement(
                innerElement: new ColumnLayout(
                    alignmentFactor: Alignment.TopLeft,
                    spacing: 4,
                    leftPadding: DefaultContentHorizontalPaddings,
                    rightPadding: DefaultContentHorizontalPaddings,
                    topPadding: DefaultContentTopPadding,
                    sizeCrossAxisToContent: true,
                    expandChildrenCrossAxis: true,
                    children: [
                        new ColumnLayout(
                            alignmentFactor: Alignment.Center,
                            sizeMainAxisToContent: true,
                            children: [
                                new SpriteElement(
                                    size: new Vector2(100),
                                    sprite: IconCollection.Get(IconName.Logo),
                                    sizeToSource: false,
                                    drawMode: DrawMode.Simple
                                )
                            ]
                        ),
                        new TextElement(
                            sizeToTextWidth: true,
                            sizeToTextHeight: true,
                            text: $"{product} v{version}",
                            color: Color.White
                        ),
                        new TextElement(
                            sizeToTextWidth: true,
                            sizeToTextHeight: true,
                            text: description,
                            color: Color.White
                        ),
                        new Element()
                        {
                            Size = new Vector2(12)
                        },
                        new TextElement(
                            sizeToTextWidth: true,
                            sizeToTextHeight: true,
                            text: copyright,
                            color: Color.White
                        ),
                        new HolderElement(
                            size: new Vector2(16),
                            innerElement: new AlignmentElement(
                                alignmentFactor: Alignment.MiddleLeft,
                                pivot: Alignment.MiddleLeft,
                                innerElement: _repositoryLinkButton
                            )
                        ),
                        new Element()
                        {
                            Size = new Vector2(18)
                        },
                        _closeButton
                    ]
                )
            ));
        }

        private void OnRepositoryLinkButtonClicked(PointerInputHandlerElement sender,
            PointerEvent pointerEvent)
        {
            _urlOpener.OpenUrl(_repositoryUrl);
        }

        private void OnCloseButtonClicked(PointerInputHandlerElement sender,
            PointerEvent pointerEvent)
        {
            Close();
        }
    }
}
