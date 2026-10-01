using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using Microsoft.Xna.Framework;

namespace ComposableUi
{
    public sealed class UiBatcher<T> where T : struct, IRenderCommand<T>
    {
        // Static.
        private static bool IsBoundingRectangleIntersects(Rectangle boundingRectangle, List<T> commands)
        {
            var commandsSpan = CollectionsMarshal.AsSpan(commands);
            foreach (ref var command in commandsSpan)
            {
                if (boundingRectangle.Intersects(command.BoundingRectangle))
                    return true;
            }

            return false;
        }

        // Class.
        private readonly List<T> _batchedCommands = [];
        public ReadOnlySpan<T> BatchedCommands => CollectionsMarshal.AsSpan(_batchedCommands);

        private readonly List<T> _addedCommands = [];

        private readonly List<T> _currentCommands = [];
        private readonly List<T> _breakingCommands = [];

        private int _addedCommandCount;

        private bool _isBatchDirty;

        public void AddRenderCommand(T renderCommand)
        {
            if (_addedCommandCount >= _addedCommands.Count)
            {
                _isBatchDirty = true;
                _addedCommands.Add(renderCommand);
            }
            else
            {
                _isBatchDirty = _isBatchDirty 
                    || !renderCommand.Equals(_addedCommands[_addedCommandCount]);
                _addedCommands[_addedCommandCount] = renderCommand;
            }

            _addedCommandCount++;
        }

        public void Batch()
        {
            _isBatchDirty |= _addedCommandCount != _batchedCommands.Count;
            if (!_isBatchDirty)
            {
                _addedCommandCount = 0;
                return;
            }

            _batchedCommands.Clear();
            _currentCommands.Clear();
            for (var i = 0; i < _addedCommandCount; i++)
                _currentCommands.Add(_addedCommands[i]);

            var currentCommands = _currentCommands;
            var breakingCommands = _breakingCommands;

            while (currentCommands.Count > 0)
            {
                breakingCommands.Clear();

                var currentCommand = currentCommands[0];
                _batchedCommands.Add(currentCommand);

                for (var i = 1; i < currentCommands.Count; i++)
                {
                    var nextCommand = currentCommands[i];

                    var canBatchCommand = currentCommand.CanBatchWith(nextCommand)
                        && !IsBoundingRectangleIntersects(nextCommand.BoundingRectangle, breakingCommands);
                    if (canBatchCommand)
                    {
                        _batchedCommands.Add(nextCommand);
                    }
                    else
                    {
                        breakingCommands.Add(nextCommand);
                    }
                }
                (currentCommands, breakingCommands) = (breakingCommands, currentCommands);
            }

            _isBatchDirty = false;
            _addedCommandCount = 0;
        }
    }
}
