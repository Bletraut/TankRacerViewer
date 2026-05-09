using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ComposableUi
{
    public class TextInputFieldElement : PointerInputHandlerElement<TextInputFieldElement>,
        IDrawableElement, IKeyboardInputHandler
    {
        public static readonly Vector2 DefaultSize = new(200, 30);

        private const int TextPaddingX = 6;
        private const int TextPaddingY = 4;
        private const int CaretWidth = 1;
        private const long CaretBlinkIntervalMs = 530;
        private const float LineSpacingExtra = 2f;
        private const int ScrollbarWidth = 10;
        private const int ScrollbarMinThumbHeight = 20;
        private const long DoubleClickMaxIntervalMs = 400;
        private const int DoubleClickMaxDistancePx = 5;
        private const int ClearButtonWidth = 20;

        // ── Text ──────────────────────────────────────────────────────────

        private string _text = string.Empty;
        public string Text
        {
            get => _text;
            set
            {
                var incoming = value ?? string.Empty;
                if (MaxLength > 0 && incoming.Length > MaxLength)
                    incoming = incoming[..MaxLength];
                if (_text == incoming) return;
                _text = incoming;
                ClampCaretToText();
                _scrollOffset = 0;
                _scrollOffsetY = 0;
                _preferredColumn = -1;
                OnStateChanged();
                TextChanged?.Invoke(this, _text);
            }
        }

        private string _placeholderText = string.Empty;
        public string PlaceholderText
        {
            get => _placeholderText;
            set => SetAndChangeState(ref _placeholderText, value ?? string.Empty);
        }

        private int _maxLength;
        public int MaxLength
        {
            get => _maxLength;
            set => _maxLength = Math.Max(0, value);
        }

        private bool _isMultiLine;
        public bool IsMultiLine
        {
            get => _isMultiLine;
            set
            {
                if (_isMultiLine == value) return;
                _isMultiLine = value;
                _scrollOffset = 0;
                _scrollOffsetY = 0;
                OnStateChanged();
            }
        }

        // ── Font ──────────────────────────────────────────────────────────

        private SpriteFont _spriteFont;
        public SpriteFont SpriteFont
        {
            get => _spriteFont ?? TextElement.DefaultSpriteFont;
            set => _spriteFont = value;
        }

        // ── Skins ─────────────────────────────────────────────────────────

        public StandardSkin NormalSkin { get; set; } = StandardSkin.TextField;
        public StandardSkin FocusedSkin { get; set; } = StandardSkin.TextField;
        public StandardSkin DisabledSkin { get; set; } = StandardSkin.TextField;

        // ── Colors ────────────────────────────────────────────────────────

        public Color NormalColor { get; set; } = Color.White;
        public Color FocusedColor { get; set; } = new Color(180, 210, 255);
        public Color DisabledColor { get; set; } = new Color(128, 128, 128);

        public Color TextColor { get; set; } = new Color(20, 20, 20);
        public Color PlaceholderColor { get; set; } = new Color(150, 150, 150);
        public Color SelectionColor { get; set; } = new Color(0, 120, 215, 160);
        public Color CaretColor { get; set; } = new Color(20, 20, 20);

        // ── Events ────────────────────────────────────────────────────────

        public event ElementEventHandler<TextInputFieldElement, string> TextChanged;
        public event ElementEventHandler<TextInputFieldElement> ReturnPressed;

        // ── Clear button ──────────────────────────────────────────────────

        private bool _hasClearButton;
        public bool HasClearButton
        {
            get => _hasClearButton;
            set
            {
                if (_hasClearButton == value) return;
                _hasClearButton = value;
                OnStateChanged();
            }
        }

        // ── Internal caret / selection / scroll state ─────────────────────

        private int _caretPosition;
        private int _selectionAnchor;
        private float _scrollOffset;      // horizontal scroll for single-line mode
        private float _scrollOffsetY;     // vertical scroll for multi-line mode
        private long _caretBlinkResetTime;
        private int _preferredColumn = -1;

        private bool _isDraggingScrollbar;
        private float _scrollbarDragStartY;
        private float _scrollbarDragStartOffset;

        private long _lastClickTime;
        private Point _lastClickPosition;
        private bool _wordSelectionMode;
        private int _wordSelectionAnchorStart;
        private int _wordSelectionAnchorEnd;

        public int CaretPosition => _caretPosition;
        public bool HasSelection => _caretPosition != _selectionAnchor;
        private int SelectionMin => Math.Min(_caretPosition, _selectionAnchor);
        private int SelectionMax => Math.Max(_caretPosition, _selectionAnchor);

        // ── Visual line ───────────────────────────────────────────────────

        // A visual line covers _text[Start .. Start+Length].
        // SkipAfter: 1 when the char at Start+Length is a \n or a wrap-triggering space (skipped);
        //            0 for mid-word wrap or the final line.
        // NextStart = Start + Length + SkipAfter
        private readonly struct VisualLine
        {
            public readonly int Start;
            public readonly int Length;
            public readonly int SkipAfter;
            public int NextStart => Start + Length + SkipAfter;

            public VisualLine(int start, int length, int skipAfter)
            { Start = start; Length = length; SkipAfter = skipAfter; }
        }

        // ── Constructor ───────────────────────────────────────────────────

        public TextInputFieldElement(
            Vector2? size = default,
            string text = default,
            string placeholderText = default,
            SpriteFont spriteFont = default,
            StandardSkin normalSkin = StandardSkin.TextField,
            StandardSkin focusedSkin = StandardSkin.TextField,
            StandardSkin disabledSkin = StandardSkin.TextField,
            Color? normalColor = default,
            Color? focusedColor = default,
            Color? disabledColor = default,
            Color? textColor = default,
            Color? placeholderColor = default,
            Color? selectionColor = default,
            Color? caretColor = default,
            bool isInteractable = true,
            bool isMultiLine = false,
            bool hasClearButton = false)
            : base(isInteractable: isInteractable)
        {
            Size = size ?? DefaultSize;
            _text = text ?? string.Empty;
            _placeholderText = placeholderText ?? string.Empty;
            _spriteFont = spriteFont;
            _isMultiLine = isMultiLine;
            _hasClearButton = hasClearButton;

            NormalSkin = normalSkin;
            FocusedSkin = focusedSkin;
            DisabledSkin = disabledSkin;

            NormalColor = normalColor ?? Color.White;
            FocusedColor = focusedColor ?? Color.White;
            DisabledColor = disabledColor ?? new Color(128, 128, 128);

            TextColor = textColor ?? new Color(20, 20, 20);
            PlaceholderColor = placeholderColor ?? new Color(150, 150, 150);
            SelectionColor = selectionColor ?? new Color(0, 120, 215, 160);
            CaretColor = caretColor ?? new Color(20, 20, 20);

            ResetCaretBlink();
        }

        // ── Text area geometry ────────────────────────────────────────────

        // In multi-line mode the scrollbar occupies the right edge.
        private float GetTextAreaWidth()
            => Size.X - 2 * TextPaddingX
               - (_isMultiLine ? ScrollbarWidth : (_hasClearButton ? ClearButtonWidth : 0));

        // ── Caret helpers ─────────────────────────────────────────────────

        private void ResetCaretBlink()
            => _caretBlinkResetTime = Environment.TickCount64;

        private bool IsCaretVisible()
        {
            var elapsed = Environment.TickCount64 - _caretBlinkResetTime;
            return elapsed % (2 * CaretBlinkIntervalMs) < CaretBlinkIntervalMs;
        }

        private void ClampCaretToText()
        {
            _caretPosition = Math.Clamp(_caretPosition, 0, _text.Length);
            _selectionAnchor = Math.Clamp(_selectionAnchor, 0, _text.Length);
        }

        private void SetCaretPosition(int pos, bool extendSelection = false, bool preservePreferredColumn = false)
        {
            _caretPosition = Math.Clamp(pos, 0, _text.Length);
            if (!extendSelection)
                _selectionAnchor = _caretPosition;
            if (!preservePreferredColumn)
                _preferredColumn = -1;
            UpdateScrollOffset();
            ResetCaretBlink();
        }

        private void UpdateScrollOffset()
        {
            var font = SpriteFont;
            if (font is null) return;

            if (_isMultiLine)
            {
                _scrollOffset = 0;
                var lines = ComputeVisualLines(font, GetTextAreaWidth());
                var (caretLine, _) = GetVisualLineCol(_caretPosition, lines);

                var fontHeight = font.MeasureString("Ag").Y;
                var lineSpacing = fontHeight + LineSpacingExtra;
                var textAreaHeight = Size.Y - 2 * TextPaddingY;
                var caretTop = caretLine * lineSpacing;
                var caretBottom = caretTop + lineSpacing;

                if (caretTop - _scrollOffsetY < 0)
                    _scrollOffsetY = caretTop;
                else if (caretBottom - _scrollOffsetY > textAreaHeight)
                    _scrollOffsetY = caretBottom - textAreaHeight;
                _scrollOffsetY = Math.Max(0, _scrollOffsetY);
            }
            else
            {
                var textAreaWidth = GetTextAreaWidth();
                var caretX = MeasureWidth(font, _text, 0, _caretPosition);
                if (caretX - _scrollOffset < 0)
                    _scrollOffset = caretX;
                else if (caretX - _scrollOffset > textAreaWidth - CaretWidth)
                    _scrollOffset = caretX - textAreaWidth + CaretWidth;
                _scrollOffset = Math.Max(0, _scrollOffset);
            }
        }

        // ── Visual line computation with word wrap ────────────────────────

        // Computes visual lines with word wrapping to fit textAreaWidth.
        // Each logical line (separated by \n) is broken into one or more visual lines.
        // Soft-wrap break at a space: space is the SkipAfter char (not rendered, not on next line).
        // Soft-wrap break mid-word: SkipAfter=0, next visual line starts immediately after.
        private List<VisualLine> ComputeVisualLines(SpriteFont font, float textAreaWidth)
        {
            var lines = new List<VisualLine>();

            if (_text.Length == 0)
            {
                lines.Add(new VisualLine(0, 0, 0));
                return lines;
            }

            int pos = 0;
            while (pos <= _text.Length)
            {
                int newlineIdx = _text.IndexOf('\n', pos);
                int hardEnd = newlineIdx >= 0 ? newlineIdx : _text.Length;
                int hardSkip = newlineIdx >= 0 ? 1 : 0;

                int segStart = pos;
                bool segDone = false;

                while (!segDone && segStart <= hardEnd)
                {
                    float width = 0;
                    int lastSpaceIdx = -1;
                    bool wrapped = false;

                    for (int i = segStart; i < hardEnd; i++)
                    {
                        float charW = font.MeasureString(_text[i].ToString()).X;

                        // Only wrap when at least one char fits (i > segStart prevents infinite loop)
                        if (i > segStart && width + charW > textAreaWidth)
                        {
                            if (lastSpaceIdx > segStart)
                            {
                                // Wrap at the space: exclude it from this line, skip it for the next
                                lines.Add(new VisualLine(segStart, lastSpaceIdx - segStart, 1));
                                segStart = lastSpaceIdx + 1;
                            }
                            else
                            {
                                // No usable space found — hard break mid-word
                                lines.Add(new VisualLine(segStart, i - segStart, 0));
                                segStart = i;
                            }
                            wrapped = true;
                            break;
                        }

                        width += charW;
                        if (_text[i] == ' ')
                            lastSpaceIdx = i;
                    }

                    if (!wrapped)
                    {
                        lines.Add(new VisualLine(segStart, hardEnd - segStart, hardSkip));
                        segDone = true;
                    }
                }

                if (newlineIdx < 0) break;
                pos = newlineIdx + 1;
            }

            if (lines.Count == 0)
                lines.Add(new VisualLine(0, 0, 0));

            return lines;
        }

        // Returns the visual line index and column within that line for a caret position.
        private static (int line, int col) GetVisualLineCol(int pos, List<VisualLine> lines)
        {
            for (var i = lines.Count - 1; i >= 0; i--)
            {
                if (pos >= lines[i].Start)
                    return (i, pos - lines[i].Start);
            }
            return (0, 0);
        }

        // ── Word selection helpers ────────────────────────────────────────

        private static bool IsWordChar(char c) => char.IsLetterOrDigit(c) || c == '_';

        // Scan leftward from caret position through word chars; returns word start.
        private int WordStartAt(int caretPos)
        {
            int i = caretPos;
            while (i > 0 && IsWordChar(_text[i - 1]))
                i--;
            return i;
        }

        // Scan rightward from caret position through word chars; returns word end.
        private int WordEndAt(int caretPos)
        {
            int i = caretPos;
            while (i < _text.Length && IsWordChar(_text[i]))
                i++;
            return i;
        }

        // Sets both anchor and caret (for word-selection dragging).
        private void SetSelection(int anchor, int caret)
        {
            _selectionAnchor = Math.Clamp(anchor, 0, _text.Length);
            SetCaretPosition(caret, extendSelection: true);
        }

        // Extends selection word-by-word relative to the double-click anchor word.
        private void ExtendSelectionByWord(int dragCaret)
        {
            if (dragCaret <= _wordSelectionAnchorStart)
                SetSelection(_wordSelectionAnchorEnd, WordStartAt(dragCaret));
            else if (dragCaret >= _wordSelectionAnchorEnd)
                SetSelection(_wordSelectionAnchorStart, WordEndAt(dragCaret));
            else
                SetSelection(_wordSelectionAnchorStart, _wordSelectionAnchorEnd);
        }

        // ── Text manipulation ─────────────────────────────────────────────

        private void InsertText(string toInsert)
        {
            if (string.IsNullOrEmpty(toInsert)) return;
            DeleteSelection();
            var available = MaxLength > 0 ? MaxLength - _text.Length : int.MaxValue;
            if (available <= 0) return;
            if (toInsert.Length > available)
                toInsert = toInsert[..available];
            _text = _text[.._caretPosition] + toInsert + _text[_caretPosition..];
            SetCaretPosition(_caretPosition + toInsert.Length);
            NotifyTextChanged();
        }

        private void DeleteSelection()
        {
            if (!HasSelection) return;
            _text = _text[..SelectionMin] + _text[SelectionMax..];
            SetCaretPosition(SelectionMin);
        }

        private void DeleteBackward()
        {
            if (HasSelection) { DeleteSelection(); NotifyTextChanged(); return; }
            if (_caretPosition == 0) return;
            _text = _text[..(_caretPosition - 1)] + _text[_caretPosition..];
            SetCaretPosition(_caretPosition - 1);
            NotifyTextChanged();
        }

        private void DeleteForward()
        {
            if (HasSelection) { DeleteSelection(); NotifyTextChanged(); return; }
            if (_caretPosition >= _text.Length) return;
            _text = _text[.._caretPosition] + _text[(_caretPosition + 1)..];
            NotifyTextChanged();
        }

        private void SelectAll()
        {
            _selectionAnchor = 0;
            SetCaretPosition(_text.Length, extendSelection: true);
        }

        private void CopyToClipboard(IClipboardProvider clipboard)
        {
            if (!HasSelection || clipboard is null) return;
            clipboard.SetText(_text.Substring(SelectionMin, SelectionMax - SelectionMin));
        }

        private void CutToClipboard(IClipboardProvider clipboard)
        {
            if (!HasSelection) return;
            CopyToClipboard(clipboard);
            DeleteSelection();
            NotifyTextChanged();
        }

        private void PasteFromClipboard(IClipboardProvider clipboard)
        {
            if (clipboard is null) return;
            var pasted = clipboard.GetText();
            if (string.IsNullOrEmpty(pasted)) return;
            if (!_isMultiLine)
                pasted = pasted.Replace("\r\n", " ").Replace("\n", " ").Replace("\r", " ");
            else
                pasted = pasted.Replace("\r\n", "\n").Replace("\r", "\n");
            InsertText(pasted);
        }

        private void MoveCaretVertically(int lineDelta, bool extendSelection)
        {
            var font = SpriteFont;
            if (font is null) return;

            var lines = ComputeVisualLines(font, GetTextAreaWidth());
            var (caretLine, caretCol) = GetVisualLineCol(_caretPosition, lines);

            if (_preferredColumn < 0)
                _preferredColumn = (int)MeasureWidth(font, _text, lines[caretLine].Start, caretCol);

            var targetLine = Math.Clamp(caretLine + lineDelta, 0, lines.Count - 1);
            if (targetLine == caretLine) return;

            var targetLineStart = lines[targetLine].Start;
            var targetLineLength = lines[targetLine].Length;
            var targetCol = HitTestLineColumn(font, _text, targetLineStart, targetLineLength, _preferredColumn);

            SetCaretPosition(targetLineStart + targetCol, extendSelection, preservePreferredColumn: true);
        }

        private int HomePosition()
        {
            if (!_isMultiLine) return 0;
            var font = SpriteFont;
            if (font is null) return 0;
            var lines = ComputeVisualLines(font, GetTextAreaWidth());
            var (lineIdx, _) = GetVisualLineCol(_caretPosition, lines);
            return lines[lineIdx].Start;
        }

        private int EndPosition()
        {
            if (!_isMultiLine) return _text.Length;
            var font = SpriteFont;
            if (font is null) return _text.Length;
            var lines = ComputeVisualLines(font, GetTextAreaWidth());
            var (lineIdx, _) = GetVisualLineCol(_caretPosition, lines);
            return lines[lineIdx].Start + lines[lineIdx].Length;
        }

        private void NotifyTextChanged()
        {
            OnStateChanged();
            TextChanged?.Invoke(this, _text);
        }

        // ── IKeyboardInputHandler ─────────────────────────────────────────

        void IKeyboardInputHandler.OnKeyboardInput(in KeyboardInputEvent e)
        {
            if (!IsFocused || !IsInteractable) return;

            if (e.IsSelectAllDown) { SelectAll(); return; }
            if (e.IsCopyDown) { CopyToClipboard(e.ClipboardProvider); return; }
            if (e.IsCutDown) { CutToClipboard(e.ClipboardProvider); return; }
            if (e.IsPasteDown) { PasteFromClipboard(e.ClipboardProvider); return; }

            foreach (var ch in e.TypedCharacters)
                InsertText(ch.ToString());

            if (e.IsBackspaceDown) DeleteBackward();
            if (e.IsDeleteDown) DeleteForward();

            if (e.IsLeftArrowDown)
            {
                if (HasSelection && !e.IsShiftPressed)
                    SetCaretPosition(SelectionMin);
                else
                    SetCaretPosition(_caretPosition - 1, e.IsShiftPressed);
            }

            if (e.IsRightArrowDown)
            {
                if (HasSelection && !e.IsShiftPressed)
                    SetCaretPosition(SelectionMax);
                else
                    SetCaretPosition(_caretPosition + 1, e.IsShiftPressed);
            }

            if (e.IsUpArrowDown && _isMultiLine) MoveCaretVertically(-1, e.IsShiftPressed);
            if (e.IsDownArrowDown && _isMultiLine) MoveCaretVertically(1, e.IsShiftPressed);

            if (e.IsHomeDown) SetCaretPosition(HomePosition(), e.IsShiftPressed);
            if (e.IsEndDown) SetCaretPosition(EndPosition(), e.IsShiftPressed);

            if (e.IsReturnDown)
            {
                if (_isMultiLine)
                    InsertText("\n");
                else
                    ReturnPressed?.Invoke(this);
            }
        }

        // ── Pointer input ─────────────────────────────────────────────────

        protected override void OnScrollWheel(in PointerScrollEvent pointerEvent)
        {
            if (!_isMultiLine)
            {
                base.OnScrollWheel(pointerEvent);
                return;
            }

            var font = SpriteFont;
            if (font is null) return;

            var lines = ComputeVisualLines(font, GetTextAreaWidth());
            var lineSpacing = font.MeasureString("Ag").Y + LineSpacingExtra;
            var totalH = lines.Count * lineSpacing;
            var visH = Size.Y - 2 * TextPaddingY;
            var maxOffset = Math.Max(0f, totalH - visH);
            if (maxOffset <= 0f) return;

            // Scroll wheel up (positive delta) = show content above = decrease offset
            _scrollOffsetY = Math.Clamp(
                _scrollOffsetY - Math.Sign(pointerEvent.Delta) * lineSpacing * 3,
                0, maxOffset);
            OnStateChanged();
        }

        protected override void OnPointerDown(in PointerEvent pointerEvent)
        {
            if (!_isMultiLine && _hasClearButton && _text.Length > 0
                && pointerEvent.Position.X >= BoundingRectangle.Right - ClearButtonWidth)
            {
                Text = string.Empty;
                return;
            }

            if (_isMultiLine)
            {
                var scrollbarX = BoundingRectangle.Right - ScrollbarWidth;
                if (pointerEvent.Position.X >= scrollbarX)
                {
                    HandleScrollbarClick(pointerEvent.Position);
                    return;
                }
            }

            base.OnPointerDown(pointerEvent);

            var ks = Keyboard.GetState();
            var isShift = ks.IsKeyDown(Keys.LeftShift) || ks.IsKeyDown(Keys.RightShift);
            var localX = pointerEvent.Position.X - BoundingRectangle.Left - TextPaddingX + _scrollOffset;

            int clickCaret;
            if (_isMultiLine)
            {
                var localY = pointerEvent.Position.Y - BoundingRectangle.Top - TextPaddingY + _scrollOffsetY;
                clickCaret = HitTestCaretMultiLine(localX, localY);
            }
            else
            {
                clickCaret = HitTestCaret(localX);
            }

            var now = Environment.TickCount64;
            var isDoubleClick = !isShift
                && now - _lastClickTime < DoubleClickMaxIntervalMs
                && Math.Abs(pointerEvent.Position.X - _lastClickPosition.X) <= DoubleClickMaxDistancePx
                && Math.Abs(pointerEvent.Position.Y - _lastClickPosition.Y) <= DoubleClickMaxDistancePx;
            _lastClickTime = now;
            _lastClickPosition = pointerEvent.Position;

            if (isDoubleClick && _text.Length > 0)
            {
                // Determine which word (or single non-word char) was clicked.
                bool touchRight = clickCaret < _text.Length && IsWordChar(_text[clickCaret]);
                bool touchLeft  = clickCaret > 0 && IsWordChar(_text[clickCaret - 1]);

                int wordStart, wordEnd;
                if (touchRight || touchLeft)
                {
                    wordStart = WordStartAt(clickCaret);
                    wordEnd   = WordEndAt(clickCaret);
                }
                else
                {
                    // Non-word character: select just that one character.
                    wordStart = Math.Min(clickCaret, _text.Length - 1);
                    wordEnd   = wordStart + 1;
                }

                _wordSelectionMode        = true;
                _wordSelectionAnchorStart = wordStart;
                _wordSelectionAnchorEnd   = wordEnd;
                SetSelection(wordStart, wordEnd);
                return;
            }

            _wordSelectionMode = false;
            SetCaretPosition(clickCaret, extendSelection: isShift);
        }

        protected override void OnPointerUp(in PointerEvent pointerEvent)
        {
            _isDraggingScrollbar = false;
            base.OnPointerUp(pointerEvent);
        }

        protected override void OnPointerDrag(in PointerDragEvent pointerEvent)
        {
            if (_isDraggingScrollbar)
            {
                HandleScrollbarDrag(pointerEvent.Position.Y);
                return;
            }

            base.OnPointerDrag(pointerEvent);
            if (!pointerEvent.IsPrimaryButtonPressed) return;

            var localX = pointerEvent.Position.X - BoundingRectangle.Left - TextPaddingX + _scrollOffset;
            int dragCaret;
            if (_isMultiLine)
            {
                var localY = pointerEvent.Position.Y - BoundingRectangle.Top - TextPaddingY + _scrollOffsetY;
                dragCaret = HitTestCaretMultiLine(localX, localY);
            }
            else
            {
                dragCaret = HitTestCaret(localX);
            }

            if (_wordSelectionMode)
                ExtendSelectionByWord(dragCaret);
            else
                SetCaretPosition(dragCaret, extendSelection: true);
        }

        private void HandleScrollbarClick(Point clickPos)
        {
            var font = SpriteFont;
            if (font is null) return;

            var lines = ComputeVisualLines(font, GetTextAreaWidth());
            var lineSpacing = font.MeasureString("Ag").Y + LineSpacingExtra;
            var totalH = lines.Count * lineSpacing;
            var visH = Size.Y - 2 * TextPaddingY;
            var maxOffset = Math.Max(0f, totalH - visH);
            if (maxOffset <= 0f) return;

            GetScrollbarThumb(BoundingRectangle, totalH, visH, out var thumbY, out var thumbH);

            if (clickPos.Y >= thumbY && clickPos.Y < thumbY + thumbH)
            {
                _isDraggingScrollbar = true;
                _scrollbarDragStartY = clickPos.Y;
                _scrollbarDragStartOffset = _scrollOffsetY;
            }
            else
            {
                // Click on track — jump to proportional position
                var clickFrac = (float)(clickPos.Y - BoundingRectangle.Top) / BoundingRectangle.Height;
                _scrollOffsetY = Math.Clamp(clickFrac * maxOffset, 0, maxOffset);
                OnStateChanged();
            }
        }

        private void HandleScrollbarDrag(float currentY)
        {
            var font = SpriteFont;
            if (font is null) return;

            var lines = ComputeVisualLines(font, GetTextAreaWidth());
            var lineSpacing = font.MeasureString("Ag").Y + LineSpacingExtra;
            var totalH = lines.Count * lineSpacing;
            var visH = Size.Y - 2 * TextPaddingY;
            var maxOffset = Math.Max(0f, totalH - visH);
            if (maxOffset <= 0f) return;

            GetScrollbarThumb(BoundingRectangle, totalH, visH, out _, out var thumbH);
            var thumbTrackH = BoundingRectangle.Height - thumbH;
            var scrollPerPixel = thumbTrackH > 0 ? maxOffset / thumbTrackH : 0;
            var deltaY = currentY - _scrollbarDragStartY;
            _scrollOffsetY = Math.Clamp(_scrollbarDragStartOffset + deltaY * scrollPerPixel, 0, maxOffset);
            OnStateChanged();
        }

        private int HitTestCaret(float localX)
        {
            var font = SpriteFont;
            if (font is null || string.IsNullOrEmpty(_text)) return 0;

            float x = 0;
            for (var i = 0; i < _text.Length; i++)
            {
                var charWidth = font.MeasureString(_text[i].ToString()).X;
                if (localX < x + charWidth * 0.5f)
                    return i;
                x += charWidth;
            }
            return _text.Length;
        }

        private int HitTestCaretMultiLine(float localX, float localY)
        {
            var font = SpriteFont;
            if (font is null) return 0;

            var fontHeight = font.MeasureString("Ag").Y;
            var lineSpacing = fontHeight + LineSpacingExtra;
            var lineIndex = (int)(localY / lineSpacing);

            var lines = ComputeVisualLines(font, GetTextAreaWidth());
            lineIndex = Math.Clamp(lineIndex, 0, lines.Count - 1);

            var line = lines[lineIndex];
            return line.Start + HitTestLineColumn(font, _text, line.Start, line.Length, localX);
        }

        private static int HitTestLineColumn(SpriteFont font, string text, int lineStart, int lineLength, float targetX)
        {
            float x = 0;
            for (var i = 0; i < lineLength; i++)
            {
                var charWidth = font.MeasureString(text[lineStart + i].ToString()).X;
                if (targetX < x + charWidth * 0.5f)
                    return i;
                x += charWidth;
            }
            return lineLength;
        }

        // ── Scrollbar helpers ─────────────────────────────────────────────

        private void GetScrollbarThumb(Rectangle bounds, float totalContentHeight, float visibleHeight,
            out int thumbY, out int thumbH)
        {
            thumbH = Math.Max(ScrollbarMinThumbHeight,
                (int)(bounds.Height * visibleHeight / totalContentHeight));
            var thumbTrackH = bounds.Height - thumbH;
            var maxOffset = totalContentHeight - visibleHeight;
            var scrollFrac = maxOffset > 0 ? Math.Clamp(_scrollOffsetY / maxOffset, 0f, 1f) : 0f;
            thumbY = bounds.Top + (int)(scrollFrac * thumbTrackH);
        }

        private void DrawScrollbar(IUiRenderer renderer, Rectangle bounds, Rectangle? clip,
            float totalContentHeight, float visibleHeight)
        {
            var trackRect = new Rectangle(bounds.Right - ScrollbarWidth, bounds.Top, ScrollbarWidth, bounds.Height);
            renderer.DrawSkinnedRectangle(StandardSkin.WhitePixel, DrawMode.Simple, trackRect, clip,
                new Color(210, 210, 210, 200));

            if (totalContentHeight <= visibleHeight) return;

            GetScrollbarThumb(bounds, totalContentHeight, visibleHeight, out var thumbY, out var thumbH);
            var thumbRect = new Rectangle(trackRect.Left + 2, thumbY, trackRect.Width - 4, thumbH);
            renderer.DrawSkinnedRectangle(StandardSkin.WhitePixel, DrawMode.Simple, thumbRect, clip,
                new Color(140, 140, 140, 220));
        }

        // ── IDrawableElement ──────────────────────────────────────────────

        void IDrawableElement.Draw(IUiRenderer renderer)
        {
            var bounds = BoundingRectangle;
            var clip = ClipMask;
            var font = SpriteFont;

            var skin = !IsInteractable ? DisabledSkin : IsFocused ? FocusedSkin : NormalSkin;
            var bgColor = !IsInteractable ? DisabledColor : IsFocused ? FocusedColor : NormalColor;
            renderer.DrawSkinnedRectangle(skin, DrawMode.Sliced, bounds, clip, bgColor);

            if (font is null) return;

            var fontHeight = font.MeasureString("Ag").Y;
            var textAreaLeft = bounds.Left + TextPaddingX;
            var textAreaRight = bounds.Right - TextPaddingX
                - (_isMultiLine ? ScrollbarWidth : (_hasClearButton ? ClearButtonWidth : 0));
            var textAreaWidth = textAreaRight - textAreaLeft;

            if (textAreaWidth <= 0) return;

            var textClipRect = new Rectangle(textAreaLeft, bounds.Top, textAreaWidth, bounds.Height);
            Rectangle? textClip = textClipRect;
            if (clip.HasValue)
            {
                var intersection = Rectangle.Intersect(textClipRect, clip.Value);
                if (intersection.Width <= 0 || intersection.Height <= 0) return;
                textClip = intersection;
            }

            if (_isMultiLine)
            {
                var lines = ComputeVisualLines(font, textAreaWidth);
                DrawMultiLine(renderer, font, bounds, clip, textClip, fontHeight,
                    textAreaLeft, textAreaRight, lines);
            }
            else
            {
                DrawSingleLine(renderer, font, bounds, clip, textClip, fontHeight,
                    textAreaLeft, textAreaRight);
            }
        }

        private void DrawSingleLine(IUiRenderer renderer, SpriteFont font,
            Rectangle bounds, Rectangle? clip, Rectangle? textClip,
            float fontHeight, float textAreaLeft, float textAreaRight)
        {
            var textY = bounds.Top + (bounds.Height - fontHeight) * 0.5f;

            if (_text.Length == 0)
            {
                if (_placeholderText.Length > 0)
                    renderer.DrawString(font, _placeholderText,
                        new Vector2(textAreaLeft, textY), textClip, PlaceholderColor);
            }
            else
            {
                var originX = textAreaLeft - _scrollOffset;

                if (HasSelection)
                {
                    var selStartX = originX + MeasureWidth(font, _text, 0, SelectionMin);
                    var selEndX = originX + MeasureWidth(font, _text, 0, SelectionMax);
                    var visStart = Math.Max(selStartX, textAreaLeft);
                    var visEnd = Math.Min(selEndX, textAreaRight);
                    if (visEnd > visStart)
                    {
                        var selRect = new Rectangle(
                            (int)visStart, bounds.Top,
                            (int)(visEnd - visStart), bounds.Height);
                        renderer.DrawSkinnedRectangle(StandardSkin.WhitePixel,
                            DrawMode.Simple, selRect, textClip, SelectionColor);
                    }
                }

                renderer.DrawString(font, _text, new Vector2(originX, textY), textClip, TextColor);
            }

            if (IsFocused && IsInteractable && IsCaretVisible())
            {
                var caretX = (int)(textAreaLeft + MeasureWidth(font, _text, 0, _caretPosition) - _scrollOffset);
                if (caretX >= textAreaLeft && caretX <= textAreaRight)
                {
                    var caretRect = new Rectangle(caretX, bounds.Top + 2, CaretWidth, bounds.Height - 4);
                    renderer.DrawSkinnedRectangle(StandardSkin.WhitePixel,
                        DrawMode.Simple, caretRect, clip, CaretColor);
                }
            }

            if (_hasClearButton && _text.Length > 0)
            {
                var xText = "×";
                var xSize = font.MeasureString(xText);
                var xPos = new Vector2(
                    bounds.Right - ClearButtonWidth + (ClearButtonWidth - xSize.X) * 0.5f,
                    bounds.Top + (bounds.Height - xSize.Y) * 0.5f);
                renderer.DrawString(font, xText, xPos, clip, new Color(150, 150, 150));
            }
        }

        private void DrawMultiLine(IUiRenderer renderer, SpriteFont font,
            Rectangle bounds, Rectangle? clip, Rectangle? textClip,
            float fontHeight, float textAreaLeft, float textAreaRight,
            List<VisualLine> lines)
        {
            var lineSpacing = fontHeight + LineSpacingExtra;
            var textAreaTop = bounds.Top + TextPaddingY;
            var visibleHeight = Size.Y - 2 * TextPaddingY;

            if (_text.Length == 0)
            {
                if (_placeholderText.Length > 0)
                    renderer.DrawString(font, _placeholderText,
                        new Vector2(textAreaLeft, textAreaTop), textClip, PlaceholderColor);
            }
            else
            {
                var selMin = SelectionMin;
                var selMax = SelectionMax;

                for (var i = 0; i < lines.Count; i++)
                {
                    var line = lines[i];
                    var lineY = textAreaTop + i * lineSpacing - _scrollOffsetY;
                    if (lineY + lineSpacing < bounds.Top || lineY > bounds.Bottom) continue;

                    var lineStart = line.Start;
                    var lineLength = line.Length;
                    var lineEnd = lineStart + lineLength;
                    var lineText = _text.Substring(lineStart, lineLength);

                    if (HasSelection && selMin < lineEnd && selMax > lineStart)
                    {
                        var localSelStart = Math.Max(selMin - lineStart, 0);
                        var localSelEnd = Math.Min(selMax - lineStart, lineLength);

                        var selStartX = textAreaLeft + MeasureWidth(font, lineText, 0, localSelStart);
                        // Extend selection highlight to right edge when selection continues past this visual line
                        var selEndX = selMax > line.NextStart && i + 1 < lines.Count
                            ? textAreaRight + 2
                            : textAreaLeft + MeasureWidth(font, lineText, 0, localSelEnd);

                        var visStart = Math.Max(selStartX, textAreaLeft);
                        var visEnd = Math.Min(selEndX, textAreaRight);
                        if (visEnd > visStart)
                        {
                            var selRect = new Rectangle(
                                (int)visStart, (int)lineY,
                                (int)(visEnd - visStart), (int)lineSpacing);
                            renderer.DrawSkinnedRectangle(StandardSkin.WhitePixel,
                                DrawMode.Simple, selRect, textClip, SelectionColor);
                        }
                    }

                    renderer.DrawString(font, lineText, new Vector2(textAreaLeft, lineY), textClip, TextColor);
                }
            }

            if (IsFocused && IsInteractable && IsCaretVisible())
            {
                var (caretLine, caretCol) = GetVisualLineCol(_caretPosition, lines);
                var caretLineData = lines[caretLine];
                var caretX = (int)(textAreaLeft + MeasureWidth(font, _text, caretLineData.Start, caretCol));
                var caretY = (int)(textAreaTop + caretLine * lineSpacing - _scrollOffsetY);

                if (caretX >= textAreaLeft && caretX <= textAreaRight
                    && caretY + lineSpacing > bounds.Top && caretY < bounds.Bottom)
                {
                    var caretRect = new Rectangle(caretX, caretY + 1, CaretWidth, (int)fontHeight - 2);
                    renderer.DrawSkinnedRectangle(StandardSkin.WhitePixel,
                        DrawMode.Simple, caretRect, clip, CaretColor);
                }
            }

            var totalHeight = lines.Count * lineSpacing;
            DrawScrollbar(renderer, bounds, clip, totalHeight, visibleHeight);
        }

        // ── Helpers ───────────────────────────────────────────────────────

        private static float MeasureWidth(SpriteFont font, string text, int start, int count)
        {
            if (count <= 0 || start >= text.Length) return 0f;
            return font.MeasureString(text.Substring(start, Math.Min(count, text.Length - start))).X;
        }
    }
}
