using System;
using System.Diagnostics;

using ComposableUi;

using Microsoft.Xna.Framework;

namespace TankRacerViewer.Core
{
    public sealed class LevelObjectElement : SizedToContentHolderElement,
        ILazyListItem<LevelObject>
    {
        public const int DefaultHeight = 30;

        public const int DefaultContentSpacing = 4;
        public const int DefaultContentPaddings = 4;

        // Static.
        public static readonly Color DefaultBackgroundColor = Color.MediumSlateBlue;

        // Class.
        public event Action<LevelObject> TargetSelected;

        private readonly IconButtonElement _visibilityButton;
        private readonly IconButtonElement _boundingBoxButton;
        private readonly IconButtonElement _lookAtButton;

        private readonly TextElement _name;

        private LevelObject _data;

        public LevelObjectElement()
        {
            _visibilityButton = new IconButtonElement();
            _visibilityButton.PointerClick += OnVisibilityButtonPointerClick;

            _boundingBoxButton = new IconButtonElement();
            _boundingBoxButton.PointerClick += OnBoundingBoxButtonPointerClick;

            _lookAtButton = new IconButtonElement(null, StandardSkin.ScrollButton);
            _lookAtButton.PointerClick += OnLookAtButtonPointerClick;

            _name = new TextElement(
                textAlignmentFactor: Alignment.MiddleLeft,
                sizeToTextWidth: true,
                sizeToTextHeight: true
            );

            InnerElement = new RowLayout(
                alignmentFactor: Alignment.TopLeft,
                leftPadding: DefaultContentPaddings,
                rightPadding: DefaultContentPaddings,
                topPadding: DefaultContentPaddings,
                bottomPadding: DefaultContentPaddings,
                spacing: DefaultContentSpacing,
                sizeCrossAxisToContent: true,
                children: [
                    new LayoutElement(
                        ignoreLayout: true,
                        innerElement: new ExpandedElement(
                            innerElement: new SpriteElement(
                                skin: StandardSkin.WhitePixel,
                                color: DefaultBackgroundColor
                            )
                        )
                    ),
                    _visibilityButton,
                    _boundingBoxButton,
                    _lookAtButton,
                    _name
                ]
            );
        }

        private void RefreshVisibilityButtonVisualState()
        {
            if (_data is null)
                return;

            _visibilityButton.Icon.Skin = _data.IsEnabled
                ? StandardSkin.PressedRectangleButton
                : StandardSkin.DisabledRectangleButton;
        }

        private void RefreshBoundingBoxButtonVisualState()
        {
            if (_data is null)
                return;

            _boundingBoxButton.Icon.Skin = _data.IsBoundingBoxEnabled
                ? StandardSkin.PressedRoundedButton
                : StandardSkin.DisabledRoundedButton;
        }

        void ILazyListItem<LevelObject>.SetData(LevelObject data)
        {
            _data = data;

            _name.Text = $"Name: {_data.ModelAssetView.Name}";

            RefreshVisibilityButtonVisualState();
            RefreshBoundingBoxButtonVisualState();
        }

        void ILazyListItem<LevelObject>.ClearData()
        {
            _data = null;
        }

        private void OnVisibilityButtonPointerClick(PointerInputHandlerElement sender,
            PointerEvent pointerEvent)
        {
            _data.IsEnabled = !_data.IsEnabled;
            RefreshVisibilityButtonVisualState();
        }

        private void OnBoundingBoxButtonPointerClick(PointerInputHandlerElement sender,
            PointerEvent pointerEvent)
        {
            _data.IsBoundingBoxEnabled = !_data.IsBoundingBoxEnabled;
            RefreshBoundingBoxButtonVisualState();
        }

        private void OnLookAtButtonPointerClick(PointerInputHandlerElement sender,
            PointerEvent pointerEvent)
        {
            TargetSelected?.Invoke(_data);
        }
    }
}
