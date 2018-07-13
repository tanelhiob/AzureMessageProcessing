using System;
using System.Text;
using System.Threading.Tasks;
using AzureMessageProcessing.Core.Models;
using Microsoft.Azure.WebJobs.Host;

namespace AzureMessageProcessing.Processes.Steps
{
    public class HelloWorldDuplicate : IStep
    {
        public async Task<PipelineMessage> ProcessAsync(PipelineMessage message, TraceWriter traceWriter)
        {
            traceWriter.Warning("Appending strings to themselves");

            var lines = message.Body.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

            var sb = new StringBuilder();
            foreach (var line in lines)
            {
                sb.AppendLine(line + "|" + line);
            }

            message.Body = sb.ToString();

            return message;
        }
    }
}
