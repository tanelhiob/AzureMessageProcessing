using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureMessageProcessing.Core.Models;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;

namespace AzureMessageProcessing.Processes.Processors
{
    public class FruitProcessor : IProcessor
    {
        public async Task ProcessAsync(PipelineMessage step, TraceWriter traceWriter)
        {
            traceWriter.Info("Processing list of fruit crates");

            var fruits = JsonConvert.DeserializeObject<List<Fruit>>(step.Body);

            traceWriter.Info($"Total number of crates: {fruits.Count}");

            traceWriter.Info("Number of crates per fruit:");
            fruits.GroupBy(x => x.Name)
                .Select(g => (Name: g.Key, Count: g.Count()))
                .OrderByDescending(x => x.Count)
                .ToList()
                .ForEach(x => traceWriter.Info($"- {x.Name}: {x.Count}"));

            traceWriter.Info("Number of crates per country of origin:");
            fruits.GroupBy(x => x.CountryOfOrigin)
                .Select(g => (Country: g.Key, Count: g.Count()))
                .OrderByDescending(x => x.Country)
                .ToList()
                .ForEach(x => traceWriter.Info($"- {x.Country}: {x.Count}"));

            var isFairTradeCount = fruits.Count(x => x.IsFairTrade);
            traceWriter.Info($"Percentage of fair trade fruit crates: {Math.Round((double)isFairTradeCount / fruits.Count * 100, 2)}%");

            traceWriter.Info("Processing finished");
        }
    }
}
