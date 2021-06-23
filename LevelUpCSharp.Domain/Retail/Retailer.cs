﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using LevelUpCSharp.Collections;
using LevelUpCSharp.Helpers;
using LevelUpCSharp.Products;

namespace LevelUpCSharp.Retail
{
    public class Retailer
    {
        private static Retailer _instance;
        private readonly ISandwichesRack<SandwichKind, Sandwich> _rack;

        protected Retailer(string name)
        {
            Name = name;
            _rack = InitializeRack();
        }
        
        public static Retailer Instance => _instance ?? (_instance = new Retailer("Build-in"));

        public event Action<PackingSummary> Packed;
        public event Action<DateTimeOffset, Sandwich> Purchase;

        public string Name { get; }

        public Result<Sandwich> Sell(SandwichKind kind)
        {
            var hasKind = _rack.Contains(kind);
            
            if (!hasKind)
            {
                return Result<Sandwich>.Failed();
            }
            
            var sandwich = _rack.Get(kind);

            new List<Sandwich>(_rack);

            OnPurchase(DateTimeOffset.Now, sandwich);
            return sandwich.ToSuccess();
        }

        public void Pack(IEnumerable<Sandwich> package, string deliver)
        {
            package = package.ToArray();
            PopulateRack(package);
            var summary = ComputeLabel(package, deliver);
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

        private ISandwichesRack<SandwichKind, Sandwich> InitializeRack()
        {
            return new SandwichesRack();
        }
        
        private void PopulateRack(IEnumerable<Sandwich> package)
        {
            package.ForEach(p => _rack.Add(p));
        }

        private static PackingSummary ComputeLabel(IEnumerable<Sandwich> package, string deliver)
        {
            var summaryPositions = package
                .GroupBy(
                    p => p.Kind,
                    (kind, sandwiches) => new LineSummary(kind, sandwiches.Count()))
                .ToArray();

            var summary = new PackingSummary(summaryPositions, deliver);
            return summary;
        }
    }
}
