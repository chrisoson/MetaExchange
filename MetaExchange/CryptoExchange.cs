namespace MetaExchange
{
    public class CryptoExchange
    {
        public int Id { get; set; }

        public double Money { get; set; } = 9000;

        public double Cryptocurrency { get; set; } = 3;

        public OrderBook OrderBook { get; set; } = new();
    }
}
