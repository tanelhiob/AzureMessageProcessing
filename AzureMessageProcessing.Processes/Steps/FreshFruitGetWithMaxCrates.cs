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
            traceWriter.Warning("Finding fruit with maximum number of crates");

            var fruits = JsonConvert.DeserializeObject<List<Fruit>>(message.Body);

            var (Name, Count, MaxFruits) = fruits
                .GroupBy(x => x.Name)
                .Select(g => (Name: g.Key, Count: g.Count(), Fruits: g.ToList()))
                .OrderByDescending(x => x.Count)
                .FirstOrDefault();

            traceWriter.Warning($"{Name} has most crates ({Count})");

            message.Body = JsonConvert.SerializeObject(MaxFruits);

            message.Id = Guid.NewGuid();
            return message;
        }
    }
}
