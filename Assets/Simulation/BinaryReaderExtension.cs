using System.IO;
using Framework;
using Simulation.Data;
using Simulation.Network;

namespace Simulation
{
    public static class BinaryReaderExtension
    {
        public static T ReadReference<T>(this BinaryReader stream)
            where T : IDatabaseElement
        {
            var id = stream.ReadUInt32();
            Databases.Instance.TryGet(id, out T definition);
            return definition;
        }

        public static void ReadCreateGameOrder(this BinaryReader stream, out byte gameInstanceId, out ulong timeElapsedPerTick)
        {
            gameInstanceId = stream.ReadByte();
            timeElapsedPerTick = stream.ReadUInt64();
        }

        public static void ReadJoinGameOrder(this BinaryReader stream, out byte gameInstanceId, out byte clientId, out ulong timeElapsedPerTick, out ulong durationBetweenTwoTicks, out byte playerId, out Simulation.Game.Game game)
        {
            gameInstanceId = stream.ReadByte();
            clientId = stream.ReadByte();
            timeElapsedPerTick = stream.ReadUInt64();
            durationBetweenTwoTicks = stream.ReadUInt64();
            playerId = stream.ReadByte();
            stream.ParseGame(out game);
        }
        
        public static void ReadListGamesOrder(this BinaryReader stream, out GameInstanceSummary[] gameSummaries)
        {
            gameSummaries = stream.ReadArray<GameInstanceSummary>();
        }

        public static void ParseGame(this BinaryReader stream, out Simulation.Game.Game game)
        {
            var isNull = stream.ReadBoolean();
            if (isNull)
            {
                game = null;
                return;
            }

            game = new Simulation.Game.Game();
            game.Deserialize(stream);
        }
    }
}
