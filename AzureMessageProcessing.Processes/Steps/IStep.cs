using AzureMessageProcessing.Core.Models;
using Microsoft.Azure.WebJobs.Host;
using System.Threading.Tasks;

namespace AzureMessageProcessing.Processes.Steps
{
    public interface IStep
    {
        Task<PipelineMessage> ProcessAsync(PipelineMessage message, TraceWriter traceWriter);
    }
}
