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

        public double Cryptocurrency { get; set; } = 0.1;

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

            List<OrderBook> OrderBooks = new();

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
    }
}
