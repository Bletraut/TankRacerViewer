using ComposableUi.Core;

using Microsoft.Xna.Framework;

using System;
using System.Collections.Generic;

namespace ComposableUi
{
    public sealed class CanvasElement : ContainerElement, IDrawableElement
    {
        private readonly GameWindow _gameWindow;

        public CanvasElement(GameWindow gameWindow,
            IReadOnlyList<Element> children = default)
            : base(children)
        {
            _gameWindow = gameWindow;
            _gameWindow.ClientSizeChanged += OnGameWindowClientSizeChanged;
        }

        public override Vector2 CalculatePreferredSize()
        {
            return new Vector2(_gameWindow.ClientBounds.Width, _gameWindow.ClientBounds.Height);
        }

        public override void AddChild(Element child)
        {
            Pivot = Vector2.Zero;
            base.AddChild(child);
        }

        void IDrawableElement.Draw(IUiRenderer renderer)
        {
            var rectangle = new Rectangle((Position - Size * Pivot).ToPoint(), Size.ToPoint());
            renderer.DrawRectangle(rectangle, ClipMask, new Color(Color.Blue, 0.2f));
        }

        private void OnGameWindowClientSizeChanged(object sender, EventArgs eventArgs)
            => OnStateChanged();
    }
}
