using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using LevelUpCSharp.Products;

namespace LevelUpCSharp.Production
{
    public class Vendor
    {
        private readonly List<Sandwich> _warehouse = new List<Sandwich>();

        private readonly List<PendingOrder> _orders = new List<PendingOrder>();

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
            var toSell = new List<Sandwich>();

            lock (_warehouse)
            {
                if (_warehouse.Count == 0)
                {
                    return Array.Empty<Sandwich>();
                }

                if (howMuch == 0 || _warehouse.Count <= howMuch)
                {
                    var result = _warehouse.ToArray();
                    _warehouse.Clear();
                    return result;
                }

                for (int i = 0; i < howMuch; i++)
                {
                    var first = _warehouse[0];
                    toSell.Add(first);
                    _warehouse.Remove(first);
                } 
            }

            return toSell;
        }

        public void Order(SandwichKind kind, int count)
        {
            lock (_orders)
            {
                _orders.Add(new PendingOrder(kind, count));
            }
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
                .Add(main)
                .AddExtra(new Cheese())
                .AddExtra(new Olives())
                .AddExtra(new Tomato())
                .AddExtra(new Onion())
                .AddExtra(new Onion())
                .AddExtra(new Tomato())
                .Add(new Mayo()).Wrap();

            sandwichBuilder.Add(new Fish()).AddExtra(new Olives()).Wrap();

            return sandwich;

            //return new Sandwich(kind, addMinutes);
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
                var sandwich = Produce(SandwichKind.Beef);

                lock (_warehouse)
                {
                    _warehouse.Add(sandwich);
                }

                Produced?.Invoke(new[] { sandwich });

                IEnumerable<PendingOrder> sideWork;
                lock (_orders)
                {
                    sideWork = _orders.ToArray();
                    _orders.Clear();
                }

                foreach (var pendingOrder in sideWork)
                {
                    var sandwiches = new List<Sandwich>();

                    for (int i = 0; i < pendingOrder.Amount; i++)
                    {
                        sandwich = Produce(pendingOrder.Kind);

                        lock (_warehouse)
                        {
                            _warehouse.Add(sandwich);
                        }

                        sandwiches.Add(sandwich);
                        Thread.Sleep(1 * 1000);
                    }

                    Produced?.Invoke(sandwiches.ToArray());

                    Thread.Sleep(2 * 1000);
                }

                Thread.Sleep(5 * 1000);
            }
        }
    }
}
