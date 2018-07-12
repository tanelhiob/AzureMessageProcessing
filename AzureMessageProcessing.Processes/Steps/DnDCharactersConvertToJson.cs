using System;
using System.Threading.Tasks;
using AzureMessageProcessing.Core.Models;
using Microsoft.Azure.WebJobs.Host;

namespace AzureMessageProcessing.Processes.Steps
{
    public class DnDCharactersConvertToJson : IStep
    {
        public Task<PipelineMessage> ProcessAsync(PipelineMessage message, TraceWriter traceWriter)
        {
            throw new NotImplementedException();
        }
    }
}
