using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MetaExchange
{
    public class MetaExchangeService
    {
        public string OrderBooksFilePath { get; set; } = @"..\..\..\..\order_books_data.zip";

        public int NuberOfOrderBooksToRead { get; set; } = 3;

        public List<CryptoExchange> CryptoExchanges { get; set; } = new();

        private readonly Random _random = new();

        public void TryReadOrderBooksFile()
        {
            if (!File.Exists(OrderBooksFilePath))
            {
                throw new FileNotFoundException("File not found: " + OrderBooksFilePath);
            }

            if (Path.GetExtension(OrderBooksFilePath) == ".zip")
            {
                using ZipArchive archive = ZipFile.OpenRead(OrderBooksFilePath);

                if (archive.Entries.Count == 1)
                {
                    ZipArchiveEntry archiveEntry = archive.Entries.First();
                    string extractPath = @".\";
                    OrderBooksFilePath = Path.Combine(extractPath, archiveEntry.FullName);
                    archiveEntry.ExtractToFile(OrderBooksFilePath, overwrite: true);
                }
                else
                {
                    throw new InvalidDataException("Zip file does not contain a single file: " + OrderBooksFilePath);
                }
            }

            foreach (string line in File.ReadLines(OrderBooksFilePath))
            {
                int first = line.IndexOf('{');
                int last = line.LastIndexOf('}');

                if (first == -1 || last == -1)
                {
                    throw new InvalidDataException("No JSON found in file: " + OrderBooksFilePath);
                }

                string jsonString = line[first..++last];

                OrderBook? orderBook = JsonSerializer.Deserialize<OrderBook>(jsonString, new JsonSerializerOptions { Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) } });

                if (orderBook != null)
                {
                    CryptoExchanges.Add(new CryptoExchange
                    {
                        Id = CryptoExchanges.Count, 
                        Money = _random.Next(3000, 9000),
                        Cryptocurrency = _random.Next(3, 9),
                        OrderBook = orderBook
                    });
                }

                if (CryptoExchanges.Count == NuberOfOrderBooksToRead)
                    break;
            }
        }

        public List<(CryptoExchange cryptoExchange, Order order, double amount)> Buy(double cryptocurrencyToBuy)
        {
            List<(CryptoExchange cryptoExchange, Order order, double amount)> orders = new();

            foreach (var (cryptoExchange, order) in CryptoExchanges.SelectMany(cryptoExchange => cryptoExchange.OrderBook.AskOrders.Select(order => (cryptoExchange, order))).OrderBy(pair => pair.order.Price))
            {
                if (cryptoExchange.Money > 0 && cryptocurrencyToBuy > 0)
                {
                    double amountToBuy = Math.Min(cryptocurrencyToBuy, order.Amount);

                    double amountCanBuy = Math.Min(amountToBuy, cryptoExchange.Money / order.Price);

                    cryptocurrencyToBuy -= amountCanBuy;

                    cryptoExchange.Cryptocurrency += amountCanBuy;

                    cryptoExchange.Money -= amountCanBuy * order.Price;

                    orders.Add((cryptoExchange, order, amountCanBuy));
                }
                else
                {
                    break;
                }
            }

            return orders;
        }

        public List<(CryptoExchange cryptoExchange, Order order, double amount)> Sell(double cryptocurrencyToSell)
        {
            List<(CryptoExchange cryptoExchange, Order order, double amount)> orders = new();

            foreach (var (cryptoExchange, order) in CryptoExchanges.SelectMany(cryptoExchange => cryptoExchange.OrderBook.BidOrders.Select(order => (cryptoExchange, order))).OrderByDescending(pair => pair.order.Price))
            {
                if (cryptoExchange.Cryptocurrency > 0 && cryptocurrencyToSell > 0)
                {
                    double amountToSell = Math.Min(cryptocurrencyToSell, order.Amount);

                    double amountCanSell = Math.Min(amountToSell, cryptoExchange.Cryptocurrency);

                    cryptocurrencyToSell -= amountCanSell;

                    cryptoExchange.Cryptocurrency -= amountCanSell;

                    cryptoExchange.Money += amountCanSell * order.Price;

                    orders.Add((cryptoExchange, order, amountCanSell));
                }
                else
                {
                    break;
                }
            }

            return orders;
        }
    }
}
