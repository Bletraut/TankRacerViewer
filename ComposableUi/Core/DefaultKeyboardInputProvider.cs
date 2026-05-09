using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace ComposableUi
{
    public sealed class DefaultKeyboardInputProvider : IKeyboardInputProvider, IUpdateable
    {
        private const long InitialRepeatDelayMs = 350;
        private const long RepeatIntervalMs = 20;

        private static readonly Keys[] NavigationKeys =
        [
            Keys.Back, Keys.Delete, Keys.Left, Keys.Right, Keys.Up, Keys.Down, Keys.Home, Keys.End
        ];

        private readonly List<char> _typedCharacters = [];
        private readonly List<char> _pendingCharacters = [];

        private bool _pendingCopy, _pendingPaste, _pendingCut, _pendingSelectAll;
        private bool _isCopyDown, _isPasteDown, _isCutDown, _isSelectAllDown;

        private KeyboardState _current;
        private KeyboardState _last;

        private readonly Dictionary<Keys, long> _repeatStartTimes = [];
        private readonly HashSet<Keys> _repeatingThisFrame = [];

        IReadOnlyList<char> IKeyboardInputProvider.TypedCharacters => _typedCharacters;

        bool IKeyboardInputProvider.IsShiftPressed
            => _current.IsKeyDown(Keys.LeftShift) || _current.IsKeyDown(Keys.RightShift);

        bool IKeyboardInputProvider.IsCtrlPressed
            => _current.IsKeyDown(Keys.LeftControl) || _current.IsKeyDown(Keys.RightControl);

        bool IKeyboardInputProvider.IsBackspaceDown => IsDownOrRepeating(Keys.Back);
        bool IKeyboardInputProvider.IsDeleteDown => IsDownOrRepeating(Keys.Delete);
        bool IKeyboardInputProvider.IsLeftArrowDown => IsDownOrRepeating(Keys.Left);
        bool IKeyboardInputProvider.IsRightArrowDown => IsDownOrRepeating(Keys.Right);
        bool IKeyboardInputProvider.IsUpArrowDown => IsDownOrRepeating(Keys.Up);
        bool IKeyboardInputProvider.IsDownArrowDown => IsDownOrRepeating(Keys.Down);
        bool IKeyboardInputProvider.IsHomeDown => IsDownOrRepeating(Keys.Home);
        bool IKeyboardInputProvider.IsEndDown => IsDownOrRepeating(Keys.End);

        bool IKeyboardInputProvider.IsReturnDown
            => _last.IsKeyUp(Keys.Enter) && _current.IsKeyDown(Keys.Enter);

        bool IKeyboardInputProvider.IsSelectAllDown => _isSelectAllDown;
        bool IKeyboardInputProvider.IsCopyDown => _isCopyDown;
        bool IKeyboardInputProvider.IsPasteDown => _isPasteDown;
        bool IKeyboardInputProvider.IsCutDown => _isCutDown;

        public DefaultKeyboardInputProvider(GameWindow window)
        {
            window.TextInput += OnTextInput;
        }

        private void OnTextInput(object sender, TextInputEventArgs e)
        {
            switch (e.Character)
            {
                case '\x01': _pendingSelectAll = true; break;
                case '\x03': _pendingCopy = true; break;
                case '\x16': _pendingPaste = true; break;
                case '\x18': _pendingCut = true; break;
                default:
                    if (!char.IsControl(e.Character))
                        _pendingCharacters.Add(e.Character);
                    break;
            }
        }

        void IUpdateable.Update(GameTime gameTime)
        {
            _last = _current;
            _current = Keyboard.GetState();

            _isCopyDown = _pendingCopy || IsCtrlAndJustPressed(Keys.C);
            _isPasteDown = _pendingPaste || IsCtrlAndJustPressed(Keys.V);
            _isCutDown = _pendingCut || IsCtrlAndJustPressed(Keys.X);
            _isSelectAllDown = _pendingSelectAll || IsCtrlAndJustPressed(Keys.A);
            _pendingCopy = _pendingPaste = _pendingCut = _pendingSelectAll = false;

            _typedCharacters.Clear();
            _typedCharacters.AddRange(_pendingCharacters);
            _pendingCharacters.Clear();

            _repeatingThisFrame.Clear();
            var now = Environment.TickCount64;

            foreach (var key in NavigationKeys)
            {
                if (_current.IsKeyDown(key))
                {
                    if (_last.IsKeyUp(key))
                    {
                        _repeatStartTimes[key] = now;
                    }
                    else if (_repeatStartTimes.TryGetValue(key, out var startTime))
                    {
                        var elapsed = now - startTime;
                        if (elapsed >= InitialRepeatDelayMs)
                        {
                            var repeatsNow = (elapsed - InitialRepeatDelayMs) / RepeatIntervalMs;
                            var repeatsBefore = (elapsed - 1 - InitialRepeatDelayMs) / RepeatIntervalMs;
                            if (repeatsNow > repeatsBefore)
                                _repeatingThisFrame.Add(key);
                        }
                    }
                }
                else
                {
                    _repeatStartTimes.Remove(key);
                }
            }
        }

        private bool IsDownOrRepeating(Keys key)
            => (_last.IsKeyUp(key) && _current.IsKeyDown(key))
               || _repeatingThisFrame.Contains(key);

        private bool IsCtrlAndJustPressed(Keys key)
            => (_current.IsKeyDown(Keys.LeftControl) || _current.IsKeyDown(Keys.RightControl))
               && _last.IsKeyUp(key) && _current.IsKeyDown(key);
    }
}
