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

        private readonly GraphicsDevice _graphicsDevice;

        private readonly Stack<Element> _stack = new();
        private readonly List<(Rectangle InputArea, IPointerInputHandler handler)> _pointerInputHandlers = [];
        private readonly List<IDrawableElement> _renderQueue = [];

        private bool _isRootDirty = true;

        public UiManager(GraphicsDevice graphicsDevice,
            ContentManager contentManager,
            SpriteBatch spriteBatch)
            : this(graphicsDevice,
                  new DefaultPointerInputProvider(),
                  new DefaultUiRenderer(contentManager, spriteBatch)) { }

        public UiManager(GraphicsDevice graphicsDevice,
            IPointerInputProvider pointerInputProvider,
            IUiRenderer uiRenderer)
        {
            _graphicsDevice = graphicsDevice;

            PointerInputProvider = pointerInputProvider;
            UiRenderer = uiRenderer;

            Root = new ContainerElement();
            Root.StateChanged += OnRootStateChanged;
        }

        public void Update(GameTime gameTime)
        {
            RebuildIfDirty();
            HandlePointerInput();
        }

        public void Draw(GameTime gameTime)
        {
            if (UiRenderer == null)
                return;

            foreach (var element in _renderQueue)
                element.Draw(UiRenderer);
        }

        private void HandlePointerInput()
        {
            if (PointerInputProvider == null)
                return;

            var pointerPosition = PointerInputProvider.PointerPosition;

            for (var i = _pointerInputHandlers.Count - 1; i >= 0; i--)
            {
                var (inputArea, handler) = _pointerInputHandlers[i];

                if (inputArea.Contains(pointerPosition))
                    handler.OnPointerOver(pointerPosition);

                if (handler.BlockInput)
                    break;
            }
        }

        private void RebuildIfDirty()
        {
            if (!_isRootDirty)
                return;

            var size = Root.CalculatePreferredSize();
            Root.ApplySize(size);

            RefreshVisibleElementLists();

            _isRootDirty = false;
        }

        private void RefreshVisibleElementLists()
        {
            _pointerInputHandlers.Clear();
            _renderQueue.Clear();

            _stack.Clear();
            _stack.Push(Root);

            var viewportBounds = _graphicsDevice.Viewport.Bounds;

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

                var clipMask = element.ClipMask;
                var isClipped = clipMask.HasValue
                    && clipMask.Value.Width <= 0 && clipMask.Value.Height <= 0;
                if (isClipped)
                    continue;

                if (!viewportBounds.Intersects(element.BoundingRectangle))
                    continue;

                HandleElement(element);
            }
        }

        private void HandleElement(Element element)
        {
            if (element is IPointerInputHandler pointerInputHandler)
            {
                var inputArea = element.BoundingRectangle;

                var clipMask = element.ClipMask;
                if (clipMask.HasValue)
                    inputArea = Rectangle.Intersect(inputArea, clipMask.Value);

                _pointerInputHandlers.Add((inputArea, pointerInputHandler));
            }

            if (element is IDrawableElement drawableElement)
                _renderQueue.Add(drawableElement);
        }

        private void OnRootStateChanged(Element sender)
        {
            _isRootDirty = true;
        }
    }
}
