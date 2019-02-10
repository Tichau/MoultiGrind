namespace Simulation.Network
{
    public enum OrderType : byte
    {
        Invalid = 0,

        CreateGame,
        JoinGame,

        ChangeGameSpeed,

        CraftRecipe,
        CreateFactory,
        DestroyFactory,
        ResearchTechnology,

        // Cheat
        CreditResources,
    }
}
