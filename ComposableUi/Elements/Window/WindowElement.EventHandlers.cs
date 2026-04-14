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
            IsDragHandlePressed = true;

            BringToFront();
            Select();
        }

        private void OnDragHandlePointerUp(PointerInputHandlerElement sender,
            PointerEvent pointerEvent)
        {
            IsDragHandlePressed = false;
        }

        private void OnDragHandlePointerFixedDrag(PointerInputHandlerElement sender,
            PointerDragEvent pointerEvent)
        {
            if (IsResizingInternally)
                return;

            if (IsMaximized)
                return;

            var delta = pointerEvent.Delta;
            var root = ResolveRootContainer();

            var shouldConstrainToParent = ConstrainToParent
                && root.Parent is not null;
            if (shouldConstrainToParent)
            {
                var boundingRectangle = root.BoundingRectangle;
                boundingRectangle.Location += pointerEvent.Delta;

                delta += CalculateOffsetToConstrainToParent(boundingRectangle,
                    root.Parent.BoundingRectangle);
            }
            root.Position += delta.ToVector2();
        }

        // Tab button.
        private void OnTabButtonPointerDown(PointerInputHandlerElement sender,
            PointerEvent pointerEvent)
        {
            if (IsResizingInternally)
                return;

            if (IsMaximized)
                return;

            IsTabPressed = true;
            _isDragStarted = false;

            _dragOffset = CalculateTabOffset();
            _dragDeltaAccumulator = Vector2.Zero;

            Tab.InnerElement.IsEnabled = false;

            _composableWindowsSolver?.SetSource(this);

            TabPointerDown?.Invoke(this, pointerEvent);

            Select();
        }

        private void OnTabButtonPointerUp(PointerInputHandlerElement sender,
            PointerEvent pointerEvent)
        {
            if (!IsTabPressed)
                return;

            IsTabPressed = false;

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
            if (!IsTabPressed)
                return;

            var deltaVector = pointerEvent.Delta.ToVector2();
            _dragDeltaAccumulator += deltaVector;

            if (!_isDragStarted)
            {
                _isDragStarted = MathF.Abs(_dragDeltaAccumulator.X) + MathF.Abs(_dragDeltaAccumulator.Y) >= DefaultDragThreshold;
                if (!_isDragStarted)
                    return;

                _dragDeltaAccumulator = deltaVector;
            }

            if (ConstrainToParent)
            {
                var root = ResolveRootContainer();
                if (root.Parent is not null)
                {
                    var boundingRectangle = BoundingRectangle;
                    boundingRectangle.Location += (_dragDeltaAccumulator + _dragOffset).ToPoint();

                    var tabBoundingRectangle = new Rectangle()
                    {
                        Location = boundingRectangle.Location,
                        Size = Tab.Size.ToPoint()
                    };

                    _dragDeltaAccumulator -= deltaVector;

                    var fixedDragDelta = Tab.CalculateFixedDragDelta(tabBoundingRectangle,
                        pointerEvent.Delta, pointerEvent.Position);
                    if (fixedDragDelta == Point.Zero)
                        return;

                    var constrainedDelta = CalculateOffsetToConstrainToParent(boundingRectangle,
                        root.Parent.BoundingRectangle) + fixedDragDelta;
                    _dragDeltaAccumulator += constrainedDelta.ToVector2();

                    pointerEvent = new PointerDragEvent(pointerEvent.Pointer, pointerEvent.Position,
                        pointerEvent.IsPrimaryButtonPressed, pointerEvent.IsSecondaryButtonPressed, constrainedDelta);
                }
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
            Select();
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
