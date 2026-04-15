using System.Linq;
using System.Reflection;

using ComposableUi;

using Microsoft.Xna.Framework;

namespace TankRacerViewer.Core
{
    public sealed class AboutWindow : WindowElement
    {
        private readonly IPlatformUrlOpener _urlOpener;

        private readonly string _repositoryUrl;
        private readonly ContentButtonElement _repositoryLinkButton;
        private readonly ContentButtonElement _closeButton;

        public AboutWindow(IPlatformUrlOpener urlOpener) : base("About")
        {
            this.SetIcon(IconName.About);

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
                normalTextColor: Color.Blue,
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
                normalSkin: StandardSkin.ContentPanel,
                hoverSkin: StandardSkin.ContentPanel,
                pressedSkin: StandardSkin.ContentPanel,
                normalTextColor: Color.White,
                hoverTextColor: Color.LightYellow,
                pressedTextColor: Color.WhiteSmoke
            );
            _closeButton.ContentLayout.BottomPadding = 8;
            _closeButton.Icon.IsEnabled = false;
            _closeButton.PointerClick += OnCloseButtonClicked;

            ContentContainer.AddChild(new ExpandedElement(
                innerElement: new ColumnLayout(
                    spacing: 4,
                    sizeCrossAxisToContent: true,
                    expandChildrenCrossAxis: true,
                    children: [
                        new ColumnLayout(
                            alignmentFactor: Alignment.Center,
                            sizeMainAxisToContent: true,
                            children: [
                                new SpriteElement(
                                    size: new Vector2(100),
                                    skin: StandardSkin.WhitePixel,
                                    sizeToSource: true,
                                    drawMode: DrawMode.Simple
                                )
                            ]
                        ),
                        new TextElement(
                            sizeToTextWidth: true,
                            sizeToTextHeight: true,
                            text: $"{product} v{version}",
                            color: Color.Black
                        ),
                        new TextElement(
                            sizeToTextWidth: true,
                            sizeToTextHeight: true,
                            text: description,
                            color: Color.Black
                        ),
                        new Element()
                        {
                            Size = new Vector2(12)
                        },
                        new TextElement(
                            sizeToTextWidth: true,
                            sizeToTextHeight: true,
                            text: copyright,
                            color: Color.Black
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
