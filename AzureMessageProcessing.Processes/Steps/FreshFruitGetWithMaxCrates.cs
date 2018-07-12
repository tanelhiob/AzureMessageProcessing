using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureMessageProcessing.Core.Models;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;

namespace AzureMessageProcessing.Processes.Steps
{
    public class FreshFruitGetWithMaxCrates : IStep
    {
        public async Task<PipelineMessage> ProcessAsync(PipelineMessage message, TraceWriter traceWriter)
        {
            traceWriter.Info("Processing list of fruit crates");

            var fruits = JsonConvert.DeserializeObject<List<Fruit>>(message.Body);

            var maxCrates = fruits
                .GroupBy(x => x.Name)
                .Select(g => (Name: g.Key, Count: g.Count(), Fruits: g.ToList()))
                .OrderByDescending(x => x.Count)
                .FirstOrDefault()
                .Fruits;

            message.Body = JsonConvert.SerializeObject(maxCrates);

            message.Id = Guid.NewGuid();
            return message;
        }
    }
}
