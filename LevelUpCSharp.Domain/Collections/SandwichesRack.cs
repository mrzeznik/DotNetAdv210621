using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LevelUpCSharp.Helpers;
using LevelUpCSharp.Products;

namespace LevelUpCSharp.Collections
{
    internal class SandwichesRack : ISandwichesRack<SandwichKind, Sandwich>
    {
        private int _amount = 0;
        private readonly IDictionary<SandwichKind, Queue<Sandwich>> _lines;
        private readonly Comparison<Sandwich> _comparison;

        public SandwichesRack()
        :this(Enumerable.Empty<Sandwich>(), DefaultComparison)
        {
        }

        public SandwichesRack(Comparison<Sandwich> comparison)
        :this(Enumerable.Empty<Sandwich>(), comparison)
        {
        }

        public SandwichesRack(IEnumerable<Sandwich> source) 
            : this(source, DefaultComparison)
        {
        }

        public SandwichesRack(IEnumerable<Sandwich> source, Comparison<Sandwich> comparison)
        {
            _comparison = comparison;
            _lines = InitializeLines();

            source.GroupBy(sandwich => sandwich.Kind)
                .ForEach(kindGroup => _lines[kindGroup.Key] = new Queue<Sandwich>(kindGroup));
        }

        public void Add(Sandwich sandwich)
        {
            _lines[sandwich.Kind].Enqueue(sandwich);
            _amount++;
        }

        public Sandwich Get(SandwichKind kind)
        {
            _amount--;
            return _lines[kind].Dequeue();
        }

        public bool Contains(SandwichKind kind)
        {
            return _lines.ContainsKey(kind) && _lines[kind].Count > 0;
        }

        public IEnumerator<Sandwich> GetEnumerator()
        {
            var allSandwiches = AggregateAllSandwiches();
            allSandwiches.Sort(_comparison);
            return new Enumerator(allSandwiches);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private static int DefaultComparison(Sandwich x, Sandwich y)
        {
            return x.ExpirationDate.CompareTo(y.ExpirationDate);
        }

        private IDictionary<SandwichKind, Queue<Sandwich>> InitializeLines()
        {
            var result = new Dictionary<SandwichKind, Queue<Sandwich>>();

            foreach (var sandwichKind in EnumHelper.GetValues<SandwichKind>())
            {
                result.Add(sandwichKind, new Queue<Sandwich>());
            }

            return result;
        }

        private List<Sandwich> AggregateAllSandwiches()
        {
            List<Sandwich> result = new List<Sandwich>(_amount);

            foreach (var line in _lines)
            {
                result.AddRange(line.Value);
            }

            return result;
        }

        internal class Enumerator : IEnumerator<Sandwich>
        {
            private readonly IEnumerator<Sandwich> _sandwiches;

            public Enumerator(List<Sandwich> sandwiches)
            {
                _sandwiches = sandwiches.GetEnumerator();
            }


            public bool MoveNext()
            {
                return _sandwiches.MoveNext();
            }

            public void Reset()
            {
                _sandwiches.Reset();
            }

            public Sandwich Current => _sandwiches.Current;

            object IEnumerator.Current => Current;

            public void Dispose()
            {
                _sandwiches.Dispose();
            }
        }
    }
}