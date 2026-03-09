using System;

using Microsoft.Xna.Framework;

namespace ComposableUi
{
    public partial class WindowElement
    {
        // Drag handle.
        private void OnDragHandlePointerDown(PointerInputHandlerElement sender,
            PointerEvent pointerEvent)
        {
            BringToFront();
            Focus();
        }

        private void OnDragHandlePointerFixedDrag(PointerInputHandlerElement sender,
            PointerDragEvent pointerEvent)
        {
            if (IsResizingInternally)
                return;

            if (IsMaximized)
                return;

            var root = ResolveRoot();
            root.Position += pointerEvent.Delta.ToVector2();
        }

        // Tab button.
        private void OnTabButtonPointerDown(PointerInputHandlerElement sender,
            PointerEvent pointerEvent)
        {
            if (IsResizingInternally)
                return;

            if (IsMaximized)
                return;

            _isTabPressed = true;
            _isDragStarted = false;

            _dragOffset = CalculateTabOffset();
            _dragDeltaAccumulator = Vector2.Zero;

            Tab.InnerElement.IsEnabled = false;

            _composableWindowsSolver?.SetSource(this);

            TabPointerDown?.Invoke(this, pointerEvent);

            Focus();
        }

        private void OnTabButtonPointerUp(PointerInputHandlerElement sender,
            PointerEvent pointerEvent)
        {
            if (!_isTabPressed)
                return;

            _isTabPressed = false;

            Tab.InnerElement.IsEnabled = true;

            if (_composableWindowsSolver is not null)
            {
                if (!_composableWindowsSolver.TryDock())
                {
                    _dragDeltaAccumulator += _dragOffset;
                    if (Container is not null)
                    {
                        Undock(this, Position + _dragDeltaAccumulator);

                        var oldSize = Size;
                        var newSize = Vector2.Max(MinSize, oldSize);
                        SetSize(newSize);
                        Position += (newSize - oldSize) * Pivot;
                    }
                    else
                    {
                        Position += _dragDeltaAccumulator;
                    }

                    MovedByTab?.Invoke(this);
                }
                BringToFront();
            }

            TabPointerUp?.Invoke(this, pointerEvent);
        }

        private void OnTabButtonPointerDrag(PointerInputHandlerElement sender,
            PointerDragEvent pointerEvent)
        {
            if (!_isTabPressed)
                return;

            _dragDeltaAccumulator += pointerEvent.Delta.ToVector2();

            if (!_isDragStarted)
            {
                _isDragStarted = MathF.Abs(_dragDeltaAccumulator.X) + MathF.Abs(_dragDeltaAccumulator.Y) >= DefaultDragThreshold;
                if (!_isDragStarted)
                    return;

                _dragDeltaAccumulator = pointerEvent.Delta.ToVector2();
            }

            TabPointerDrag?.Invoke(this, pointerEvent);
        }

        // Tab preview.
        private void OnTabPreviewInputAreaPointerMove(PointerInputHandlerElement sender,
            PointerEvent pointerEvent)
        {
            if (IsMaximized)
                return;

            if (_composableWindowsSolver is null)
                return;

            var source = _composableWindowsSolver.Source;
            if (source is null)
                return;

            ShowTabPreview(source, pointerEvent.Position);
            _composableWindowsSolver.SetTabTarget(this);
        }

        private void OnTabPreviewInputAreaPointerLeave(PointerInputHandlerElement sender,
            PointerEvent pointerEvent)
        {
            _composableWindowsSolver?.ClearTabTarget(this);
            HideTabPreviewIfPossible();
        }

        // Split preview.
        private void OnSplitPreviewInputAreaPointerDown(PointerInputHandlerElement sender,
            PointerEvent pointerEvent)
        {
            BringToFront();
            Focus();
        }

        private void OnSplitPreviewInputAreaPointerMove(PointerInputHandlerElement sender,
            PointerEvent pointerEvent)
        {
            if (IsMaximized)
                return;

            if (_composableWindowsSolver is null)
                return;

            var source = _composableWindowsSolver.Source;
            if (source is null)
                return;

            if (TryShowSplitPreview(source, pointerEvent.Position))
            {
                _composableWindowsSolver.SetSplitTarget(this);
            }
            else
            {
                _composableWindowsSolver.ClearSplitTarget(this);
            }
        }

        private void OnSplitPreviewInputAreaPointerLeave(PointerInputHandlerElement sender,
            PointerEvent pointerEvent)
        {
            _composableWindowsSolver?.ClearSplitTarget(this);
            HideSplitPreviewIfPossible();
        }

        // Inner resizing.
        private void OnInnerResizeHandlePointerMove(PointerInputHandlerElement sender,
            PointerEvent pointerEvent)
        {
            if (Container is null)
                return;

            if (pointerEvent.IsPrimaryButtonPressed)
                return;

            ResolveResizeCursor(pointerEvent.Pointer, pointerEvent.Position);
        }

        private void OnInnerResizeHandlePointerLeave(PointerInputHandlerElement sender,
            PointerEvent pointerEvent)
        {
            if (pointerEvent.IsPrimaryButtonPressed)
                return;

            pointerEvent.Pointer.SetCursor(PointerCursor.Arrow);
        }

        private void OnInnerResizeHandlePointerDown(PointerInputHandlerElement sender,
            PointerEvent pointerEvent)
        {
            if (Container is null)
                return;

            _resizeNormal = GetResizeNormal(pointerEvent.Position);
            _resizeAxis = new Vector2(MathF.Abs(_resizeNormal.X), MathF.Abs(_resizeNormal.Y));
        }

        private void OnInnerResizeHandlePointerUp(PointerInputHandlerElement sender,
            PointerEvent pointerEvent)
        {
            _resizeNormal = Vector2.Zero;
        }

        private void OnInnerResizeHandlePointerFixedDrag(PointerInputHandlerElement sender,
            PointerDragEvent pointerEvent)
        {
            if (!IsResizingInternally)
                return;

            var deltaVector = pointerEvent.Delta.ToVector2();

            var axisDelta = Vector2.Dot(_resizeAxis, deltaVector);
            if (axisDelta == 0)
                return;

            var delta = Vector2.Dot(_resizeNormal, deltaVector);
            var dockingMode = EdgeNormalToDockingMode(_resizeNormal);

            WindowContainerElement.IncreaseSizeInHierarchyIfPossible(this, dockingMode, _resizeAxis, axisDelta, delta);
        }

        // Buttons.
        private void OnCloseButtonPointerClick(PointerInputHandlerElement sender,
            PointerEvent pointerEvent)
        {
            Close();
        }

        private void OnMaximizeButtonPointerClick(PointerInputHandlerElement sender,
            PointerEvent pointerEvent)
        {
            Maximize();
        }

        private void OnRestoreButtonPointerClick(PointerInputHandlerElement sender,
            PointerEvent pointerEvent)
        {
            Restore();
        }

        protected override void OnPointerDown(in PointerEvent pointerEvent)
        {
            base.OnPointerDown(pointerEvent);
            BringToFront();
        }
    }
}
