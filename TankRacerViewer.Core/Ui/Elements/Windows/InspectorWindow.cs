using ComposableUi;

namespace TankRacerViewer.Core
{
    public sealed class InspectorWindow : WindowElement
    {
        private readonly TextureInspectorElement _textureInspector;
        private readonly ModelInspectorElement _modelInspector;

        private readonly ScrollViewElement _scrollView;

        public InspectorWindow() : base("Inspector")
        {
            _scrollView = new ScrollViewElement(
                expandContentWidth: true,
                sizeToContentHeight: true
            );
            ContentContainer.AddChild(new ExpandedElement(_scrollView));

            _textureInspector = new TextureInspectorElement();
            _modelInspector = new ModelInspectorElement();
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

        public void HideInspector()
        {
            _scrollView.Content = null;
        }
    }
}
