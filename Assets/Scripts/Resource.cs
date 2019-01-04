[System.Serializable]
public struct Resource
{
    public ResourceType Name;
    public float Amount;

    public Resource(ResourceType name, float amount)
    {
        this.Name = name;
        this.Amount = amount;
    }
}