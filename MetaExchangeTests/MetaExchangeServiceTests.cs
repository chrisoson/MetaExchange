using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
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
                new() { Order = new() { Id = 1, Time = DateTime.Now, Type = Type.Buy, Kind = Kind.Limit, Amount = 1, Price = 3010 } },
                new() { Order = new() { Id = 2, Time = DateTime.Now, Type = Type.Buy, Kind = Kind.Limit, Amount = 2, Price = 3020 } },
                new() { Order = new() { Id = 3, Time = DateTime.Now, Type = Type.Buy, Kind = Kind.Limit, Amount = 3, Price = 3030 } },
            },
            Asks = new()
            {
                new() { Order = new() { Id = 4, Time = DateTime.Now, Type = Type.Sell, Kind = Kind.Limit, Amount = 1, Price = 3060 } },
                new() { Order = new() { Id = 5, Time = DateTime.Now, Type = Type.Sell, Kind = Kind.Limit, Amount = 2, Price = 3050 } },
                new() { Order = new() { Id = 6, Time = DateTime.Now, Type = Type.Sell, Kind = Kind.Limit, Amount = 3, Price = 3040 } },
            }
        };

        [TestMethod()]
        [ExpectedException(typeof(FileNotFoundException))]
        public void TryReadOrderBooksFileTest_FileNotFoundException()
        {
            MetaExchangeService metaExchange = new()
            {
                OrderBooksFilePath = string.Empty,
                NuberOfOrderBooksToRead = 1
            };
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

            MetaExchangeService metaExchange = new()
            {
                OrderBooksFilePath = zipFile,
                NuberOfOrderBooksToRead = 1
            };
            metaExchange.TryReadOrderBooksFile();
        }

        [TestMethod()]
        public void TryReadOrderBooksFileTest()
        {
            string fileName = "test.json";
            string jsonString = JsonSerializer.Serialize(orderBook);
            File.WriteAllText(fileName, jsonString);

            MetaExchangeService metaExchange = new()
            {
                OrderBooksFilePath = fileName,
                NuberOfOrderBooksToRead = 1
            };
            metaExchange.TryReadOrderBooksFile();

            Assert.IsTrue(metaExchange.CryptoExchanges.Count == 1);
            Assert.AreEqual(metaExchange.CryptoExchanges.First().OrderBook.Bids.Count, orderBook.Bids.Count);
            Assert.AreEqual(metaExchange.CryptoExchanges.First().OrderBook.Asks.Count, orderBook.Asks.Count);
        }

        [TestMethod]
        public void BuyTest_OneOrder()
        {
            MetaExchangeService metaExchange = new()
            {
                CryptoExchanges = new()
                {
                    new()
                    {
                        Money = 9000,
                        Cryptocurrency = 3,
                        OrderBook = orderBook
                    }
                }
            };

            var result = metaExchange.Buy(1);

            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result[0].amount == 1);
            Assert.IsTrue(result[0].order.Price == 3040);
            Assert.IsTrue(metaExchange.CryptoExchanges.First().Money == 9000 - 3040);
            Assert.IsTrue(metaExchange.CryptoExchanges.First().Cryptocurrency == 4);
        }

        [TestMethod]
        public void BuyTest_TwoOrders()
        {
            MetaExchangeService metaExchange = new()
            {
                CryptoExchanges = new()
                {
                    new()
                    {
                        Money = 90000,
                        Cryptocurrency = 3,
                        OrderBook = orderBook
                    }
                }
            };

            var result = metaExchange.Buy(4);

            Assert.IsTrue(result.Count == 2);
            Assert.IsTrue(result[0].amount == 3);
            Assert.IsTrue(result[0].order.Price == 3040);
            Assert.IsTrue(result[1].amount == 1);
            Assert.IsTrue(result[1].order.Price == 3050);
            Assert.IsTrue(metaExchange.CryptoExchanges.First().Money == 90000 - (3 * 3040) - (1 * 3050));
            Assert.IsTrue(metaExchange.CryptoExchanges.First().Cryptocurrency == 7);
        }

        [TestMethod]
        public void BuyTest_NotEnoughMoney()
        {
            MetaExchangeService metaExchange = new()
            {
                CryptoExchanges = new()
                {
                    new()
                    {
                        Money = 3000,
                        Cryptocurrency = 3,
                        OrderBook = orderBook
                    }
                }
            };

            var result = metaExchange.Buy(1);

            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result[0].amount < 1);
            Assert.IsTrue(result[0].order.Price == 3040);
            Assert.IsTrue(metaExchange.CryptoExchanges.First().Money == 0);
            Assert.IsTrue(metaExchange.CryptoExchanges.First().Cryptocurrency > 3);
        }

        [TestMethod]
        public void BuyTest_NotEnoughAsks()
        {
            MetaExchangeService metaExchange = new()
            {
                CryptoExchanges = new()
                {
                    new()
                    {
                        Money = 90000,
                        Cryptocurrency = 3,
                        OrderBook = orderBook
                    }
                }
            };

            var result = metaExchange.Buy(7);

            Assert.IsTrue(result.Count == 3);
            Assert.IsTrue(result[0].amount == 3);
            Assert.IsTrue(result[0].order.Price == 3040);
            Assert.IsTrue(result[1].amount == 2);
            Assert.IsTrue(result[1].order.Price == 3050);
            Assert.IsTrue(result[2].amount == 1);
            Assert.IsTrue(result[2].order.Price == 3060);
            Assert.IsTrue(metaExchange.CryptoExchanges.First().Money == 90000 - (3 * 3040) - (2 * 3050) - (1 * 3060));
            Assert.IsTrue(metaExchange.CryptoExchanges.First().Cryptocurrency == 9);
        }

        [TestMethod]
        public void BuyTest_Zero()
        {
            MetaExchangeService metaExchange = new()
            {
                CryptoExchanges = new()
                {
                    new()
                    {
                        Money = 9000,
                        Cryptocurrency = 3,
                        OrderBook = orderBook
                    }
                }
            };

            var result = metaExchange.Buy(0);

            Assert.IsTrue(result.Count == 0);
            Assert.IsTrue(metaExchange.CryptoExchanges.First().Money == 9000);
            Assert.IsTrue(metaExchange.CryptoExchanges.First().Cryptocurrency == 3);
        }

        [TestMethod]
        public void BuyTest_Negative()
        {
            MetaExchangeService metaExchange = new()
            {
                CryptoExchanges = new()
                {
                    new()
                    {
                        Money = 9000,
                        Cryptocurrency = 3,
                        OrderBook = orderBook
                    }
                }
            };

            var result = metaExchange.Buy(-1);

            Assert.IsTrue(result.Count == 0);
            Assert.IsTrue(metaExchange.CryptoExchanges.First().Money == 9000);
            Assert.IsTrue(metaExchange.CryptoExchanges.First().Cryptocurrency == 3);
        }

        [TestMethod]
        public void SellTest_OneOrder()
        {
            MetaExchangeService metaExchange = new()
            {
                CryptoExchanges = new()
                {
                    new()
                    {
                        Money = 9000,
                        Cryptocurrency = 3,
                        OrderBook = orderBook
                    }
                }
            };

            var result = metaExchange.Sell(1);

            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result[0].amount == 1);
            Assert.IsTrue(result[0].order.Price == 3030);
            Assert.IsTrue(metaExchange.CryptoExchanges.First().Money == 9000 + 3030);
            Assert.IsTrue(metaExchange.CryptoExchanges.First().Cryptocurrency == 2);
        }

        [TestMethod]
        public void SellTest_TwoOrders()
        {
            MetaExchangeService metaExchange = new()
            {
                CryptoExchanges = new()
                {
                    new()
                    {
                        Money = 9000,
                        Cryptocurrency = 9,
                        OrderBook = orderBook
                    }
                }
            };

            var result = metaExchange.Sell(4);

            Assert.IsTrue(result.Count == 2);
            Assert.IsTrue(result[0].amount == 3);
            Assert.IsTrue(result[0].order.Price == 3030);
            Assert.IsTrue(result[1].amount == 1);
            Assert.IsTrue(result[1].order.Price == 3020);
            Assert.IsTrue(metaExchange.CryptoExchanges.First().Money == 9000 + (3 * 3030) + (1 * 3020));
            Assert.IsTrue(metaExchange.CryptoExchanges.First().Cryptocurrency == 5);
        }

        [TestMethod]
        public void SellTest_NotEnoughCryptocurrency()
        {
            MetaExchangeService metaExchange = new()
            {
                CryptoExchanges = new()
                {
                    new()
                    {
                        Money = 9000,
                        Cryptocurrency = 1,
                        OrderBook = orderBook
                    }
                }
            };

            var result = metaExchange.Sell(2);

            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result[0].amount == 1);
            Assert.IsTrue(result[0].order.Price == 3030);
            Assert.IsTrue(metaExchange.CryptoExchanges.First().Money == 9000 + 3030);
            Assert.IsTrue(metaExchange.CryptoExchanges.First().Cryptocurrency == 0);
        }

        [TestMethod]
        public void SellTest_NotEnoughBids()
        {
            MetaExchangeService metaExchange = new()
            {
                CryptoExchanges = new()
                {
                    new()
                    {
                        Money = 9000,
                        Cryptocurrency = 9,
                        OrderBook = orderBook
                    }
                }
            };

            var result = metaExchange.Sell(7);

            Assert.IsTrue(result.Count == 3);
            Assert.IsTrue(result[0].amount == 3);
            Assert.IsTrue(result[0].order.Price == 3030);
            Assert.IsTrue(result[1].amount == 2);
            Assert.IsTrue(result[1].order.Price == 3020);
            Assert.IsTrue(result[2].amount == 1);
            Assert.IsTrue(result[2].order.Price == 3010);
            Assert.IsTrue(metaExchange.CryptoExchanges.First().Money == 9000 + (3 * 3030) + (2 * 3020) + (1 * 3010));
            Assert.IsTrue(metaExchange.CryptoExchanges.First().Cryptocurrency == 3);
        }

        [TestMethod]
        public void SellTest_Zero()
        {
            MetaExchangeService metaExchange = new()
            {
                CryptoExchanges = new()
                {
                    new()
                    {
                        Money = 9000,
                        Cryptocurrency = 3,
                        OrderBook = orderBook
                    }
                }
            };

            var result = metaExchange.Sell(0);

            Assert.IsTrue(result.Count == 0);
            Assert.IsTrue(metaExchange.CryptoExchanges.First().Money == 9000);
            Assert.IsTrue(metaExchange.CryptoExchanges.First().Cryptocurrency == 3);
        }

        [TestMethod]
        public void SellTest_Negative()
        {
            MetaExchangeService metaExchange = new()
            {
                CryptoExchanges = new()
                {
                    new()
                    {
                        Money = 9000,
                        Cryptocurrency = 3,
                        OrderBook = orderBook
                    }
                }
            };

            var result = metaExchange.Sell(-1);

            Assert.IsTrue(result.Count == 0);
            Assert.IsTrue(metaExchange.CryptoExchanges.First().Money == 9000);
            Assert.IsTrue(metaExchange.CryptoExchanges.First().Cryptocurrency == 3);
        }
    }
}