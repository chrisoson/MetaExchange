﻿using System;
using System.IO;
using System.Linq;

namespace MetaExchange
{
    class Program
    {
        static void Main(string[] args)
        {
            MetaExchangeService metaExchange = new();

            if (args.Length > 0)
            {
                if (File.Exists(args[0]))
                    metaExchange.OrderBooksFilePath = args[0];
                else
                    Console.WriteLine("File not found: " + args[0]);
            }

            if (args.Length > 1)
            {
                if (int.TryParse(args[1], out int nuberOfOrderBooksToRead))
                    metaExchange.NuberOfOrderBooksToRead = nuberOfOrderBooksToRead;
                else
                    Console.WriteLine($"{args[1]} is not a valid number!");
            }

            try
            {
                metaExchange.TryReadOrderBooksFile();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }

            ConsoleKeyInfo consoleKeyInfo;

            do
            {
                Console.WriteLine();

                Console.WriteLine($"Money: {metaExchange.CryptoExchanges.Sum(ce => ce.Money)}, Cryptocurrency: {metaExchange.CryptoExchanges.Sum(ce => ce.Cryptocurrency)}");
                Console.WriteLine("Press Escape to quit");
                Console.WriteLine("Press B to buy");
                Console.WriteLine("Press S to sell");

                consoleKeyInfo = Console.ReadKey();

                Console.WriteLine();

                string line;

                switch (consoleKeyInfo.Key)
                {
                    case ConsoleKey.B:
                        {
                            Console.WriteLine("Enter amount of cryptocurrency to buy:");
                            line = Console.ReadLine() ?? string.Empty;

                            if (double.TryParse(line, out double cryptocurrencyToBuy))
                            {
                                foreach (var (cryptoExchange, order, amount) in metaExchange.Buy(cryptocurrencyToBuy))
                                {
                                    Console.WriteLine($"Buy '{amount}' at price of '{order.Price}' from crypto exchange ID '{cryptoExchange.Id}'");
                                }
                            }
                            else
                            {
                                Console.WriteLine($"{line} is not a valid number!");
                            }
                        }
                        break;

                    case ConsoleKey.S:
                        {
                            Console.WriteLine("Enter amount of cryptocurrency to sell:");
                            line = Console.ReadLine() ?? string.Empty;

                            if (double.TryParse(line, out double cryptocurrencyToSell))
                            {
                                foreach (var (cryptoExchange, order, amount) in metaExchange.Sell(cryptocurrencyToSell))
                                {
                                    Console.WriteLine($"Sell '{amount}' at price of '{order.Price}' from crypto exchange ID '{cryptoExchange.Id}'");
                                }
                            }
                            else
                            {
                                Console.WriteLine($"{line} is not a valid number!");
                            }
                        }
                        break;
                }
            }
            while (consoleKeyInfo.Key != ConsoleKey.Escape);
        }
    }
}
