using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ComposableUi
{
    public sealed class UiManager
    {
        public RootElement Root { get; }

        public IPointerInputProvider PointerInputProvider { get; set; }
        public IUiRenderer UiRenderer { get; set; }

        public bool HasAnyActiveInputHandlers => _currentActiveHandlers.Count > 0;
        public bool IsAnyElementPressed => _primaryButtonPressedHandlers.Count > 0 || _secondaryButtonPressedHandlers.Count > 0;

        private readonly GraphicsDevice _graphicsDevice;

        private Stack<(uint Layer, Element Element)> _stack = new();
        private Stack<(uint Layer, Element Element)> _nextStack = new();

        private readonly List<IPointerInputHandler> _skippedPointerInputHandlers = [];
        private readonly List<(Rectangle InputArea, IPointerInputHandler handler)> _pointerInputHandlers = [];
        private readonly List<IDrawableElement> _renderQueue = [];

        private HashSet<IPointerInputHandler> _lastActiveHandlers = [];
        private HashSet<IPointerInputHandler> _currentActiveHandlers = [];
        private HashSet<IPointerInputHandler> _lastFocusedHandlers = [];
        private HashSet<IPointerInputHandler> _currentFocusedHandlers = [];
        private readonly HashSet<IPointerInputHandler> _primaryButtonPressedHandlers = [];
        private readonly HashSet<IPointerInputHandler> _secondaryButtonPressedHandlers = [];

        private readonly List<IUpdateable> _updateableList = [];

        private readonly List<IElementSolver> _elementSolvers = [];

        private Point _currentPointerPosition;
        private Point _lastPointerPosition;

        private bool _isRootDirty = true;

        public UiManager(GraphicsDevice graphicsDevice,
            ContentManager contentManager,
            SpriteBatch spriteBatch)
            : this(graphicsDevice,
                  contentManager,
                  new DefaultPointerInputProvider(),
                  new DefaultUiRenderer(contentManager, spriteBatch)) 
        {
            _updateableList.Add((IUpdateable)PointerInputProvider);
        }

        public UiManager(GraphicsDevice graphicsDevice,
            ContentManager contentManager,
            IPointerInputProvider pointerInputProvider,
            IUiRenderer uiRenderer)
        {
            _graphicsDevice = graphicsDevice;

            PointerInputProvider = pointerInputProvider;
            UiRenderer = uiRenderer;

            Root = new RootElement();
            Root.ApplyRoot(Root);
            Root.StateChanged += OnRootStateChanged;

            TextElement.DefaultSpriteFont = contentManager.Load<SpriteFont>("ComposableUi\\MainFont");

            AddElementSolver(new HierarchyWheelScrollSolver());
            AddElementSolver(new ComposableWindowsSolver());
        }

        public void AddElementSolver(IElementSolver elementSolver)
        {
            if (_elementSolvers.Contains(elementSolver))
                return;

            _elementSolvers.Add(elementSolver);
        }

        public void RemoveElementSolver(IElementSolver elementSolver)
        {
            _elementSolvers.Remove(elementSolver);
        }

        public void Update(GameTime gameTime)
        {
            foreach (var updateable in _updateableList)
                updateable.Update(gameTime);

            HandlePointerInput();
            RebuildIfDirty();

            foreach (var elementSolver in _elementSolvers)
                elementSolver.Resolve();
        }

        public void Draw(GameTime gameTime)
        {
            if (UiRenderer is null)
                return;

            UiRenderer.Begin();

            foreach (var element in _renderQueue)
                element.Draw(UiRenderer);

            UiRenderer.End();
        }

        private void HandlePointerInput()
        {
            if (PointerInputProvider is null)
                return;

            (_lastActiveHandlers, _currentActiveHandlers) = (_currentActiveHandlers, _lastActiveHandlers);
            _currentActiveHandlers.Clear();

            _lastPointerPosition = _currentPointerPosition;
            _currentPointerPosition = PointerInputProvider.PointerPosition;

            var isPrimaryButtonPressed = PointerInputProvider.IsPrimaryButtonPressed;
            var isSecondaryButtonPressed = PointerInputProvider.IsSecondaryButtonPressed;

            var isAnyButtonDown = PointerInputProvider.IsPrimaryButtonDown 
                || PointerInputProvider.IsSecondaryButtonDown;
            if (isAnyButtonDown)
            {
                (_lastFocusedHandlers, _currentFocusedHandlers) = (_currentFocusedHandlers, _lastFocusedHandlers);
                _currentFocusedHandlers.Clear();
            }

            var pointer = PointerInputProvider.Pointer;
            var pointerEvent = new PointerEvent(pointer, _currentPointerPosition,
                isPrimaryButtonPressed, isSecondaryButtonPressed);

            var scrollWheelValueDelta = PointerInputProvider.ScrollWheelValueDelta;
            var pointerScrollEvent = new PointerScrollEvent(pointer, _currentPointerPosition,
                isPrimaryButtonPressed, isSecondaryButtonPressed, scrollWheelValueDelta);

            var horizontalScrollWheelValueDelta = PointerInputProvider.HorizontalScrollWheelValueDelta;
            var pointerHorizontalScrollEvent = new PointerScrollEvent(pointer, _currentPointerPosition,
                isPrimaryButtonPressed, isSecondaryButtonPressed, horizontalScrollWheelValueDelta);

            var pointerPositionDelta = _currentPointerPosition - _lastPointerPosition;
            var pointerDragEvent = new PointerDragEvent(pointer, _currentPointerPosition,
                isPrimaryButtonPressed, isSecondaryButtonPressed, pointerPositionDelta);

            var pointerFocusedEvent = new PointerFocusEvent(pointer, _currentPointerPosition,
                isPrimaryButtonPressed, isSecondaryButtonPressed, true);
            var pointerUnfocusedEvent = new PointerFocusEvent(pointer, _currentPointerPosition,
                isPrimaryButtonPressed, isSecondaryButtonPressed, false);

            foreach (var handler in _skippedPointerInputHandlers)
            {
                if (_lastActiveHandlers.Remove(handler))
                    handler.OnPointerLeave(pointerEvent);
            }
            _skippedPointerInputHandlers.Clear();

            var isInputBlocked = false;
            for (var i = _pointerInputHandlers.Count - 1; i >= 0; i--)
            {
                var (inputArea, handler) = _pointerInputHandlers[i];

                if (!isInputBlocked && inputArea.Contains(_currentPointerPosition))
                {
                    isInputBlocked |= handler.BlockInput;
                    if (!handler.IsInteractable)
                    {
                        _currentActiveHandlers.Add(handler);
                        _lastFocusedHandlers.Remove(handler);
                        _primaryButtonPressedHandlers.Remove(handler);
                        _secondaryButtonPressedHandlers.Remove(handler);

                        continue;
                    }

                    if (scrollWheelValueDelta != 0)
                        handler.OnScrollWheel(pointerScrollEvent);
                    if (horizontalScrollWheelValueDelta != 0)
                        handler.OnHorizontalScrollWheel(pointerHorizontalScrollEvent);

                    var isPointerEnter = _currentActiveHandlers.Add(handler)
                        && !_lastActiveHandlers.Contains(handler);
                    if (isPointerEnter)
                        handler.OnPointerEnter(pointerEvent);

                    handler.OnPointerMove(pointerEvent);

                    if (PointerInputProvider.IsPrimaryButtonDown)
                    {
                        if (_currentFocusedHandlers.Add(handler))
                        {
                            if(!_lastFocusedHandlers.Contains(handler))
                                handler.OnFocusChanged(pointerFocusedEvent);
                        }

                        if (_primaryButtonPressedHandlers.Add(handler))
                            handler.OnPointerDown(pointerEvent);
                    }
                    else if (PointerInputProvider.IsPrimaryButtonUp)
                    {
                        if (_primaryButtonPressedHandlers.Remove(handler))
                        {
                            handler.OnPointerUp(pointerEvent);
                            handler.OnPointerClick(pointerEvent);
                        }
                    }
                    if (isPrimaryButtonPressed)
                    {
                        if (_primaryButtonPressedHandlers.Contains(handler))
                            handler.OnPointerDrag(pointerDragEvent);
                    }

                    if (PointerInputProvider.IsSecondaryButtonDown)
                    {
                        if (_currentFocusedHandlers.Add(handler))
                        {
                            if (!_lastFocusedHandlers.Contains(handler))
                                handler.OnFocusChanged(pointerFocusedEvent);
                        }

                        if (_secondaryButtonPressedHandlers.Add(handler))
                            handler.OnPointerSecondaryDown(pointerEvent);
                    }
                    else if (PointerInputProvider.IsSecondaryButtonUp)
                    {
                        if (_secondaryButtonPressedHandlers.Remove(handler))
                        {
                            handler.OnPointerSecondaryUp(pointerEvent);
                            handler.OnPointerSecondaryClick(pointerEvent);
                        }
                    }

                    continue;
                }
                else if (_lastActiveHandlers.Remove(handler))
                {
                    handler.OnPointerLeave(pointerEvent);
                }

                if (isPrimaryButtonPressed)
                {
                    if (_primaryButtonPressedHandlers.Contains(handler))
                        handler.OnPointerDrag(pointerDragEvent);
                }
            }

            if (PointerInputProvider.IsPrimaryButtonUp)
            {
                foreach (var handler in _primaryButtonPressedHandlers)
                    handler.OnPointerUp(pointerEvent);

                _primaryButtonPressedHandlers.Clear();
            }

            if (PointerInputProvider.IsSecondaryButtonUp)
            {
                foreach(var handler in _secondaryButtonPressedHandlers)
                    handler.OnPointerSecondaryUp(pointerEvent);

                _secondaryButtonPressedHandlers.Clear();
            }

            if (isAnyButtonDown)
            {
                foreach (var handler in _lastFocusedHandlers)
                {
                    if (!_currentFocusedHandlers.Contains(handler))
                        handler.OnFocusChanged(pointerUnfocusedEvent);
                }
            }
        }

        private void RebuildIfDirty()
        {
            if (!_isRootDirty)
                return;

            var size = Root.CalculatePreferredSize();
            Root.Rebuild(size);

            RefreshVisibleElementLists();

            _isRootDirty = false;
        }

        private void RefreshVisibleElementLists()
        {
            _pointerInputHandlers.Clear();
            _renderQueue.Clear();

            _stack.Clear();
            _nextStack.Clear();
            _stack.Push((0, Root));

            uint currentLayer = 0;
            uint nextMinLayer = uint.MaxValue;

            var viewportBounds = _graphicsDevice.Viewport.Bounds;

            do
            {
                while (_stack.Count > 0)
                {
                    var (parentLayer, element) = _stack.Pop();
                    if (!element.IsEnabled)
                        continue;

                    var layer = parentLayer + element.Layer;
                    if (layer > currentLayer)
                    {
                        nextMinLayer = Math.Min(nextMinLayer, layer);
                        _nextStack.Push((parentLayer, element));

                        continue;
                    }

                    if (element is ParentElement parentElement)
                    {
                        for (var i = parentElement.ChildCount - 1; i >= 0; i--)
                            _stack.Push((layer, parentElement.GetChildAt(i)));
                    }

                    var shouldSkip = false;

                    var boundingRectangle = element.BoundingRectangle;
                    var clipMask = element.ClipMask;

                    if (clipMask.HasValue)
                    {
                        var isClipped = clipMask.Value.Width <= 0
                            && clipMask.Value.Height <= 0;

                        shouldSkip = isClipped
                            || !clipMask.Value.Intersects(boundingRectangle);
                    }

                    shouldSkip = shouldSkip 
                        || !viewportBounds.Intersects(boundingRectangle);

                    if (shouldSkip)
                    {
                        HandleSkippedElement(element);
                    }
                    else
                    {
                        HandleElement(element);
                    }
                }

                currentLayer = nextMinLayer;
                nextMinLayer = uint.MaxValue;
                (_stack, _nextStack) = (_nextStack, _stack);
            } 
            while (_stack.Count > 0);
        }

        private void HandleElement(Element element)
        {
            foreach (var elementSolver in _elementSolvers)
                elementSolver.Handle(element);

            if (element is IPointerInputHandler pointerInputHandler)
            {
                var inputArea = pointerInputHandler.ClippedInteractionRectangle;
                _pointerInputHandlers.Add((inputArea, pointerInputHandler));
            }

            if (element is IDrawableElement drawableElement)
                _renderQueue.Add(drawableElement);
        }

        private void HandleSkippedElement(Element element)
        {
            if (element is IPointerInputHandler pointerInputHandler)
            {
                _skippedPointerInputHandlers.Add(pointerInputHandler);
            }
        }

        private void OnRootStateChanged(Element sender)
        {
            _isRootDirty = true;
        }
    }
}
