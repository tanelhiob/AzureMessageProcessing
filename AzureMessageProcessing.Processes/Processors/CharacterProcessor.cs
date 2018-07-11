using System;
using System.Threading.Tasks;
using AzureMessageProcessing.Core.Models;
using Microsoft.Azure.WebJobs.Host;

namespace AzureMessageProcessing.Processes.Processors
{
    public class CharacterProcessor : IProcessor
    {
        public Task ProcessAsync(Step step, TraceWriter traceWriter)
        {
            throw new NotImplementedException();
        }
    }
}
