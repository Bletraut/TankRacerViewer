using Microsoft.Xna.Framework;

namespace ComposableUi
{
    public class ScrollBarElement : ContainerElement
    {
        public const float DefaultMainAxisSize = 100;
        public const float DefaultCrossAxisSize = 14;

        public const float DefaultMainAxisPadding = 2;
        public const float DefaultCrossAxisPadding = 2;

        private float _progressValue;
        public float ProgressValue
        {
            get => _progressValue;
            set
            {
                if (_progressValue != value)
                    OnStateChanged();

                _progressValue = value;

                var (min, max) = CalculateScrollBounds();
                Button.LocalPosition = min + (max - min) * _progressValue;
            }
        }

        public float MainAxisSize => Vector2.Dot(Size, MainAxis);
        public float CrossAxisSize => Vector2.Dot(Size, CrossAxis);

        public Vector2 MainAxis { get; }
        public Vector2 CrossAxis { get; }

        public SpriteElement Background { get; }
        public ButtonElement Button { get; }

        public event ElementEventHandler<float> ProgressValueChanged;

        public ScrollBarElement(Vector2 mainAxis, Vector2 crossAxis,
            Vector2? size = default)
        {
            MainAxis = mainAxis;
            CrossAxis = crossAxis;

            var defaultSize = new Vector2(DefaultMainAxisSize) * mainAxis
                + new Vector2(DefaultCrossAxisSize) * crossAxis;
            Size = size ?? defaultSize;

            var defaultPadding = new Vector2(DefaultMainAxisPadding) * mainAxis
                + new Vector2(DefaultCrossAxisPadding) * crossAxis;

            Background = new SpriteElement(skin: StandardSkin.SolidDarkPixel);
            AddChild(new ExpandedElement(innerElement: Background));

            Button = new ButtonElement(new Vector2(DefaultCrossAxisSize, DefaultCrossAxisSize),
                normalSkin: StandardSkin.ScrollButton,
                hoverSkin: StandardSkin.ScrollButtonHover,
                pressedSkin: StandardSkin.ScrollButtonHover,
                disabledSkin: StandardSkin.ScrollButtonHover);
            AddChild(new ExpandedElement(Button,
                expandWidth: mainAxis.Y > 0,
                expandHeight: mainAxis.X > 0,
                leftPadding: defaultPadding.X,
                rightPadding: defaultPadding.X,
                topPadding: defaultPadding.Y,
                bottomPadding: defaultPadding.Y));

            Button.PointerDrag += OnButtonPointerDrag;
        }

        private void ApplyButtonPosition(Vector2 position)
        {
            var (min, max) = CalculateScrollBounds();

            Button.LocalPosition = Vector2.Clamp(position, min, max);

            var currentValue = Vector2.Dot(Button.LocalPosition - min, MainAxis);
            var maxValue = Vector2.Dot(max - min, MainAxis);
            _progressValue = currentValue / maxValue;
            OnProgressValueChanged();
        }

        private (Vector2 Min, Vector2 Max) CalculateScrollBounds()
        {
            var buttonTopLeft = -Button.Size * Button.Pivot;
            var buttonBottomRight = buttonTopLeft + Button.Size;

            var parentTopLeft = -Parent.Size * Parent.Pivot;
            var parentBottomRight = parentTopLeft + Parent.Size;

            var min = parentTopLeft - buttonTopLeft + new Vector2(DefaultMainAxisPadding);
            var max = parentBottomRight - buttonBottomRight - new Vector2(DefaultMainAxisPadding);

            return (min, max);
        }

        private void OnProgressValueChanged()
        {
            ProgressValueChanged?.Invoke(this, _progressValue);
        }

        private void OnButtonPointerDrag(Element sender, Point delta)
        {
            ApplyButtonPosition(Button.LocalPosition + delta.ToVector2() * MainAxis);
        }
    }
}
