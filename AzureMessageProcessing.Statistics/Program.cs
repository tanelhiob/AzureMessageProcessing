using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;

namespace AzureMessageProcessing.Statistics
{
    class Program
    {
        static void Main(string[] args)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["StorageConnectionString"]?.ConnectionString;

            var storageAccount = connectionString == null
                ? CloudStorageAccount.DevelopmentStorageAccount
                : CloudStorageAccount.Parse(connectionString); 
                
            var tableClient = storageAccount.CreateCloudTableClient();
            var table = tableClient.GetTableReference("resultmessages");

            var segmentSize = 100;
            var piles = new Dictionary<string, List<double>>();
            var averages = new Dictionary<string, List<double>>();

            var hellos = new List<double>();

            var total = 0;
            TableContinuationToken token = null;
            do
            {
                var query = new TableQuery<DynamicTableEntity>();
                var response = table.ExecuteQuerySegmentedAsync(query, token).ConfigureAwait(false).GetAwaiter().GetResult();

                total += response.Results.Count;
                Console.WriteLine($"batch {response.Results.Count}, total {total}");

                foreach (var item in response.Results)
                {
                    var from = item.Properties["From"].StringValue;
                    var created = item.Properties["Created"].DateTimeOffsetValue.Value;
                    var completed = item.Properties["Completed"].DateTimeOffsetValue.Value;
                    var duration = completed - created;

                    if (from == "Hello World")
                    {
                        hellos.Add(duration.TotalMilliseconds);
                    }

                    if(!piles.ContainsKey(from))
                    {
                        piles.Add(from, new List<double>());
                    }

                    piles[from].Add(duration.TotalMilliseconds);
                }

                foreach (var source in piles.Keys)
                {
                    var pile = piles[source];

                    while (pile.Count >= segmentSize)
                    {
                        var subAverage = pile.Take(segmentSize).Average();
                        pile.RemoveRange(0, segmentSize);
                        
                        if (!averages.ContainsKey(source))
                        {
                            averages.Add(source, new List<double>());
                        }

                        Console.WriteLine(subAverage);
                        averages[source].Add(subAverage);
                    }
                }

                token = response.ContinuationToken;
            } while (token != null);

            File.WriteAllLines("hellos.csv", hellos.Select(x => x.ToString()));

            foreach (var source in averages.Keys)
            {
                var averageMillis = averages[source].Average();
                var averageTime = TimeSpan.FromMilliseconds(averageMillis);
                Console.WriteLine($"Average processing time for {source}: {averageTime}");
            }
        }
    }
}