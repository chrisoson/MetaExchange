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

        public int NuberOfOrderBooksToRead { get; set; } = 10;

        public double Money { get; set; } = 9000;

        public double Cryptocurrency { get; set; } = 3;

        public List<OrderBook> OrderBooks { get; set; } = new();

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
                    OrderBooks.Add(orderBook);

                if (OrderBooks.Count == NuberOfOrderBooksToRead)
                    break;
            }
        }

        public List<(OrderBook orderBook, Order order, double amount)> GetOrders(List<OrderBook> orderBooks, double moneyConstraint, double cryptocurrencyConstraint, Type orderType, double orderAmount)
        {
            OrderBooks = orderBooks;
            Money = moneyConstraint;
            Cryptocurrency = cryptocurrencyConstraint;

            return orderType switch
            {
                Type.Buy => Buy(orderAmount),
                Type.Sell => Sell(orderAmount),
                _ => throw new ArgumentException("Invalid argument: " + nameof(orderType))
            };
        }

        public List<(OrderBook orderBook, Order order, double amount)> Buy(double cryptocurrencyToBuy)
        {
            List<(OrderBook orderBook, Order order, double amount)> orders = new();

            foreach (var (orderBook, order) in OrderBooks.SelectMany(orderBook => orderBook.AskOrders.Select(order => (orderBook, order))).OrderBy(pair => pair.order.Price))
            {
                if (Money > 0 && cryptocurrencyToBuy > 0)
                {
                    double amountToBuy = Math.Min(cryptocurrencyToBuy, order.Amount);

                    double amountCanBuy = Math.Min(amountToBuy, Money / order.Price);

                    cryptocurrencyToBuy -= amountCanBuy;

                    Cryptocurrency += amountCanBuy;

                    Money -= amountCanBuy * order.Price;

                    orders.Add((orderBook, order, amountCanBuy));
                }
                else
                {
                    break;
                }
            }

            return orders;
        }

        public List<(OrderBook orderBook, Order order, double amount)> Sell(double cryptocurrencyToSell)
        {
            List<(OrderBook orderBook, Order order, double amount)> orders = new();

            foreach (var (orderBook, order) in OrderBooks.SelectMany(orderBook => orderBook.BidOrders.Select(order => (orderBook, order))).OrderByDescending(pair => pair.order.Price))
            {
                if (Cryptocurrency > 0 && cryptocurrencyToSell > 0)
                {
                    double amountToSell = Math.Min(cryptocurrencyToSell, order.Amount);

                    double amountCanSell = Math.Min(amountToSell, Cryptocurrency);

                    cryptocurrencyToSell -= amountCanSell;

                    Cryptocurrency -= amountCanSell;

                    Money += amountCanSell * order.Price;

                    orders.Add((orderBook, order, amountCanSell));
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
