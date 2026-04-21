using System.Collections.Generic;

using ComposableUi;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TankRacerViewer.Core
{
    public sealed class Viewer3D : ContainerElement
    {
        public const float DefaultToolPanelHeight = 22;
        public const float DefaultToolPanelBottomPadding = 2;
        public const float DefaultToolPanelLeftPadding = 12;
        public const float DefaultToolPanelRightPadding = 2;

        public const float DefaultToolButtonsSpacing = 2;

        public const float DefaultResolutionListWidth = 110;
        public const float DefaultViewModeListWidth = 150;

        public const float DefaultInfoPaddings = 4;

        private const float DefaultSpacePlaceHolderSize = 6;

        private const string InfoMessage = "Controls:\nW,A,S,D: Move\nLeft Mouse Button: Look around\nR: Reset camera\nC: Speed up\nLeft Ctrl: Slow down\nHold Shift: Strafe";

        public RenderContextElement RenderContext { get; set; }

        public RenderInfoElement RenderInfo { get; set; }

        private readonly ContainerElement _toolPanel;
        private readonly DropDownListElement _resolutionList;
        private readonly DropDownListElement _viewModeList;
        private readonly ContentButtonElement _controlsInfoToggle;
        private readonly ContentButtonElement _statisticsToggle;

        private readonly RowLayout _infoLayout;

        private readonly AspectRatioFitterElement _aspectRatioFitter;

        private readonly List<Point> _resolutions = [];
        private readonly AspectRatioMode[] _viewModes =
        [
            AspectRatioMode.FitInParent,
            AspectRatioMode.EnvelopeParent
        ];

        public Viewer3D(GraphicsDevice graphicsDevice)
        {
            _resolutionList = new DropDownListElement(
                size: new Vector2(DefaultResolutionListWidth)
            );
            foreach (var displayMode in graphicsDevice.Adapter.SupportedDisplayModes)
            {
                _resolutions.Add(new Point(displayMode.Width, displayMode.Height));
                _resolutionList.AddItem(new($"{displayMode.Width}x{displayMode.Height}"));
            }
            _resolutionList.SelectItem(0);
            _resolutionList.ItemSelected += OnResolutionSelected;

            _viewModeList = new DropDownListElement(
                size: new Vector2(DefaultViewModeListWidth),
                items: [
                    new("Fit In Parent"),
                    new("Envelope Parent")
                ]
            );
            _viewModeList.SelectItem(0);
            _viewModeList.ItemSelected += OnViewModeSelected;

            _controlsInfoToggle = UiElementFactory.CreateToggleButton();
            _controlsInfoToggle.Icon.SetScaledSprite(IconName.ControlsInfo, UiElementFactory.DefaultSpriteScale);
            _controlsInfoToggle.Text.IsEnabled = false;
            _controlsInfoToggle.SetToggle(true);
            _controlsInfoToggle.PointerClick += OnControlsInfoToggleClicked;

            _statisticsToggle = UiElementFactory.CreateToggleButton();
            _statisticsToggle.Icon.SetScaledSprite(IconName.Statistics, UiElementFactory.DefaultSpriteScale);
            _statisticsToggle.Text.IsEnabled = false;
            _statisticsToggle.SetToggle(true);
            _statisticsToggle.PointerClick += OnStatisticsToggleClicked;

            _toolPanel = new ContainerElement(
                size: new Vector2(DefaultToolPanelHeight),
                children: [
                    new ExpandedElement(
                        expandWidth: false,
                        innerElement: new AlignmentElement(
                            alignmentFactor: Alignment.MiddleLeft,
                            pivot: Alignment.MiddleLeft,
                            innerElement: new RowLayout(
                                leftPadding: DefaultToolPanelLeftPadding,
                                sizeMainAxisToContent: true,
                                expandChildrenCrossAxis: true,
                                children: [
                                    new TextElement(
                                        text: "Resolution:",
                                        sizeToTextWidth: true,
                                        sizeToTextHeight: true
                                    ),
                                    _resolutionList,
                                    new Element()
                                    {
                                        Size = new Vector2(DefaultSpacePlaceHolderSize)
                                    },
                                    new TextElement(
                                        text: "View Mode:",
                                        sizeToTextWidth: true,
                                        sizeToTextHeight: true
                                    ),
                                    _viewModeList
                                ]
                            )
                        )
                    ),
                    new ExpandedElement(
                        expandWidth: false,
                        innerElement: new AlignmentElement(
                            alignmentFactor: Alignment.MiddleRight,
                            pivot: Alignment.MiddleRight,
                            innerElement: new RowLayout(
                                alignmentFactor: Alignment.MiddleLeft,
                                spacing: DefaultToolButtonsSpacing,
                                rightPadding: DefaultToolPanelRightPadding,
                                sizeMainAxisToContent: true,
                                expandChildrenCrossAxis: true,
                                children: [
                                    new LayoutElement(
                                        ignoreLayout: true,
                                        innerElement: new ExpandedElement(
                                            innerElement: new SpriteElement(
                                                skin: StandardSkin.DarkPixel
                                            )
                                        )
                                    ),
                                    _controlsInfoToggle,
                                    _statisticsToggle
                                ]
                            )
                        )
                    )
                ]
            );
            AddChild(new ExpandedElement(
                expandHeight: false,
                innerElement: new AlignmentElement(
                    alignmentFactor: Alignment.TopLeft,
                    pivot: Alignment.TopLeft,
                    innerElement: _toolPanel
                )
            ));

            RenderContext = new RenderContextElement(graphicsDevice, _resolutions[0]);

            RenderInfo = new RenderInfoElement();

            _infoLayout = new RowLayout(
                leftPadding: DefaultInfoPaddings,
                rightPadding: DefaultInfoPaddings,
                topPadding: DefaultInfoPaddings,
                bottomPadding: DefaultInfoPaddings,
                sizeMainAxisToContent: true,
                sizeCrossAxisToContent: true,
                children: [
                    new LayoutElement(
                        ignoreLayout: true,
                        innerElement: new ExpandedElement(
                            innerElement: new SpriteElement(
                                skin: StandardSkin.WhitePixel,
                                color: RenderInfoElement.DefaultBackgroundColor
                            )
                        )
                    ),
                    new TextElement(
                        text: InfoMessage,
                        sizeToTextWidth: true,
                        sizeToTextHeight: true
                    )
                ]
            );

            _aspectRatioFitter = new AspectRatioFitterElement(
                aspectRatioMode: AspectRatioMode.FitInParent,
                aspectRatio: RenderContext.AspectRatio,
                innerElement: RenderContext
            );
            AddChild(new ExpandedElement(
                topPadding: DefaultToolPanelHeight + DefaultToolPanelBottomPadding,
                innerElement: new ContainerElement(
                    children: [
                        new ExpandedElement(new ClipMaskElement(_aspectRatioFitter)),
                        new AlignmentElement(
                            alignmentFactor: Alignment.TopRight,
                            pivot: Alignment.TopRight,
                            innerElement: RenderInfo
                        ),
                        new AlignmentElement(
                            alignmentFactor: Alignment.BottomLeft,
                            pivot: Alignment.BottomLeft,
                            innerElement: _infoLayout
                        )
                    ]
                )
            ));
        }

        private void OnControlsInfoToggleClicked(PointerInputHandlerElement sender,
            PointerEvent pointerEvent)
        {
            _infoLayout.IsEnabled = !_infoLayout.IsEnabled;
            _controlsInfoToggle.SetToggle(_infoLayout.IsEnabled);
        }

        private void OnStatisticsToggleClicked(PointerInputHandlerElement sender,
            PointerEvent pointerEvent)
        {
            RenderInfo.IsEnabled = !RenderInfo.IsEnabled;
            _statisticsToggle.SetToggle(RenderInfo.IsEnabled);
        }

        private void OnResolutionSelected(DropDownListTextItemElement sender, int index)
        {
            var resolution = _resolutions[index];
            var aspectRatio = resolution.X / (float)resolution.Y;

            RenderContext.Resolution = resolution;
            _aspectRatioFitter.AspectRatio = aspectRatio;
        }

        private void OnViewModeSelected(DropDownListTextItemElement sender, int index)
        {
            _aspectRatioFitter.AspectRatioMode = _viewModes[index];
        }
    }
}
