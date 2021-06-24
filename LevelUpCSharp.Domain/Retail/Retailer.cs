using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using LevelUpCSharp.Collections;
using LevelUpCSharp.Helpers;
using LevelUpCSharp.Products;
using Newtonsoft.Json;

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

        public void Pickup(string request)
        {
            try
            {
                IEnumerable<Sandwich> sandwiches;

                using (var connection = BuildConnection())
                {
                    using (var stream = connection.GetStream())
                    {
                        SendCommand(stream, request);

                        sandwiches = ReadResponse(stream);
                    }
                }

                Pack(sandwiches, "remote");

            }
            catch (SocketException)
            {
            }
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

        #region networking
        private TcpClient BuildConnection()
        {
            TcpClient client = new TcpClient();
            client.Connect("localhost", 13000);
            return client;
        }

        private IEnumerable<Sandwich> ReadResponse(NetworkStream stream)
        {
            using (var sr = new StreamReader(stream))
            {
                using (var jsonReader = new JsonTextReader(sr))
                {
                    return new JsonSerializer().Deserialize<IEnumerable<Sandwich>>(jsonReader);
                }
            }
        }

        private void SendCommand(NetworkStream stream, string request)
        {
            var data = System.Text.Encoding.ASCII.GetBytes(request);
            stream.Write(data, 0, data.Length);
        }
        #endregion
    }
}
