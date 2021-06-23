using System;
using System.Threading;

namespace LevelUpCSharp.Concurrency
{
    class Program
    {
        private static readonly object _syncObject = new object();

        private static Random r = new Random();

        static void Main(string[] args)
        {
            var a = new Thread(WorkA);
            var b = new Thread(WorkB);
            var vault = new Vault<int>();

            a.Start(vault);
            b.Start(vault);

            Console.ReadKey(true);
            Console.WriteLine("Closing...");
        }

        private static void WorkB(object? obj)
        {
            var vault = (Vault<int>)obj;
            object syncObject = new object();

            while (true)
            {
                var found = r.Next(100);

                Console.WriteLine("[P] i have: " + found);
                vault.Put(found);
                Console.WriteLine("[P] stored: " + found);

                Thread.Sleep(r.Next(1, 10) * 1000);
            }
        }

        private static void WorkA(object? obj)
        {
            var vault = (Vault<int>)obj;

            while (true)
            {
                Console.WriteLine("[C] wait");
                var get = vault.Get();
                Console.WriteLine("[C] get:" + get);

                Thread.Sleep(r.Next(1, 10) * 1000);
            }
        }
    }
}
