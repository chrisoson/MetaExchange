using System;

namespace MetaExchange
{
    class Program
    {
        static void Main(string[] args)
        {
            MetaExchangeService metaExchange = new();

            if (args.Length > 0)
                metaExchange.OrderBooksFilePath = args[0];

            if (args.Length > 1 && int.TryParse(args[1], out int arg))
                metaExchange.NuberOfOrderBooksToRead = arg;

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
                Console.WriteLine($"Money: {metaExchange.Money}, Cryptocurrency {metaExchange.Cryptocurrency}");

                Console.WriteLine("Press Escape to quit");
                Console.WriteLine("Press C to enter balance constraints");
                Console.WriteLine("Press B to buy");
                Console.WriteLine("Press S to sell");

                consoleKeyInfo = Console.ReadKey();

                Console.WriteLine();

                string line;

                switch (consoleKeyInfo.Key)
                {
                    case ConsoleKey.C:

                        Console.WriteLine("Enter money balance:");
                        line = Console.ReadLine() ?? string.Empty;
                        if (double.TryParse(line, out double money))
                            metaExchange.Money = money;

                        Console.WriteLine("Enter cryptocurrency balance:");
                        line = Console.ReadLine() ?? string.Empty;
                        if (double.TryParse(line, out double cryptocurrency))
                            metaExchange.Cryptocurrency = cryptocurrency;

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
