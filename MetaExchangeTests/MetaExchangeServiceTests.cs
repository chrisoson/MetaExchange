using Microsoft.VisualStudio.TestTools.UnitTesting;
using MetaExchange;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;
using System.Text.Json;

namespace MetaExchange.Tests
{
    [TestClass()]
    public class MetaExchangeServiceTests
    {
        readonly OrderBook orderBook = new()
        {
            AcqTime = DateTime.Now,
            Bids = new()
            {
                new Bid() { Order = new Order() { Id = null, Time = DateTime.MinValue, Type = Type.Buy, Kind = Kind.Limit, Amount = 0.01, Price = 2960.64 } },
                new Bid() { Order = new Order() { Id = null, Time = DateTime.MinValue, Type = Type.Buy, Kind = Kind.Limit, Amount = 0.01, Price = 2960.64 } },
                new Bid() { Order = new Order() { Id = null, Time = DateTime.MinValue, Type = Type.Buy, Kind = Kind.Limit, Amount = 0.01, Price = 2960.64 } },
            },
            Asks = new()
            {
                new Ask() { Order = new Order() { Id = null, Time = DateTime.MinValue, Type = Type.Sell, Kind = Kind.Limit, Amount = 0.01, Price = 2960.64 } },
                new Ask() { Order = new Order() { Id = null, Time = DateTime.MinValue, Type = Type.Sell, Kind = Kind.Limit, Amount = 0.01, Price = 2960.64 } },
                new Ask() { Order = new Order() { Id = null, Time = DateTime.MinValue, Type = Type.Sell, Kind = Kind.Limit, Amount = 0.01, Price = 2960.64 } },
            }
        };

        [TestMethod()]
        [ExpectedException(typeof(FileNotFoundException))]
        public void TryReadOrderBooksFileTest_FileNotFoundException()
        {
            MetaExchangeService metaExchange = new();
            metaExchange.OrderBooksFilePath = string.Empty;
            metaExchange.TryReadOrderBooksFile();
        }

        [TestMethod()]
        [ExpectedException(typeof(InvalidDataException))]
        public void TryReadOrderBooksFileTest_InvalidDataException()
        {
            string zipFile = "test.zip";

            if (File.Exists(zipFile))
                File.Delete(zipFile);

            ZipArchive zip = ZipFile.Open(zipFile, ZipArchiveMode.Create);
            zip.Dispose();

            MetaExchangeService metaExchange = new();
            metaExchange.OrderBooksFilePath = zipFile;
            metaExchange.TryReadOrderBooksFile();
        }

        [TestMethod()]
        public void TryReadOrderBooksFileTest()
        {
            string fileName = "test.json";
            string jsonString = JsonSerializer.Serialize(orderBook);
            File.WriteAllText(fileName, jsonString);

            MetaExchangeService metaExchange = new();
            metaExchange.OrderBooksFilePath = fileName;
            metaExchange.TryReadOrderBooksFile();

            Assert.IsTrue(metaExchange.OrderBooks.Count == 1);
            Assert.AreEqual(metaExchange.OrderBooks.First().Bids.Count, orderBook.Bids.Count);
            Assert.AreEqual(metaExchange.OrderBooks.First().Asks.Count, orderBook.Asks.Count);
        }

        [TestMethod]
        public void BuyTest()
        {
            MetaExchangeService metaExchange = new();
            metaExchange.OrderBooks.Add(orderBook);
            var orders = metaExchange.Buy(1);

            Assert.IsTrue(orders.Any());
        }

        [TestMethod]
        public void SellTest()
        {
            MetaExchangeService metaExchange = new();
            metaExchange.OrderBooks.Add(orderBook);
            var orders = metaExchange.Sell(1);

            Assert.IsTrue(orders.Any());
        }
    }
}