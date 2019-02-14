namespace Simulation.Network
{
    public enum OrderType : byte
    {
        Invalid = 0,

        CreateGame,
        JoinGame,
        LeaveGame,

        ChangeGameSpeed,

        CraftRecipe,
        CreateFactory,
        DestroyFactory,
        ResearchTechnology,

        // Cheat
        CreditResources,
    }
}
