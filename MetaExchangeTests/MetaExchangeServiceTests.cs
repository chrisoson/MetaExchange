using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.IO.Compression;
using System.Text.Json;

namespace MetaExchange.Tests
{
    [TestClass()]
    public class MetaExchangeServiceTests
    {
        readonly OrderBook _orderBookOdd = new()
        {
            AcqTime = DateTime.Now,
            Bids = new()
            {
                new() { Order = new() { Id = null, Time = DateTime.MinValue, Type = Type.Buy, Kind = Kind.Limit, Amount = 1, Price = 3010 } },
                new() { Order = new() { Id = null, Time = DateTime.MinValue, Type = Type.Buy, Kind = Kind.Limit, Amount = 3, Price = 3030 } },
            },
            Asks = new()
            {
                new() { Order = new() { Id = null, Time = DateTime.MinValue, Type = Type.Sell, Kind = Kind.Limit, Amount = 1, Price = 3070 } },
                new() { Order = new() { Id = null, Time = DateTime.MinValue, Type = Type.Sell, Kind = Kind.Limit, Amount = 3, Price = 3050 } },
            }
        };

        readonly OrderBook _orderBookEven = new()
        {
            AcqTime = DateTime.Now,
            Bids = new()
            {
                new() { Order = new() { Id = null, Time = DateTime.MinValue, Type = Type.Buy, Kind = Kind.Limit, Amount = 2, Price = 3020 } },
                new() { Order = new() { Id = null, Time = DateTime.MinValue, Type = Type.Buy, Kind = Kind.Limit, Amount = 4, Price = 3040 } },
            },
            Asks = new()
            {
                new() { Order = new() { Id = null, Time = DateTime.MinValue, Type = Type.Sell, Kind = Kind.Limit, Amount = 2, Price = 3080 } },
                new() { Order = new() { Id = null, Time = DateTime.MinValue, Type = Type.Sell, Kind = Kind.Limit, Amount = 4, Price = 3060 } },
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
            string jsonString = JsonSerializer.Serialize(_orderBookOdd);
            File.WriteAllText(fileName, jsonString);

            MetaExchangeService metaExchange = new()
            {
                OrderBooksFilePath = fileName,
                NuberOfOrderBooksToRead = 1
            };
            metaExchange.TryReadOrderBooksFile();

            Assert.IsTrue(metaExchange.CryptoExchanges.Count == 1);
            Assert.AreEqual(metaExchange.CryptoExchanges[0].OrderBook.Bids.Count, _orderBookOdd.Bids.Count);
            Assert.AreEqual(metaExchange.CryptoExchanges[0].OrderBook.Asks.Count, _orderBookOdd.Asks.Count);
        }

        [TestMethod]
        public void BuyTest_OneOrder()
        {
            MetaExchangeService metaExchange = new()
            {
                CryptoExchanges = new()
                {
                    new() { Money = 9000, Cryptocurrency = 3, OrderBook = _orderBookOdd },
                    new() { Money = 9000, Cryptocurrency = 3, OrderBook = _orderBookEven },
                }
            };

            var result = metaExchange.Buy(1);

            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result[0].amount == 1);
            Assert.IsTrue(result[0].order.Price == 3050);
            Assert.IsTrue(metaExchange.CryptoExchanges[0].Money == 9000 - 3050);
            Assert.IsTrue(metaExchange.CryptoExchanges[0].Cryptocurrency == 4);
            Assert.IsTrue(metaExchange.CryptoExchanges[1].Money == 9000);
            Assert.IsTrue(metaExchange.CryptoExchanges[1].Cryptocurrency == 3);
        }

