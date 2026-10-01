using System;

using Microsoft.Xna.Framework;

namespace ComposableUi
{
    public interface IRenderCommand<T> : IEquatable<T> where T : struct, IRenderCommand<T>
    {
        public Rectangle BoundingRectangle { get; }

        public bool CanBatchWith(in T commandB);
    }
}
