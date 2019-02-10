namespace Framework.Network
{
    public enum MessageType : byte
    {
        Invalid,

        Connect,
        Disconnect,
        Ping,
        Pong,
        Text,

        GameOrder,
    }
}
