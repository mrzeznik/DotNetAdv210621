﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using LevelUpCSharp.Collections;
using LevelUpCSharp.Helpers;
using LevelUpCSharp.Products;

namespace LevelUpCSharp.Retail
{
    public class Retailer
    {
        private static Retailer _instance;
        private readonly IDictionary<SandwichKind, Queue<Sandwich>> _lines;

        protected Retailer(string name)
        {
            Name = name;
            _lines = InitializeLines();
        }

        public static Retailer Instance => _instance ?? (_instance = new Retailer("Build-in"));

        public event Action<PackingSummary> Packed;
        public event Action<DateTimeOffset, Sandwich> Purchase;

        public string Name { get; }

        public Result<Sandwich> Sell(SandwichKind kind)
        {
            if (_lines.ContainsKey(kind) == false || _lines[kind].Count == 0)
            {
                return Result<Sandwich>.Failed();
            }

            var sandwich = _lines[kind].Dequeue();
            OnPurchase(DateTimeOffset.Now, sandwich);
            return sandwich.ToSuccess();
        }

        public void Pack(IEnumerable<Sandwich> package, string deliver)
        {
            /* use linq to create summary, see constructor for expectations */

            Dictionary<SandwichKind, int> sums = new Dictionary<SandwichKind, int>();
            foreach (var sandwich in package)
            {
                _lines[sandwich.Kind].Enqueue(sandwich);

                if (sums.ContainsKey(sandwich.Kind) == false)
                {
                    sums.Add(sandwich.Kind, 0);
                }

                sums[sandwich.Kind]++;
            }

            var summaryPositions = new List<LineSummary>();

            foreach (var pair in sums)
            {
                summaryPositions.Add(new LineSummary(pair.Key, pair.Value));
            }

            var summary = new PackingSummary(summaryPositions, deliver);
            OnPacked(summary);
        }

        protected virtual void OnPacked(PackingSummary summary)
        {
            Packed?.Invoke(summary);
        }

        protected virtual void OnPurchase(DateTimeOffset time, Sandwich product)
        {
            Purchase?.Invoke(time, product);
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
    }
}
