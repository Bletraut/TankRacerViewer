using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ComposableUi
{
    public sealed class UiBatcher
    {
        // Static.
        private static bool CanBatch(RenderCommand commandA, RenderCommand commandB)
        {
            return commandA.Type == commandB.Type
                && commandA.ClipMask == commandB.ClipMask
                && commandA.Texture == commandB.Texture;
        }

        private static bool IsBoundingBoxIntersects(RenderCommand command, List<RenderCommand> commands)
        {
            for (var i = 0; i < commands.Count; i++)
            {
                if (command.BoundingRectangle.Intersects(commands[i].BoundingRectangle))
                    return true;
            }

            return false;
        }

        // Class.
        private readonly List<RenderCommand> _batchedCommands = [];
        public IReadOnlyList<RenderCommand> BatchedCommands { get; }

        private readonly List<RenderCommand> _addedCommands = [];

        private readonly List<RenderCommand> _currentCommands = [];
        private readonly List<RenderCommand> _breakingCommands = [];

        private int _addedCommandCount = 0;

        private bool _isBatchDirty;

        public UiBatcher()
        {
            BatchedCommands = _batchedCommands.AsReadOnly();
        }

        public void AddRenderCommand(int id, int type,
            Rectangle boundingRectangle, Rectangle? clipMask, Texture texture)
        {
            var newCommand = new RenderCommand(id, type, boundingRectangle, clipMask, texture);

            if (_addedCommandCount >= _addedCommands.Count)
            {
                _isBatchDirty = true;
                _addedCommands.Add(newCommand);
            }
            else
            {
                _isBatchDirty = _isBatchDirty 
                    || newCommand != _addedCommands[_addedCommandCount];
                _addedCommands[_addedCommandCount] = newCommand;
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

                    var canBatchCommand = CanBatch(currentCommand, nextCommand)
                        && !IsBoundingBoxIntersects(nextCommand, breakingCommands);
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
