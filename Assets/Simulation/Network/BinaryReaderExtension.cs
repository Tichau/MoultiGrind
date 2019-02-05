using System.IO;
using Framework.Network;

namespace Simulation.Network
{
    public static class BinaryReaderExtension
    {
        public static void ReadCreateGameOrder(this BinaryReader stream, out ulong timeElapsedPerTick)
        {
            timeElapsedPerTick = stream.ReadUInt64();
        }

        public static void ReadJoinGameOrder(this BinaryReader stream, out ulong timeElapsedPerTick, out ulong durationBetweenTwoTicks, out byte playerId, out Game game)
        {
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

        public static void ReadCraftRecipeOrder(this BinaryReader stream, out uint recipeId)
        {
            recipeId = stream.ReadUInt32();
        }
    }
}