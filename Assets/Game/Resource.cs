namespace Game
{
    using System.Collections.Generic;

    [System.Serializable]
    public struct Resource
    {
        public ResourceType Name;
        public Number Amount;
        public Number Net;

        public Number NetFromPreviousTick;
        public Number AmountNeeded;
        public Number AmountToDebit;

        public List<Operation> NetOperations;

        public Resource(ResourceType name, Number amount)
        {
            this.Name = name;
            this.Amount = amount;
            this.Net = Number.Zero;
            this.NetOperations = new List<Operation>();
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

        public void Annotation(string name, Number number)
        {
            if (number == Number.Zero)
            {
                return;
            }

            this.NetOperations.Add(new Operation(name, number));
        }
    }

    public struct Operation
    {
        public string Name;
        public Number Amount;

        public Operation(string name, Number amount)
        {
            this.Name = name;
            this.Amount = amount;
        }
    }
}