using AzureMessageProcessing.Core.Models;
using Microsoft.Azure.WebJobs.Host;
using System.Threading.Tasks;

namespace AzureMessageProcessing.Processes.Processors
{
    public interface IProcessor
    {
        Task ProcessAsync(Step step, TraceWriter traceWriter);
    }
}
