using System;
using System.Collections.Generic;

namespace MetaExchange
{
    public class Bid
    {
        public Order Order { get; set; } = new();
    }

    public class Ask
    {
        public Order Order { get; set; } = new();
    }

    public class OrderBook
    {
        public DateTime AcqTime { get; set; }
        public List<Bid> Bids { get; set; } = new();
        public List<Ask> Asks { get; set; } = new();
    }
}
