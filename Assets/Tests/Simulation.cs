using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Simulation;
using Simulation.Network;
using UnityEngine;
using UnityEngine.TestTools;

public class SimulationTest
{
    private void CallSync(Action target)
    {
        var task = new Task(target);
        task.RunSynchronously();
    }

    private T CallSyncWithReturn<T>(Func<T> target)
    {
        var task = new Task<T>(target);
        task.RunSynchronously();
        return task.Result;
    }

    [Test]
    public void GameClientCanCreateANewGame()
    {
        // Dummy database.
        var databasesGameObject = new GameObject("Databases");
        Databases.Instance = databasesGameObject.AddComponent<Databases>();

        Databases.Instance.RecipeDefinitions = new RecipeDefinition[0];
        Databases.Instance.TechnologyDefinitions = new TechnologyDefinition[0];

        using (GameServer server = new GameServer())
        {
            server.Start();
            Thread.Sleep(500);

            Assert.AreEqual(0, server.GameCount);
            using (GameClient client = new GameClient())
            {
                client.Start();

                Assert.AreEqual(0, server.GameCount);

                try
                {
                    var createGameTask = AsyncHelpers.RunSync<byte>(() => client.PostCreateGameOrder(1));
                    AsyncHelpers.RunSync(() => client.PostJoinGameOrder(createGameTask));
                }
                catch (System.Exception exception)
                {
                    Assert.Fail(exception.Message);
                }

                Assert.AreEqual(1, server.GameCount);
                Assert.NotNull(client.Game);
            }
        }
    }

    [Test]
    public void OrderPassesAreRetrievedCorrectly()
    {
        using (GameServer server = new GameServer())
        {
            // Start at 1 to skip invalid order type.
            for (int index = 1; index < server.Test_OrderById.Length; index++)
            {
                var orderData = server.Test_OrderById[index];
                Assert.NotNull(orderData.ServerPass, $"{orderData.Type} order should have a server pass.");
                Assert.AreNotEqual(OrderContext.Invalid, orderData.Context, $"{orderData.Type} order context should not be invalid.");
            }
        }

        using (GameClient client = new GameClient())
        {
            // Start at 1 to skip invalid order type.
            for (int index = 1; index < client.Test_OrderById.Length; index++)
            {
                var orderData = client.Test_OrderById[index];
                Assert.NotNull(orderData.ClientPass, $"{orderData.Type} order should have a client pass.");
                Assert.AreNotEqual(OrderContext.Invalid, orderData.Context, $"{orderData.Type} order context should not be invalid.");
            }
        }
    }
}