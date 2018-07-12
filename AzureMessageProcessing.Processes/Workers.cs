using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using AzureMessageProcessing.Core.Models;
using AzureMessageProcessing.Processes.Steps;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;

namespace AzureMessageProcessing.Processes
{
    public static class Workers
    {
        [FunctionName("OnRamp")]
        public static async Task OnRampMessage(
            [QueueTrigger("onramp")] QueueMessage message,
            [Queue("consumption")] CloudQueue consumptionQueue,
            [Queue("dedicated")] CloudQueue dedicatedQueue,
            TraceWriter traceWriter)
        {
            await PushMessageToCorrectQueueAsync(message, consumptionQueue, dedicatedQueue);
        }

        [FunctionName("ProcessorForConsumption")]
        public static async Task ProcessorForConsumption(
            [QueueTrigger("consumption")] QueueMessage message,
            [Blob("blob-storage")] CloudBlobContainer blobContainer,
            [Queue("consumption")] CloudQueue consumptionQueue,
            [Queue("dedicated")] CloudQueue dedicatedQueue,
            TraceWriter traceWriter)
        {
            await ProcessStep(message, blobContainer, consumptionQueue, dedicatedQueue, traceWriter);
        }

        [FunctionName("ProcessorForDedicated")]
        public static async Task ProcessorForDedicated(
            [QueueTrigger("dedicated")] QueueMessage message,
            [Blob("blob-storage")] CloudBlobContainer blobContainer,
            [Queue("consumption")] CloudQueue consumptionQueue,
            [Queue("dedicated")] CloudQueue dedicatedQueue,
            TraceWriter traceWriter)
        {
            await ProcessStep(message, blobContainer, consumptionQueue, dedicatedQueue, traceWriter);
        }

        private static async Task PushMessageToCorrectQueueAsync(
            QueueMessage message,
            CloudQueue consumptionQueue,
            CloudQueue dedicatedQueue)
        {
            if (message.From == "FreshFruits")
            {
                await dedicatedQueue.AddMessageAsync(new CloudQueueMessage(JsonConvert.SerializeObject(message)));
            }
            else
            {
                await consumptionQueue.AddMessageAsync(new CloudQueueMessage(JsonConvert.SerializeObject(message)));
            }
        }

        private static async Task ProcessStep(
            QueueMessage queueMessage,
            CloudBlobContainer blobContainer,
            CloudQueue consumptionQueue,
            CloudQueue dedicatedQueue,
            TraceWriter traceWriter)
        {
            var blobId = queueMessage.ContentId;
            var pipelineMessage = await GetPipelineMessageFromBlobAsync(blobContainer, blobId);

            if (pipelineMessage.Steps.Count == 0)
            {
                pipelineMessage.Steps = GetStepsForProcess(queueMessage.From).ToList();
            }

            var step = SelectStep(pipelineMessage);

            if (step != null)
            {
                var nextPipelineMessage = await step.ProcessAsync(pipelineMessage, traceWriter);

                nextPipelineMessage.NextStep++;

                var newBlobId = nextPipelineMessage.Id;
                await SaveNewPipelineMessageToBlobAsync(blobContainer, newBlobId, nextPipelineMessage);

                queueMessage.ContentId = newBlobId;

                await PushMessageToCorrectQueueAsync(queueMessage, consumptionQueue, dedicatedQueue);
            }
            else
            {
                // TODO write execution info to table storage
            }


            async Task<PipelineMessage> GetPipelineMessageFromBlobAsync(CloudBlobContainer container, Guid id)
            {
                var blob = container.GetBlockBlobReference(id.ToString());
                var blobString = await blob.DownloadTextAsync();
                return JsonConvert.DeserializeObject<PipelineMessage>(blobString);
            }

            async Task SaveNewPipelineMessageToBlobAsync(CloudBlobContainer container, Guid id, PipelineMessage message)
            {
                var blob = container.GetBlockBlobReference(id.ToString());
                await blob.UploadTextAsync(JsonConvert.SerializeObject(message));
            }

            ConcurrentQueue<Type> GetStepsForProcess(string processName)
            {
                var stepMaps = new ConcurrentDictionary<string, ConcurrentQueue<Type>>
                {
                    ["FreshFruits"] = new ConcurrentQueue<Type>(new Type[] { typeof(FreshFruitPrintInfo), typeof(FreshFruitGetWithMaxCrates), typeof(FreshFruitPrintInfo) }),
                    ["DnD Characters"] = new ConcurrentQueue<Type>(new Type[] { }),
                    ["Hello World"] = new ConcurrentQueue<Type>(new Type[] { })
                };

                if (!stepMaps.TryGetValue(processName, out var stepMap)) { throw new Exception($"Steps for process '{processName}' are not configured."); }

                return stepMap;
            }

            IStep SelectStep(PipelineMessage message)
            {
                IStep nextStep = null;

                if (message.NextStep >= message.Steps.Count)
                {
                    traceWriter.Warning($"Reached max number of configured steps {message.NextStep}");
                }
                else
                {
                    var nextStepType = message.Steps[message.NextStep];

                    nextStep = (IStep)Activator.CreateInstance(nextStepType);    
                }

                return nextStep;
            }
        }
    }
}
