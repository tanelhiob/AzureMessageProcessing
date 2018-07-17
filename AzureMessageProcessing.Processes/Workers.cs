using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureMessageProcessing.Core.Models;
using AzureMessageProcessing.Processes.Steps;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace AzureMessageProcessing.Processes
{
    public static class Workers
    {
        private const string _consuptionQueueName = "consumption";
        private const string _dedicatedQueueName = "dedicated";
        private const string _onRampQueueName = "onramp";
        private const string _blobStorageName = "blob-storage";
        private const string _resultMessagesTableName = "resultmessages";

        private const string _freshFruitsSource = "FreshFruits";
        private const string _dndCharactersSource = "DnD Characters";
        private const string _helloWorldSource = "Hello World";

        private readonly static Dictionary<string, string> _messageQueueMap = new Dictionary<string, string>
        {
            [_dndCharactersSource] = _consuptionQueueName,
            [_helloWorldSource] = _consuptionQueueName,
            [_freshFruitsSource] = _dedicatedQueueName,
        };

        [FunctionName(nameof(OnRamp))]
        public static async Task OnRamp(
            [QueueTrigger(_onRampQueueName)] QueueMessage message,
            [Queue(_consuptionQueueName)] CloudQueue consumptionQueue,
            [Queue(_dedicatedQueueName)] CloudQueue dedicatedQueue,
            TraceWriter traceWriter)
        {
            await PushMessageToCorrectQueueAsync(message, consumptionQueue, dedicatedQueue);
        }

        [FunctionName(nameof(ProcessorForConsumption))]
        public static async Task ProcessorForConsumption(
            [QueueTrigger(_consuptionQueueName)] QueueMessage message,
            [Blob(_blobStorageName)] CloudBlobContainer blobContainer,
            [Queue(_consuptionQueueName)] CloudQueue consumptionQueue,
            [Queue(_dedicatedQueueName)] CloudQueue dedicatedQueue,
            [Table(_resultMessagesTableName)] CloudTable resultMessageTable,
            TraceWriter traceWriter)
        {
            await ProcessStep(message, blobContainer, consumptionQueue, dedicatedQueue, resultMessageTable, traceWriter);
        }

        [FunctionName(nameof(ProcessorForDedicated))]
        public static async Task ProcessorForDedicated(
            [QueueTrigger(_dedicatedQueueName)] QueueMessage message,
            [Blob(_blobStorageName)] CloudBlobContainer blobContainer,
            [Queue(_consuptionQueueName)] CloudQueue consumptionQueue,
            [Queue(_dedicatedQueueName)] CloudQueue dedicatedQueue,
            [Table(_resultMessagesTableName)] CloudTable resultMessageTable,
            TraceWriter traceWriter)
        {
            await ProcessStep(message, blobContainer, consumptionQueue, dedicatedQueue, resultMessageTable, traceWriter);
        }

        private static async Task PushMessageToCorrectQueueAsync(
            QueueMessage message,
            CloudQueue consumptionQueue,
            CloudQueue dedicatedQueue)
        {
            if (_messageQueueMap.TryGetValue(message.From, out string source))
            {
                if (source == _dedicatedQueueName)
                {
                    await dedicatedQueue.AddMessageAsync(new CloudQueueMessage(JsonConvert.SerializeObject(message)));
                }
                else if (source == _consuptionQueueName)
                {
                    await consumptionQueue.AddMessageAsync(new CloudQueueMessage(JsonConvert.SerializeObject(message)));
                }
                else
                {
                    throw new NotImplementedException();
                }
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
            CloudTable resultMessageTable,
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
                nextPipelineMessage.Id = Guid.NewGuid();

                nextPipelineMessage.NextStep++;

                var newBlobId = nextPipelineMessage.Id;
                await SaveNewPipelineMessageToBlobAsync(blobContainer, newBlobId, nextPipelineMessage);

                queueMessage.ContentId = newBlobId;

                await PushMessageToCorrectQueueAsync(queueMessage, consumptionQueue, dedicatedQueue);
            }
            else
            {
                await resultMessageTable.CreateIfNotExistsAsync();

                var resultMessage = new ResultMessage
                {
                    RowKey = Guid.NewGuid().ToString(),
                    PartitionKey = string.Empty,
                    ContentId = queueMessage.ContentId.ToString(),
                    From = queueMessage.From,
                    Created = queueMessage.Created,
                    Completed = DateTimeOffset.Now,
                };
                await resultMessageTable.ExecuteAsync(TableOperation.Insert(resultMessage));
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
                    [_freshFruitsSource] = new ConcurrentQueue<Type>(new Type[] {
                        typeof(FreshFruitPrintInfo),
                        typeof(FreshFruitGetWithMaxCrates),
                        typeof(FreshFruitPrintInfo)
                    }),
                    [_freshFruitsSource] = new ConcurrentQueue<Type>(new Type[] {
                        typeof(DnDCharactersConvertToJson),
                        typeof(DnDCharactersStatsByClass)
                    }),
                    [_helloWorldSource] = new ConcurrentQueue<Type>(new Type[] {
                        typeof(HelloWorldDuplicate),
                        typeof(HelloWorldDuplicate),
                        typeof(HelloWorldDuplicate),
                        typeof(HelloWorldDuplicate),
                        typeof(HelloWorldDuplicate)
                    })
                };

                if (stepMaps.TryGetValue(processName, out var stepMap))
                {
                    return stepMap;
                }
                else
                {
                    throw new Exception($"Steps for process '{processName}' are not configured.");
                }
            }

            IStep SelectStep(PipelineMessage message)
            {
                IStep nextStep = null;

                if (message.NextStep >= message.Steps.Count)
                {
                    traceWriter.Warning($"Reached max number of configured steps: {message.NextStep}");
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
