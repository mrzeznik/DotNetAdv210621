using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using LevelUpCSharp.Collections;
using LevelUpCSharp.Products;

namespace LevelUpCSharp.Production
{
    public class Vendor
    {
        private readonly ConcurrentQueue<Sandwich> _warehouse = new ConcurrentQueue<Sandwich>();

        private readonly ConcurrentQueue<PendingOrder> _orders = new ConcurrentQueue<PendingOrder>();

        private static Random _generator = new Random();

        private readonly Thread _worker = null;
        private bool _ending = false;

        public Vendor(string name)
        {
            Name = name;
            _worker = new Thread(Production) { IsBackground = true };
            _worker.Start();
        }

        public event Action<Sandwich[]> Produced;

        public string Name { get; }

        public IEnumerable<Sandwich> Buy(int howMuch = 0)
        {
            if (howMuch == 0)
            {
                howMuch = _warehouse.Count;
            }

            int index = 0;
            var toSell = new List<Sandwich>();
            while (_warehouse.TryDequeue(out var s) && howMuch > index)
            {
                toSell.Add(s);
                index++;
            }

            return toSell;
        }

        public void Order(SandwichKind kind, int count)
        {
            _orders.Enqueue(new PendingOrder(kind, count));
        }

        public IEnumerable<StockItem> GetStock()
        {
            Dictionary<SandwichKind, int> counts = new Dictionary<SandwichKind, int>()
            {
                {SandwichKind.Cheese, 0},
                {SandwichKind.Chicken, 0},
                {SandwichKind.Beef, 0},
                {SandwichKind.Pork, 0},
            };

            lock (_warehouse)
            {
                foreach (var sandwich in _warehouse)
                {
                    counts[sandwich.Kind] += 1;
                }
            }

            var result = new StockItem[counts.Count];

            int i = 0;
            foreach (var count in counts)
            {
                result[i] = new StockItem(count.Key, count.Value);
                i++;
            }

            return result;
        }
        
        public void Dispose()
        {
            _ending = true;
        }

        private Sandwich Produce(SandwichKind kind)
        {
            return kind switch
            {
                SandwichKind.Beef => ProduceSandwich(kind, DateTimeOffset.Now.AddMinutes(3)),
                SandwichKind.Cheese => ProduceSandwich(kind, DateTimeOffset.Now.AddSeconds(90)),
                SandwichKind.Chicken => ProduceSandwich(kind, DateTimeOffset.Now.AddMinutes(4)),
                SandwichKind.Pork => ProduceSandwich(kind, DateTimeOffset.Now.AddSeconds(150)),
                _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
            };
        }

        private Sandwich ProduceSandwich(SandwichKind kind, DateTimeOffset addMinutes)
        {
            IKeyIngredient main = kind switch
            {
                SandwichKind.Cheese => new Cheese(),
                SandwichKind.Beef => new PulledBeef(),
                SandwichKind.Chicken => new GrilledChicken(),
                SandwichKind.Pork => new Ham(),
            };

            var sandwichBuilder = new SandwichBuilder();
            var sandwich = sandwichBuilder
                .AddMainIngredient(main)
                .AddEstra(new Cheese())
                .AddEstra(new Olives())
                .AddEstra(new Tomato())
                .AddEstra(new Onion())
                .AddEstra(new Onion())
                .AddEstra(new Tomato())
                .AddSos(new Mayo())
                .Wrap();

            return sandwich;
        }

        private class PendingOrder
        {
            public PendingOrder(SandwichKind kind, int amount)
            {
                Kind = kind;
                Amount = amount;
            }

            public SandwichKind Kind { get; }

            public int Amount { get; }
        }

        private void Production()
        {
            while (_ending == false)
            {
                var orderPresent = _orders.TryDequeue(out PendingOrder order);

                if (!orderPresent)
                {
                    order = new PendingOrder((SandwichKind)_generator.Next(1, 4), 10);
                    _orders.Enqueue(order);
                }

                // substitute for lines till .ForEach usage
                //var s = _orders.AsParallel().SelectMany(o => 
                //    Enumerable.Range(0, order.Amount).AsParallel().Select(_ => Produce(order.Kind))).ToArray();

                var tasks = new Task<Sandwich>[order.Amount];

                for (int a = 0; a < order.Amount; a++)
                {
                    var worker = new Task<Sandwich>(() => Produce(order.Kind));
                    worker.Start();
                    tasks[a] = worker;
                }

                Task.WaitAll(tasks);

                var sandwiches = tasks.Select(task => task.Result).ToArray();

                sandwiches.ForEach(sandwich => _warehouse.Enqueue(sandwich));

                Produced?.Invoke(sandwiches);

                Thread.Sleep(_generator.Next(1, 30) * 100);
            }
        }
    }
}
