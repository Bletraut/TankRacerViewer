using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using System.Collections.Generic;

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

        private readonly HashSet<IPointerInputHandler> _activeHandlers = [];
        private readonly HashSet<IPointerInputHandler> _primaryButtonPressedHandlers = [];
        private readonly HashSet<IPointerInputHandler> _secondaryButtonPressedHandlers = [];

        private readonly List<IUpdateable> _updateableList = [];

        private Point _currentPointerPosition;
        private Point _lastPointerPosition;

        private bool _isRootDirty = true;

        public UiManager(GraphicsDevice graphicsDevice,
            ContentManager contentManager,
            SpriteBatch spriteBatch)
            : this(graphicsDevice,
                  new DefaultPointerInputProvider(),
                  new DefaultUiRenderer(contentManager, spriteBatch)) 
        {
            _updateableList.Add((IUpdateable)PointerInputProvider);
        }

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
            foreach (var updateable in _updateableList)
                updateable.Update(gameTime);

            HandlePointerInput();
            RebuildIfDirty();
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

            _lastPointerPosition = _currentPointerPosition;
            _currentPointerPosition = PointerInputProvider.PointerPosition;
            var pointerPositionDelta = _currentPointerPosition - _lastPointerPosition;

            var scrollWheelValue = PointerInputProvider.ScrollWheelValue;
            var horizontalScrollWheelValue = PointerInputProvider.HorizontalScrollWheelValue;

            var isInputBlocked = false;
            for (var i = _pointerInputHandlers.Count - 1; i >= 0; i--)
            {
                var (inputArea, handler) = _pointerInputHandlers[i];

                if (!isInputBlocked && inputArea.Contains(_currentPointerPosition))
                {
                    isInputBlocked |= handler.BlockInput;
                    if (!handler.IsInteractable)
                    {
                        _activeHandlers.Remove(handler);
                        _primaryButtonPressedHandlers.Remove(handler);
                        _secondaryButtonPressedHandlers.Remove(handler);

                        continue;
                    }

                    handler.OnScrollWheel(scrollWheelValue);
                    handler.OnHorizontalScrollWheel(horizontalScrollWheelValue);

                    if (_activeHandlers.Add(handler))
                        handler.OnPointerEnter(_currentPointerPosition);

                    handler.OnPointerMove(_currentPointerPosition);

                    if (PointerInputProvider.IsPrimaryButtonDown)
                    {
                        if (_primaryButtonPressedHandlers.Add(handler))
                            handler.OnPointerDown(_currentPointerPosition);
                    }
                    else if (PointerInputProvider.IsPrimaryButtonUp)
                    {
                        if (_primaryButtonPressedHandlers.Remove(handler))
                        {
                            handler.OnPointerUp(_currentPointerPosition);
                            handler.OnPointerClick(_currentPointerPosition);
                        }
                    }
                    if (PointerInputProvider.IsPrimaryButtonPressed)
                    {
                        if (_primaryButtonPressedHandlers.Contains(handler))
                            handler.OnPointerDrag(pointerPositionDelta);
                    }

                    if (PointerInputProvider.IsSecondaryButtonDown)
                    {
                        if (_secondaryButtonPressedHandlers.Add(handler))
                            handler.OnPointerSecondaryDown(_currentPointerPosition);
                    }
                    else if (PointerInputProvider.IsSecondaryButtonUp)
                    {
                        if (_secondaryButtonPressedHandlers.Remove(handler))
                        {
                            handler.OnPointerSecondaryUp(_currentPointerPosition);
                            handler.OnPointerSecondaryClick(_currentPointerPosition);
                        }
                    }

                    continue;
                }
                else if (_activeHandlers.Remove(handler))
                {
                    handler.OnPointerLeave(_currentPointerPosition);
                }

                if (PointerInputProvider.IsPrimaryButtonPressed)
                {
                    if (_primaryButtonPressedHandlers.Contains(handler))
                        handler.OnPointerDrag(pointerPositionDelta);
                }

                if (PointerInputProvider.IsPrimaryButtonUp)
                {
                    if (_primaryButtonPressedHandlers.Remove(handler))
                        handler.OnPointerUp(_currentPointerPosition);
                }

                if (PointerInputProvider.IsSecondaryButtonUp)
                {
                    if (_secondaryButtonPressedHandlers.Remove(handler))
                        handler.OnPointerSecondaryUp(_currentPointerPosition);
                }
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
                    for (var i = parentElement.ChildCount - 1; i >= 0; i--)
                        _stack.Push(parentElement.GetChildAt(i));
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
