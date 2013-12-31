using System;

namespace MetaExchange
{
    public enum Kind
    {
        Limit
    }

    public enum Type
    {
        Buy,
        Sell
    }

    public class Order
    {
        public object? Id { get; set; }
        public DateTime Time { get; set; }
        public Type Type { get; set; }
        public Kind Kind { get; set; }
        public double Amount { get; set; }
        public double Price { get; set; }
    }
}
