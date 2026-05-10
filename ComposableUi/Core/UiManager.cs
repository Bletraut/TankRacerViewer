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

        private IPointerInputProvider _pointerInputProvider;
        public IPointerInputProvider PointerInputProvider
        {
            get => _pointerInputProvider;
            set => SetProvider(ref _pointerInputProvider, value);
        }

        private IKeyboardInputProvider _keyboardInputProvider;
        public IKeyboardInputProvider KeyboardInputProvider
        {
            get => _keyboardInputProvider;
            set => SetProvider(ref _keyboardInputProvider, value);
        }

        private ITextInputProvider _textInputProvider;
        public ITextInputProvider TextInputProvider
        {
            get => _textInputProvider;
            set => SetProvider(ref _textInputProvider, value);
        }

        private IClipboardProvider _clipboardProvider;
        public IClipboardProvider ClipboardProvider
        {
            get => _clipboardProvider;
            set => SetProvider(ref _clipboardProvider, value);
        }

        private IUiRenderer _uiRenderer;
        public IUiRenderer UiRenderer
        {
            get => _uiRenderer;
            set => SetProvider(ref _uiRenderer, value);
        }

        public bool HasAnyActiveInputHandlers => _currentActiveHandlers.Count > 0;
        public bool IsAnyElementPressed => _primaryButtonPressedHandlers.Count > 0 || _secondaryButtonPressedHandlers.Count > 0;

        private readonly GraphicsDevice _graphicsDevice;

        private readonly Stack<(uint Layer, Element Element)> _stack = new();
        private readonly Stack<(uint Layer, Element Element)> _nextStack = new();

        private readonly List<(Rectangle InputArea, IPointerInputHandler handler)> _pointerInputHandlers = [];
        private readonly List<IKeyboardInputHandler> _keyboardInputHandlers = [];
        private readonly List<ITextInputHandler> _textInputHandlers = [];
        private readonly List<IClipboardHandler> _clipboardHandlers = [];
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

        public UiManager(GraphicsDevice graphicsDevice,
            ContentManager contentManager,
            GameWindow gameWindow,
            SpriteBatch spriteBatch)
            : this(graphicsDevice,
                  contentManager,
                  new DefaultPointerInputProvider(),
                  new DefaultKeyboardInputProvider(),
                  new DefaultTextInputProvider(gameWindow),
                  new DefaultClipboardProvider(),
                  new DefaultUiRenderer(contentManager, spriteBatch))
        {
        }

        public UiManager(GraphicsDevice graphicsDevice,
            ContentManager contentManager,
            IPointerInputProvider pointerInputProvider,
            IKeyboardInputProvider keyboardInputProvider,
            ITextInputProvider textInputProvider,
            IClipboardProvider clipboardProvider,
            IUiRenderer uiRenderer)
        {
            _graphicsDevice = graphicsDevice;

            PointerInputProvider = pointerInputProvider;
            KeyboardInputProvider = keyboardInputProvider;
            TextInputProvider = textInputProvider;
            ClipboardProvider = clipboardProvider;
            UiRenderer = uiRenderer;

            Root = new RootElement
            {
                Pivot = Alignment.TopLeft
            };
            Root.ApplyRoot(Root);

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
            HandleKeyboardInput();
            HandleTextInput();
            HandleClipboard();

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
                            if (!_lastFocusedHandlers.Contains(handler))
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
                foreach (var handler in _secondaryButtonPressedHandlers)
                    handler.OnPointerSecondaryUp(pointerEvent);

                _secondaryButtonPressedHandlers.Clear();
            }

            foreach (var handler in _lastActiveHandlers)
            {
                if (!_currentActiveHandlers.Contains(handler))
                    handler.OnPointerLeave(pointerEvent);
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

        private void HandleKeyboardInput()
        {
            if (KeyboardInputProvider is null)
                return;

            foreach (var handler in _keyboardInputHandlers)
                handler.Handle(KeyboardInputProvider);
        }

        private void HandleTextInput()
        {
            if (TextInputProvider is null)
                return;

            if (!TextInputProvider.HasText)
                return;

            var text = TextInputProvider.Text;
            foreach (var handler in _textInputHandlers)
                handler.OnTextInput(text);
        }

        private void HandleClipboard()
        {
            if (ClipboardProvider is null)
                return;

            foreach (var handler in _clipboardHandlers)
                handler.Handle(ClipboardProvider);
        }

        private void RebuildIfDirty()
        {
            if (!Root.IsDirty)
                return;

            var size = Root.CalculatePreferredSize();
            Root.Rebuild(size);

            RefreshVisibleElementLists();
        }

        private void RefreshVisibleElementLists()
        {
            _pointerInputHandlers.Clear();
            _keyboardInputHandlers.Clear();
            _textInputHandlers.Clear();
            _clipboardHandlers.Clear();
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

                    if (element is PointerInputHandlerElement pointerInputHandler)
                    {
                        var isPressed = _primaryButtonPressedHandlers.Contains(pointerInputHandler)
                            || _secondaryButtonPressedHandlers.Contains(pointerInputHandler);
                        if (isPressed)
                        {
                            HandleElement(element);
                            continue;
                        }
                    }

                    var boundingRectangle = element.BoundingRectangle;
                    var clipMask = element.ClipMask;

                    if (clipMask.HasValue)
                    {
                        var isClipped = clipMask.Value.Width <= 0
                            && clipMask.Value.Height <= 0;
                        if (isClipped)
                            continue;

                        if (!clipMask.Value.Intersects(boundingRectangle))
                            continue;
                    }

                    if (!viewportBounds.Intersects(boundingRectangle))
                        continue;

                    HandleElement(element);
                }

                currentLayer = nextMinLayer;
                nextMinLayer = uint.MaxValue;

                while (_nextStack.Count > 0)
                    _stack.Push(_nextStack.Pop());
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

            if (element is IKeyboardInputHandler keyboardInputHandler)
                _keyboardInputHandlers.Add(keyboardInputHandler);

            if (element is ITextInputHandler textInputHandler)
                _textInputHandlers.Add(textInputHandler);

            if (element is IClipboardHandler clipboardHandler)
                _clipboardHandlers.Add(clipboardHandler);

            if (element is IDrawableElement drawableElement)
                _renderQueue.Add(drawableElement);
        }

        private bool SetProvider<T>(ref T provider, T value)
        {
            if (EqualityComparer<T>.Default.Equals(provider, value))
                return false;

            if (provider is IUpdateable oldUpdateable)
                _updateableList.Remove(oldUpdateable);

            if (provider is IProviderLifecycle oldProviderLifecycle)
                oldProviderLifecycle.OnRemoved();

            provider = value;
            if (provider is IUpdateable newUpdateable)
                _updateableList.Add(newUpdateable);

            if (provider is IProviderLifecycle newProviderLifecycle)
                newProviderLifecycle.OnAdded();

            return true;
        }
    }
}
