using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureMessageProcessing.Core.Models;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;

namespace AzureMessageProcessing.Processes.Steps
{
    public class FreshFruitPrintInfo : IStep
    {
        public async Task<PipelineMessage> ProcessAsync(PipelineMessage message, TraceWriter traceWriter)
        {
            traceWriter.Warning("Information about fruit crates");

            var fruits = JsonConvert.DeserializeObject<List<Fruit>>(message.Body);

            traceWriter.Warning($"- Total number of crates: {fruits.Count}");

            traceWriter.Warning("- Number of crates per fruit:");
            var cratesPerFruit=fruits.GroupBy(x => x.Name)
                .Select(g => (Name: g.Key, Count: g.Count()))
                .OrderByDescending(x => x.Count);

            foreach (var (Name, Count) in cratesPerFruit)
            {
                traceWriter.Warning($"-- {Name}: {Count}");
            }

            traceWriter.Warning("- Number of crates per country of origin:");
            var cratesPerCountry = fruits.GroupBy(x => x.CountryOfOrigin)
                .Select(g => (Country: g.Key, Count: g.Count()))
                .OrderByDescending(x => x.Country);

            foreach (var (Country, Count) in cratesPerCountry)
            {
                traceWriter.Warning($"-- {Country}: {Count}");
            }
            
            var isFairTradeCount = fruits.Count(x => x.IsFairTrade);
            traceWriter.Warning($"- Percentage of fair trade fruit crates: {Math.Round((double)isFairTradeCount / fruits.Count * 100, 2)}%");

            message.Id = Guid.NewGuid();

            return message;
        }
    }
}
