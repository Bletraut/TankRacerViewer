using System;

using ComposableUi;

namespace TankRacerViewer.Core
{
    public sealed class InspectorWindow : WindowElement
    {
        private readonly TextureInspectorElement _textureInspector;
        private readonly ModelInspectorElement _modelInspector;
        private readonly LevelInspectorElement _levelInspector;
        private readonly TankInspectorElement _tankInspector;
        private readonly BackgroundInspectorElement _backgroundInspector;

        private readonly ScrollViewElement _scrollView;

        public InspectorWindow() : base("Inspector")
        {
            this.SetScaledIcon(IconName.Inspector, UiElementFactory.DefaultSpriteScale);

            _scrollView = new ScrollViewElement(
                expandingContentWidthMode: ScrollViewElement.ExpandingMode.FillParent
            );
            ContentContainer.AddChild(new ExpandedElement(_scrollView));

            _textureInspector = new TextureInspectorElement();
            _modelInspector = new ModelInspectorElement();
            _levelInspector = new LevelInspectorElement();
            _tankInspector = new TankInspectorElement();
            _backgroundInspector = new BackgroundInspectorElement();
        }

        public void ShowTextureInspector(TextureAssetView textureAssetView)
        {
            _scrollView.Content = _textureInspector;
            _textureInspector.SetTarget(textureAssetView);
        }

        public void ShowModelInspector(ModelAssetView modelAssetView)
        {
            _scrollView.Content = _modelInspector;
            _modelInspector.SetTarget(modelAssetView);
        }

        public void ShowLevelInspector(LevelView levelView,
            Action<LevelObject> levelObjectSelectedAction)
        {
            _scrollView.Content = _levelInspector;
            _levelInspector.SetTarget(levelView);
            _levelInspector.LevelObjectSelectedAction = levelObjectSelectedAction;
        }

        public void ShowTankInspector(TankView tankView,
            Action<LevelObject> levelObjectSelectedAction)
        {
            _scrollView.Content = _tankInspector;
            _tankInspector.SetTarget(tankView);
            _tankInspector.LevelObjectSelectedAction = levelObjectSelectedAction;
        }

        public void ShowBackgroundInspector(BackgroundAssetView backgroundAssetView)
        {
            _scrollView.Content = _backgroundInspector;
            _backgroundInspector.SetTarget(backgroundAssetView);
        }

        public void HideInspector()
        {
            _scrollView.Content = null;
        }
    }
}
