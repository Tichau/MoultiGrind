using System.IO;
using Framework.Network;

namespace Simulation.Network
{
    public static class BinaryReaderExtension
    {
        public static void ReadCreateGameOrder(this BinaryReader stream, out byte gameInstanceId, out ulong timeElapsedPerTick)
        {
            gameInstanceId = stream.ReadByte();
            timeElapsedPerTick = stream.ReadUInt64();
        }

        public static void ReadJoinGameOrder(this BinaryReader stream, out byte gameInstanceId, out byte clientId, out ulong timeElapsedPerTick, out ulong durationBetweenTwoTicks, out byte playerId, out Game game)
        {
            gameInstanceId = stream.ReadByte();
            clientId = stream.ReadByte();
            timeElapsedPerTick = stream.ReadUInt64();
            durationBetweenTwoTicks = stream.ReadUInt64();
            playerId = stream.ReadByte();
            stream.ParseGame(out game);
        }

        public static void ParseGame(this BinaryReader stream, out Game game)
        {
            var isNull = stream.ReadBoolean();
            if (isNull)
            {
                game = null;
                return;
            }

            game = new Game();
            game.Deserialize(stream);
        }
    }
}