        [TestMethod]
        public void BuyTest_TwoOrders()
        {
            MetaExchangeService metaExchange = new()
            {
                CryptoExchanges = new()
                {
                    new() { Money = 90000, Cryptocurrency = 3, OrderBook = _orderBookOdd },
                    new() { Money = 90000, Cryptocurrency = 3, OrderBook = _orderBookEven },
                }
            };

            var result = metaExchange.Buy(4);

            Assert.IsTrue(result.Count == 2);
            Assert.IsTrue(result[0].amount == 3);
            Assert.IsTrue(result[0].order.Price == 3050);
            Assert.IsTrue(result[1].amount == 1);
            Assert.IsTrue(result[1].order.Price == 3060);
            Assert.IsTrue(metaExchange.CryptoExchanges[0].Money == 90000 - (3 * 3050));
            Assert.IsTrue(metaExchange.CryptoExchanges[0].Cryptocurrency == 6);
            Assert.IsTrue(metaExchange.CryptoExchanges[1].Money == 90000 - (1 * 3060));
            Assert.IsTrue(metaExchange.CryptoExchanges[1].Cryptocurrency == 4);
        }

        [TestMethod]
        public void BuyTest_NotEnoughMoney()
        {
            MetaExchangeService metaExchange = new()
            {
                CryptoExchanges = new()
                {
                    new() { Money = 9000, Cryptocurrency = 3, OrderBook = _orderBookOdd },
                    new() { Money = 90000, Cryptocurrency = 3, OrderBook = _orderBookEven },
                }
            };

            var result = metaExchange.Buy(8);

            Assert.IsTrue(result.Count == 3);
            Assert.IsTrue(result[0].amount < 3);
            Assert.IsTrue(result[0].order.Price == 3050);
            Assert.IsTrue(result[1].amount == 4);
            Assert.IsTrue(result[1].order.Price == 3060);
            Assert.IsTrue(result[2].amount > 0);
            Assert.IsTrue(result[2].order.Price == 3080);
            Assert.IsTrue(metaExchange.CryptoExchanges[0].Money == 0);
            Assert.IsTrue(metaExchange.CryptoExchanges[0].Cryptocurrency < 6);
            Assert.IsTrue(metaExchange.CryptoExchanges[1].Money < 90000 - (4 * 3060));
            Assert.IsTrue(metaExchange.CryptoExchanges[1].Cryptocurrency > 7);
        }

        [TestMethod]
        public void BuyTest_NotEnoughAsks()
        {
            MetaExchangeService metaExchange = new()
            {
                CryptoExchanges = new()
                {
                    new() { Money = 90000, Cryptocurrency = 3, OrderBook = _orderBookOdd },
                    new() { Money = 90000, Cryptocurrency = 3, OrderBook = _orderBookEven },
                }
            };

            var result = metaExchange.Buy(11);

            Assert.IsTrue(result.Count == 4);
            Assert.IsTrue(result[0].amount == 3);
            Assert.IsTrue(result[0].order.Price == 3050);
            Assert.IsTrue(result[1].amount == 4);
            Assert.IsTrue(result[1].order.Price == 3060);
            Assert.IsTrue(result[2].amount == 1);
            Assert.IsTrue(result[2].order.Price == 3070);
            Assert.IsTrue(result[3].amount == 2);
            Assert.IsTrue(result[3].order.Price == 3080);
            Assert.IsTrue(metaExchange.CryptoExchanges[0].Money == 90000 - (3 * 3050) - (1 * 3070));
            Assert.IsTrue(metaExchange.CryptoExchanges[0].Cryptocurrency == 7);
            Assert.IsTrue(metaExchange.CryptoExchanges[1].Money == 90000 - (4 * 3060) - (2 * 3080));
            Assert.IsTrue(metaExchange.CryptoExchanges[1].Cryptocurrency == 9);
        }

