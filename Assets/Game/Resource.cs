[System.Serializable]
public struct Resource
{
    public ResourceType Name;
    public Number Amount;
    public Number Net;

    public Number NetFromPreviousTick;
    public Number AmountNeeded;

    public Resource(ResourceType name, Number amount)
    {
        this.Name = name;
        this.Amount = amount;
        this.Net = Number.Zero;
    }

    public Number SpendableAmount => this.Amount + this.NetFromPreviousTick;

    public Number SpendableRatio => Number.Min(this.AmountNeeded, this.SpendableAmount) / this.SpendableAmount;
}