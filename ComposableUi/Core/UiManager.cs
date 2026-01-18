using ComposableUi.Core;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ComposableUi
{
    public sealed class UiManager
    {
        public ContainerElement Root { get; }

        public IPointerInputProvider PointerInputProvider { get; set; }
        public IUiRenderer UiRenderer { get; set; }

        private readonly Stack<Element> _stack = new();
        private readonly List<IPointerInputHandler> _pointerInputHandlers = [];
        private readonly List<IDrawableElement> _renderQueue = [];

        private bool _isRootDirty = true;

        public UiManager(ContentManager contentManager, SpriteBatch spriteBatch)
            : this(new DefaultPointerInputProvider(),
                  new DefaultUiRenderer(contentManager, spriteBatch)) { }

        public UiManager(IPointerInputProvider pointerInputProvider,
            IUiRenderer uiRenderer)
        {
            PointerInputProvider = pointerInputProvider;
            UiRenderer = uiRenderer;

            Root = new ContainerElement();
            Root.StateChanged += OnRootStateChanged;
        }

        public void Update(GameTime gameTime)
        {
            RebuildIfDirty();
        }

        public void Draw(GameTime gameTime)
        {
            if (UiRenderer == null)
                return;

            foreach (var element in _renderQueue)
                element.Draw(UiRenderer);
        }

        private void RebuildIfDirty()
        {
            if (!_isRootDirty)
                return;

            var size = Root.CalculatePreferredSize();
            Root.ApplySize(size);

            _pointerInputHandlers.Clear();
            _renderQueue.Clear();

            ForEachEnabledElement(element =>
            {
                if (element is IPointerInputHandler pointerInputHandler)
                    _pointerInputHandlers.Add(pointerInputHandler);

                if (element is IDrawableElement drawableElement)
                    _renderQueue.Add(drawableElement);
            });

            _isRootDirty = false;
        }

        private void ForEachEnabledElement(Action<Element> action)
        {
            _stack.Clear();

            _stack.Push(Root);
            while (_stack.Count > 0)
            {
                var element = _stack.Pop();
                if (!element.IsEnabled)
                    break;

                if (element is ParentElement parentElement)
                {
                    for (var i = parentElement.Children.Count - 1; i >= 0; i--)
                        _stack.Push(parentElement.Children[i]);
                }

                action?.Invoke(element);
            }
        }

        private void OnRootStateChanged(Element sender)
        {
            _isRootDirty = true;
        }
    }
}
