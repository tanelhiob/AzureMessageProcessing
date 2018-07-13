using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace AzureMessageProcessing.Statistics
{
    class Program
    {
        static void Main(string[] args)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["StorageConnectionString"]?.ConnectionString
                ?? throw new Exception("Could not parse 'StorageConnectionString' from configuration");

            var storageAccount = CloudStorageAccount.DevelopmentStorageAccount;
            var tableClient = storageAccount.CreateCloudTableClient();
            var table = tableClient.GetTableReference("resultmessages");

            // var results = 

            TableContinuationToken token = null;
            do
            {
                var query = new TableQuery<DynamicTableEntity>();
                var response = table.ExecuteQuerySegmentedAsync(query, token).ConfigureAwait(false).GetAwaiter().GetResult();

                Console.WriteLine($"batch {response.Results.Count}");

                var subtotal = 0L;
                foreach (var item in response.Results)
                {
                    var from = item.Properties["From"].StringValue;
                    var created = item.Properties["Created"].DateTimeOffsetValue.Value;
                    var duration = TimeSpan.FromMilliseconds(item.Properties["DurationMillis"].DoubleValue.Value);
                    

                    subtotal += duration.Milliseconds;
                }

                // averages.Add(subtotal / response.Results.Count);

            } while (token != null);

            // Console.Write($"Average processing time {averages.Average()}");
        }
    }
}
