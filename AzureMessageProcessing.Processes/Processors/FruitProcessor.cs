using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AzureMessageProcessing.Core.Models;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;

namespace AzureMessageProcessing.Processes.Processors
{
    public class FruitProcessor : IProcessor
    {
        public async Task ProcessAsync(Step step, TraceWriter traceWriter)
        {
            var fruits = JsonConvert.DeserializeObject<List<Fruit>>(step.Body);

            traceWriter.Verbose(step.Body);
            traceWriter.Info($"Number of fruit crates; {fruits.Count}");

        }
    }
}
