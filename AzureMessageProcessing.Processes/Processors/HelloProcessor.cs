using System;
using System.Threading.Tasks;
using AzureMessageProcessing.Core.Models;
using Microsoft.Azure.WebJobs.Host;

namespace AzureMessageProcessing.Processes.Processors
{
    public class HelloProcessor : IProcessor
    {
        public async Task ProcessAsync(Step step, TraceWriter traceWriter)
        {
            var lines = step.Body.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);


        }
    }
}
