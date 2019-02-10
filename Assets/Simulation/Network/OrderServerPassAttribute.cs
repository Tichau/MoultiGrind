namespace Simulation.Network
{
    using System;

    public class OrderServerPassAttribute : Attribute
    {
        public readonly OrderType OrderType;

        public OrderServerPassAttribute(OrderType orderType)
        {
            this.OrderType = orderType;
        }
    }
}
