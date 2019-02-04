namespace Simulation.Network
{
    using System;

    public class OrderClientPassAttribute : Attribute
    {
        public readonly OrderType OrderType;

        public OrderClientPassAttribute(OrderType orderType)
        {
            this.OrderType = orderType;
        }
    }
}