        [TestMethod]
        public void BuyTest_Zero()
        {
            MetaExchangeService metaExchange = new()
            {
                CryptoExchanges = new()
                {
                    new() { Money = 9000, Cryptocurrency = 3, OrderBook = _orderBookOdd },
                    new() { Money = 9000, Cryptocurrency = 3, OrderBook = _orderBookEven },
                }
            };

            var result = metaExchange.Buy(0);

            Assert.IsTrue(result.Count == 0);
            Assert.IsTrue(metaExchange.CryptoExchanges[0].Money == 9000);
            Assert.IsTrue(metaExchange.CryptoExchanges[0].Cryptocurrency == 3);
            Assert.IsTrue(metaExchange.CryptoExchanges[1].Money == 9000);
            Assert.IsTrue(metaExchange.CryptoExchanges[1].Cryptocurrency == 3);
        }

        [TestMethod]
        public void BuyTest_Negative()
        {
            MetaExchangeService metaExchange = new()
            {
                CryptoExchanges = new()
                {
                    new() { Money = 9000, Cryptocurrency = 3, OrderBook = _orderBookOdd },
                    new() { Money = 9000, Cryptocurrency = 3, OrderBook = _orderBookEven },
                }
            };

            var result = metaExchange.Buy(-1);

            Assert.IsTrue(result.Count == 0);
            Assert.IsTrue(metaExchange.CryptoExchanges[0].Money == 9000);
            Assert.IsTrue(metaExchange.CryptoExchanges[0].Cryptocurrency == 3);
            Assert.IsTrue(metaExchange.CryptoExchanges[1].Money == 9000);
            Assert.IsTrue(metaExchange.CryptoExchanges[1].Cryptocurrency == 3);
        }

        [TestMethod]
        public void SellTest_OneOrder()
        {
            MetaExchangeService metaExchange = new()
            {
                CryptoExchanges = new()
                {
                    new() { Money = 9000, Cryptocurrency = 3, OrderBook = _orderBookOdd },
                    new() { Money = 9000, Cryptocurrency = 3, OrderBook = _orderBookEven },
                }
            };

            var result = metaExchange.Sell(1);

            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result[0].amount == 1);
            Assert.IsTrue(result[0].order.Price == 3040);
            Assert.IsTrue(metaExchange.CryptoExchanges[0].Money == 9000);
            Assert.IsTrue(metaExchange.CryptoExchanges[0].Cryptocurrency == 3);
            Assert.IsTrue(metaExchange.CryptoExchanges[1].Money == 9000 + 3040);
            Assert.IsTrue(metaExchange.CryptoExchanges[1].Cryptocurrency == 2);
        }

        [TestMethod]
        public void SellTest_TwoOrders()
        {
            MetaExchangeService metaExchange = new()
            {
                CryptoExchanges = new()
                {
                    new() { Money = 9000, Cryptocurrency = 9, OrderBook = _orderBookOdd },
                    new() { Money = 9000, Cryptocurrency = 9, OrderBook = _orderBookEven },
                }
            };

            var result = metaExchange.Sell(5);

            Assert.IsTrue(result.Count == 2);
            Assert.IsTrue(result[0].amount == 4);
            Assert.IsTrue(result[0].order.Price == 3040);
            Assert.IsTrue(result[1].amount == 1);
            Assert.IsTrue(result[1].order.Price == 3030);
            Assert.IsTrue(metaExchange.CryptoExchanges[0].Money == 9000 + (1 * 3030));
            Assert.IsTrue(metaExchange.CryptoExchanges[0].Cryptocurrency == 8);
            Assert.IsTrue(metaExchange.CryptoExchanges[1].Money == 9000 + (4 * 3040));
            Assert.IsTrue(metaExchange.CryptoExchanges[1].Cryptocurrency == 5);
        }

