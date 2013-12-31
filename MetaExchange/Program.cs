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
            string filePath = @"..\..\..\..\order_books_data.zip";

            if (args.Length > 0)
                filePath = args.First();

            if (!File.Exists(filePath))
            {
                Console.WriteLine("File not found: " + filePath);
                return;
            }

            if (Path.GetExtension(filePath) == ".zip")
            {
                using ZipArchive archive = ZipFile.OpenRead(filePath);

                if (archive.Entries.Count == 1)
                {
                    ZipArchiveEntry archiveEntry = archive.Entries.First();
                    string extractPath = @".\";
                    filePath = Path.Combine(extractPath, archiveEntry.FullName);
                    archiveEntry.ExtractToFile(filePath, overwrite: true);
                }
                else
                {
                    Console.WriteLine("Zip file does not contain a single file: " + filePath);
                    return;
                }
            }

            List<OrderBook> OrderBooks = new();

            foreach (string line in File.ReadLines(filePath))
            {
                int first = line.IndexOf('{');
                int last = line.LastIndexOf('}');

                if (first == -1 || last == -1)
                {
                    Console.WriteLine("No JSON found in file: " + filePath);
                    return;
                }

                string jsonString = line[first..++last];

                OrderBook? orderBook = JsonSerializer.Deserialize<OrderBook>(jsonString, new JsonSerializerOptions { Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) } });

                if (orderBook != null)
                    OrderBooks.Add(orderBook);
            }

            Console.WriteLine($"Found {OrderBooks.Count} order books");
        }
    }
}
