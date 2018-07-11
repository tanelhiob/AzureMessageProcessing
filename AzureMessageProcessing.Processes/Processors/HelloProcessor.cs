using System;
using System.Linq;
using System.Threading.Tasks;
using AzureMessageProcessing.Core.Models;
using Microsoft.Azure.WebJobs.Host;

namespace AzureMessageProcessing.Processes.Processors
{
    public class HelloProcessor : IProcessor
    {
        public async Task ProcessAsync(Step step, TraceWriter traceWriter)
        {
            traceWriter.Info("Processing Hello world guids");

            var lines = step.Body.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

            traceWriter.Info($"Found {lines.Length} guids");
            lines.ToList()
                .ForEach(x => traceWriter.Info($"--- {x}"));

            traceWriter.Info("Finished processing");
        }
    }
}
