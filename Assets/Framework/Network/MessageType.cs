namespace Framework.Network
{
    public enum MessageType : byte
    {
        Invalid,

        Ping,
        Pong,
        Text,
        Binary,
    }
}
