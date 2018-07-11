using AzureMessageProcessing.Core.Models;
using Microsoft.Azure.WebJobs.Host;
using System.Threading.Tasks;

namespace AzureMessageProcessing.Processes.Processors
{
    public interface IProcessor
    {
        Task ProcessAsync(PipelineMessage step, TraceWriter traceWriter);
    }
}
