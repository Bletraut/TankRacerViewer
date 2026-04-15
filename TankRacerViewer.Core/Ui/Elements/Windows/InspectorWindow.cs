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
            this.SetIcon(IconName.Inspector);

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
            _textureInspector.SetTarget(textureAssetView);
            _scrollView.Content = _textureInspector;
        }

        public void ShowModelInspector(ModelAssetView modelAssetView)
        {
            _modelInspector.SetTarget(modelAssetView);
            _scrollView.Content = _modelInspector;
        }

        public void ShowLevelInspector(LevelView levelView,
            Action<LevelObject> levelObjectSelectedAction)
        {
            _levelInspector.SetTarget(levelView);
            _levelInspector.LevelObjectSelectedAction = levelObjectSelectedAction;
            _scrollView.Content = _levelInspector;
        }

        public void ShowTankInspector(TankView tankView,
            Action<LevelObject> levelObjectSelectedAction)
        {
            _tankInspector.SetTarget(tankView);
            _tankInspector.LevelObjectSelectedAction = levelObjectSelectedAction;
            _scrollView.Content = _tankInspector;
        }

        public void ShowBackgroundInspector(BackgroundAssetView backgroundAssetView)
        {
            _backgroundInspector.SetTarget(backgroundAssetView);
            _scrollView.Content = _backgroundInspector;
        }

        public void HideInspector()
        {
            _scrollView.Content = null;
        }
    }
}