        [TestMethod]
        public void SellTest_NotEnoughCryptocurrency()
        {
            MetaExchangeService metaExchange = new()
            {
                CryptoExchanges = new()
                {
                    new() { Money = 9000, Cryptocurrency = 4, OrderBook = _orderBookOdd },
                    new() { Money = 9000, Cryptocurrency = 1, OrderBook = _orderBookEven },
                }
            };

            var result = metaExchange.Sell(6);

            Assert.IsTrue(result.Count == 3);
            Assert.IsTrue(result[0].amount == 1);
            Assert.IsTrue(result[0].order.Price == 3040);
            Assert.IsTrue(result[1].amount == 3);
            Assert.IsTrue(result[1].order.Price == 3030);
            Assert.IsTrue(result[2].amount == 1);
            Assert.IsTrue(result[2].order.Price == 3010);
            Assert.IsTrue(metaExchange.CryptoExchanges[0].Money == 9000 + (3 * 3030) + 3010);
            Assert.IsTrue(metaExchange.CryptoExchanges[0].Cryptocurrency == 0);
            Assert.IsTrue(metaExchange.CryptoExchanges[1].Money == 9000 + 3040);
            Assert.IsTrue(metaExchange.CryptoExchanges[1].Cryptocurrency == 0);
        }

        [TestMethod]
        public void SellTest_NotEnoughBids()
        {
            MetaExchangeService metaExchange = new()
            {
                CryptoExchanges = new()
                {
                    new() { Money = 9000, Cryptocurrency = 9, OrderBook = _orderBookOdd },
                    new() { Money = 9000, Cryptocurrency = 9, OrderBook = _orderBookEven },
                }
            };

            var result = metaExchange.Sell(11);

            Assert.IsTrue(result.Count == 4);
            Assert.IsTrue(result[0].amount == 4);
            Assert.IsTrue(result[0].order.Price == 3040);
            Assert.IsTrue(result[1].amount == 3);
            Assert.IsTrue(result[1].order.Price == 3030);
            Assert.IsTrue(result[2].amount == 2);
            Assert.IsTrue(result[2].order.Price == 3020);
            Assert.IsTrue(result[3].amount == 1);
            Assert.IsTrue(result[3].order.Price == 3010);
            Assert.IsTrue(metaExchange.CryptoExchanges[0].Money == 9000 + (3 * 3030) + (1 * 3010));
            Assert.IsTrue(metaExchange.CryptoExchanges[0].Cryptocurrency == 5);
            Assert.IsTrue(metaExchange.CryptoExchanges[1].Money == 9000 + (4 * 3040) + (2 * 3020));
            Assert.IsTrue(metaExchange.CryptoExchanges[1].Cryptocurrency == 3);
        }

        [TestMethod]
        public void SellTest_Zero()
        {
            MetaExchangeService metaExchange = new()
            {
                CryptoExchanges = new()
                {
                    new() { Money = 9000, Cryptocurrency = 3, OrderBook = _orderBookOdd },
                    new() { Money = 9000, Cryptocurrency = 3, OrderBook = _orderBookEven },
                }
            };

            var result = metaExchange.Sell(0);

            Assert.IsTrue(result.Count == 0);
            Assert.IsTrue(metaExchange.CryptoExchanges[0].Money == 9000);
            Assert.IsTrue(metaExchange.CryptoExchanges[0].Cryptocurrency == 3);
            Assert.IsTrue(metaExchange.CryptoExchanges[1].Money == 9000);
            Assert.IsTrue(metaExchange.CryptoExchanges[1].Cryptocurrency == 3);
        }

        [TestMethod]
        public void SellTest_Negative()
        {
            MetaExchangeService metaExchange = new()
            {
                CryptoExchanges = new()
                {
                    new() { Money = 9000, Cryptocurrency = 3, OrderBook = _orderBookOdd },
                    new() { Money = 9000, Cryptocurrency = 3, OrderBook = _orderBookEven },
                }
            };

            var result = metaExchange.Sell(-1);

            Assert.IsTrue(result.Count == 0);
            Assert.IsTrue(metaExchange.CryptoExchanges[0].Money == 9000);
            Assert.IsTrue(metaExchange.CryptoExchanges[0].Cryptocurrency == 3);
            Assert.IsTrue(metaExchange.CryptoExchanges[1].Money == 9000);
            Assert.IsTrue(metaExchange.CryptoExchanges[1].Cryptocurrency == 3);
        }
    }
}