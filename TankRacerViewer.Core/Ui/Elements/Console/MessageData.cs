using System;

namespace TankRacerViewer.Core
{
    public readonly record struct MessageData(int Index,
        MessageType Type,
        string Message, DateTime DateTime);
}
