using System.IO;
using Framework.Network;

namespace Simulation.Network
{
    public static class BinaryWriterExtension
    {
        public static void WriteCreateGameOrder(this BinaryWriter stream, ulong timeElapsedPerTick)
        {
            stream.Write(timeElapsedPerTick);
        }

        public static void WriteJoinGameOrder(this BinaryWriter stream, ulong timeElapsedPerTick, ulong durationBetweenTwoTicks, byte playerId, Game game)
        {
            stream.Write(timeElapsedPerTick);
            stream.Write(durationBetweenTwoTicks);
            stream.Write(playerId);
            stream.WriteGame(game);
        }

        public static void WriteGame(this BinaryWriter stream, Game game)
        {
            var isNull = game == null;
            stream.Write(isNull);
            if (isNull)
            {
                return;
            }

            game.Serialize(stream);
        }
    }
}
