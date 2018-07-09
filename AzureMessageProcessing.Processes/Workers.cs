using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Queue;

namespace AzureMessageProcessing.Processes
{
    public static class Workers
    {
        [FunctionName("Hello")]
        public static async Task<HttpResponseMessage> Hello([HttpTrigger] HttpRequestMessage request, [Queue("onramp")] CloudQueue queue, TraceWriter traceWriter)
        {
            traceWriter.Info("Hello, World!");

            await queue.AddMessageAsync(new CloudQueueMessage("Hello, world!"));

            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("Hello, World!") };
        }

        [FunctionName("OnRamp")]
        public static async Task OnRampMessage([QueueTrigger("onramp")] string message, [Queue("consumption")] CloudQueue queue, TraceWriter log)
        {
            // TODO: decide which type of client

            await queue.AddMessageAsync(new CloudQueueMessage(message));
        }

        [FunctionName("Processor")]
        public static void Processor([QueueTrigger("consumption")] string message, TraceWriter log)
        {
            log.Info("Handled!");
        }
    }
}
