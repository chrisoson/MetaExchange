using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MetaExchange
{
    class Program
    {
        static void Main(string[] args)
        {
            string orderBooksFilePath = @"..\..\..\..\order_books_data.zip";

            int nuberOfOrderBooksToRead = 10;

            if (args.Length > 0)
                orderBooksFilePath = args[0];

            if (args.Length > 1 && int.TryParse(args[1], out int arg))
                nuberOfOrderBooksToRead = arg;

            if (!File.Exists(orderBooksFilePath))
            {
                Console.WriteLine("File not found: " + orderBooksFilePath);
                return;
            }

            if (Path.GetExtension(orderBooksFilePath) == ".zip")
            {
                using ZipArchive archive = ZipFile.OpenRead(orderBooksFilePath);

                if (archive.Entries.Count == 1)
                {
                    ZipArchiveEntry archiveEntry = archive.Entries.First();
                    string extractPath = @".\";
                    orderBooksFilePath = Path.Combine(extractPath, archiveEntry.FullName);
                    archiveEntry.ExtractToFile(orderBooksFilePath, overwrite: true);
                }
                else
                {
                    Console.WriteLine("Zip file does not contain a single file: " + orderBooksFilePath);
                    return;
                }
            }

            List<OrderBook> OrderBooks = new();

            foreach (string line in File.ReadLines(orderBooksFilePath))
            {
                int first = line.IndexOf('{');
                int last = line.LastIndexOf('}');

                if (first == -1 || last == -1)
                {
                    Console.WriteLine("No JSON found in file: " + orderBooksFilePath);
                    return;
                }

                string jsonString = line[first..++last];

                OrderBook? orderBook = JsonSerializer.Deserialize<OrderBook>(jsonString, new JsonSerializerOptions { Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) } });

                if (orderBook != null)
                    OrderBooks.Add(orderBook);

                if (OrderBooks.Count == nuberOfOrderBooksToRead)
                    break;
            }

            Balance balance = new() { Money = 9000, Cryptocurrency = 0.1 };

            ConsoleKeyInfo consoleKeyInfo;

            do
            {
                Console.WriteLine($"Money: {balance.Money}, Cryptocurrency {balance.Cryptocurrency}");

                Console.WriteLine("Press Escape to quit");
                Console.WriteLine("Press C to enter balance constraints");
                Console.WriteLine("Press B to buy");
                Console.WriteLine("Press S to sell");

                consoleKeyInfo = Console.ReadKey();

                Console.WriteLine();

                string line = string.Empty;

                switch (consoleKeyInfo.Key)
                {
                    case ConsoleKey.C:

                        Console.WriteLine("Enter money balance:");
                        line = Console.ReadLine() ?? string.Empty;
                        if (double.TryParse(line, out double money))
                            balance.Money = money;

                        Console.WriteLine("Enter cryptocurrency balance:");
                        line = Console.ReadLine() ?? string.Empty;
                        if (double.TryParse(line, out double cryptocurrency))
                            balance.Cryptocurrency = cryptocurrency;

                        break;

                    case ConsoleKey.B:

                        break;

                    case ConsoleKey.S:

                        break;
                }
            }
            while (consoleKeyInfo.Key != ConsoleKey.Escape);
        }
    }
}
