using System;
using System.Text;

using ComposableUi;

using Microsoft.Xna.Framework;

namespace TankRacerViewer.Core
{
    public sealed class LevelObjectElement : ClipMaskElement,
        ILazyListItem<LevelObject>
    {
        public const int DefaultHeight = 30;
        public const int DefaultButtonPaddings = 2;

        public const int DefaultContentSpacing = 4;
        public const int DefaultContentPaddings = 4;

        // Static.
        public static readonly Vector2 DefaultIconSize = new(18);

        public static readonly Color DefaultNormalButtonColor = Color.Plum;
        public static readonly Color DefaultHoverButtonColor = Color.Coral;
        public static readonly Color DefaultPressedButtonColor = Color.LightSlateGray;

        public static readonly Color EnabledBoundingBoxColor = Color.Fuchsia;
        public static readonly Color SelectedBoundingBoxColor = Color.GreenYellow;

        public static readonly Color DefaultNormalBackgroundColor = Color.DarkSlateBlue;
        public static readonly Color DefaultHoverBackgroundColor = Color.DeepSkyBlue;

        private static readonly StringBuilder _stringBuilder = new();

        public static ContentButtonElement CreateButton(StandardSkin iconSkin = default)
        {
            var button = new ContentButtonElement(
                iconSize: DefaultIconSize,
                iconSkin: iconSkin,
                normalSkin: StandardSkin.WhitePixel,
                hoverSkin: StandardSkin.WhitePixel,
                pressedSkin: StandardSkin.WhitePixel,
                disabledSkin: StandardSkin.WhitePixel,
                normalColor: DefaultNormalButtonColor,
                hoverColor: DefaultHoverButtonColor,
                pressedColor: DefaultPressedButtonColor
            );
            button.Text.IsEnabled = false;
            button.ContentLayout.LeftPadding = DefaultButtonPaddings;
            button.ContentLayout.RightPadding = DefaultButtonPaddings;
            button.ContentLayout.TopPadding = DefaultButtonPaddings;
            button.ContentLayout.BottomPadding = DefaultButtonPaddings;

            return button;
        }

        // Class.
        public event Action<LevelObject> VisibilityChanged;
        public event Action<LevelObject> BoundingBoxVisibilityChanged;
        public event Action<LevelObject> TargetSelected;

        private readonly SpriteElement _background;
        private readonly ContentButtonElement _visibilityButton;
        private readonly ContentButtonElement _boundingBoxButton;
        private readonly ContentButtonElement _lookAtButton;
        private readonly PointerInputHandlerElement _hoverInputHandler;

        private readonly TextElement _name;

        private LevelObject _data;
        private bool _isBoundingBoxEnabled;

        public LevelObjectElement()
        {
            _background = new SpriteElement(
                skin: StandardSkin.WhitePixel,
                color: DefaultNormalBackgroundColor
            );

            _visibilityButton = CreateButton();
            _visibilityButton.PointerClick += OnVisibilityButtonPointerClick;

            _boundingBoxButton = CreateButton();
            _boundingBoxButton.PointerClick += OnBoundingBoxButtonPointerClick;

            _lookAtButton = CreateButton(iconSkin: StandardSkin.ScrollButton);
            _lookAtButton.PointerClick += OnLookAtButtonPointerClick;

            _name = new TextElement(
                sizeToTextWidth: true,
                sizeToTextHeight: true
            );

            _hoverInputHandler = new PointerInputHandlerElement(
                blockInput: false
            );
            _hoverInputHandler.PointerEnter += OnHoverInputHandlerPointerEnter;
            _hoverInputHandler.PointerLeave += OnHoverInputHandlerPointerLeave;

            InnerElement = new RowLayout(
                alignmentFactor: Alignment.MiddleLeft,
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
                            innerElement: _background
                        )
                    ),
                    _visibilityButton,
                    _boundingBoxButton,
                    _lookAtButton,
                    _name,
                    new LayoutElement(
                        ignoreLayout: true,
                        innerElement: new ExpandedElement(
                            innerElement: _hoverInputHandler
                        )
                    )
                ]
            );
        }

        public void RefreshButtonsVisualState()
        {
            _isBoundingBoxEnabled = _data.IsBoundingBoxEnabled;

            RefreshVisibilityButtonVisualState();
            RefreshBoundingBoxButtonVisualState();
        }

        private void RefreshBackgroundColor()
        {
            _background.Color = _hoverInputHandler.IsHover
                ? DefaultHoverBackgroundColor
                : DefaultNormalBackgroundColor;
        }

        private void RefreshBoundingBoxColor()
        {
            if (_data is null)
                return;

            _data.BoundingBoxColor = _isBoundingBoxEnabled
                ? EnabledBoundingBoxColor
                : SelectedBoundingBoxColor;
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

            _boundingBoxButton.Icon.Skin = _isBoundingBoxEnabled
                ? StandardSkin.PressedRoundedButton
                : StandardSkin.DisabledRoundedButton;
        }

        void ILazyListItem<LevelObject>.SetData(LevelObject data)
        {
            if (_data == data)
                return;

            _data = data;

            _stringBuilder.Clear();
            _stringBuilder.AppendLine($"Name: {_data.ModelAssetView.Name}");
            _stringBuilder.Append($"Type: {_data.Type}");
            _name.Text = _stringBuilder.ToString();

            RefreshButtonsVisualState();

            RefreshBackgroundColor();
            RefreshBoundingBoxColor();
        }

        void ILazyListItem<LevelObject>.ClearData()
        {
            _data.IsBoundingBoxEnabled = _isBoundingBoxEnabled;

            _data = null;
        }

        private void OnVisibilityButtonPointerClick(PointerInputHandlerElement sender,
            PointerEvent pointerEvent)
        {
            _data.IsEnabled = !_data.IsEnabled;
            RefreshVisibilityButtonVisualState();

            VisibilityChanged?.Invoke(_data);
        }

        private void OnBoundingBoxButtonPointerClick(PointerInputHandlerElement sender,
            PointerEvent pointerEvent)
        {
            _isBoundingBoxEnabled = !_isBoundingBoxEnabled;

            RefreshBoundingBoxColor();
            RefreshBoundingBoxButtonVisualState();

            BoundingBoxVisibilityChanged?.Invoke(_data);
        }

        private void OnLookAtButtonPointerClick(PointerInputHandlerElement sender,
            PointerEvent pointerEvent)
        {
            TargetSelected?.Invoke(_data);
        }

        private void OnHoverInputHandlerPointerEnter(PointerInputHandlerElement sender,
            PointerEvent pointerEvent)
        {
            _data.IsBoundingBoxEnabled = true;
            RefreshBackgroundColor();
        }

        private void OnHoverInputHandlerPointerLeave(PointerInputHandlerElement sender,
            PointerEvent pointerEvent)
        {
            if (_data is null)
                return;

            _data.IsBoundingBoxEnabled = _isBoundingBoxEnabled;
            RefreshBackgroundColor();
        }
    }
}
