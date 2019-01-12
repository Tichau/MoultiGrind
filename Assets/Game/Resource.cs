[System.Serializable]
public struct Resource
{
    public ResourceType Name;
    public Number Amount;
    public Number Net;

    public Number NetFromPreviousTick;
    public Number AmountNeeded;
    public Number AmountToDebit;

    public Resource(ResourceType name, Number amount)
    {
        this.Name = name;
        this.Amount = amount;
        this.Net = Number.Zero;
    }

    public Number AmountToSpend => Number.Min(this.AmountNeeded, this.SpendableAmount);

    public Number SpendableAmount => this.Amount + this.NetFromPreviousTick;

    public Number SpendableNeededAmountPercent
    {
        get
        {
            if (this.AmountNeeded == Number.Zero)
            {
                return new Number(1);
            }

            return this.AmountToSpend / this.AmountNeeded;
        }
    }
}