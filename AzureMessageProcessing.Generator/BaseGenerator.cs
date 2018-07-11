using AzureMessageProcessing.Core.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AzureMessageProcessing.Generator
{
    public abstract class BaseGenerator
    {
        /// <summary>
        /// Name of Azure queue.
        /// Default is "onramp"
        /// </summary>
        public string QueueName { get; set; } = "onramp";
        /// <summary>
        /// Name of Azure storage container.
        /// Default is "blob-storage"
        /// </summary>
        public string StorageContainerName { get; set; } = "blob-storage";
        /// <summary>
        /// How much to wait until generating new messages.
        /// Default is 100
        /// </summary>
        public int IntervalInMilliseconds { get; set; } = 100;
        /// <summary>
        /// How many items to include in messages.
        /// Default is 1000
        /// </summary>
        public int NumberOfItemsInMessage { get; set; } = 1000;
        /// <summary>
        /// How many messages will be generated. Only effective if number is >0, otherwise will generate forever.
        /// Default is 1
        /// </summary>
        public int NumberOfMessages { get; set; } = 1;

        private readonly string _source;

        public BaseGenerator(string source)
        {
            _source = source;
        }

        public async Task RunAsync()
        {
            Console.WriteLine($"Starting generator for {_source}");
            Console.WriteLine($"Generating collection of {NumberOfItemsInMessage} every {Math.Round(IntervalInMilliseconds / 1000d, 2)} seconds " +
                $"until {(NumberOfMessages <= 0 ? "process is stopped" : $"{NumberOfMessages} has been generated") }");

            var queue = await GetQueueClientAsync(QueueName);
            var container = await GetStorageContainerAsync(StorageContainerName);

            Console.WriteLine("Start generation");

            if (NumberOfMessages <= 0)
            {
                while (true)
                {
                    await GenerateAsync();
                }
            }
            else
            {
                for (var i = 0; i < NumberOfMessages; i++)
                {
                    await GenerateAsync();
                }
            }

            async Task GenerateAsync()
            {
                var step = GenerateStep();

                Console.Write($"Uploading payload message '{step.Id}' to storage... ");

                var blockBlob = container.GetBlockBlobReference(step.Id.ToString());
                await blockBlob.UploadTextAsync(JsonConvert.SerializeObject(step));

                Console.WriteLine($"Done.");

                var message = new Message
                {
                    Created = DateTimeOffset.UtcNow,
                    ContentId = step.Id,
                    From = _source
                };

                Console.Write("Inserting new message to queue... ");
                await queue.AddMessageAsync(new CloudQueueMessage(JsonConvert.SerializeObject(message)));
                Console.WriteLine("Done.");

                Thread.Sleep(IntervalInMilliseconds);
            }
        }

        public abstract Step GenerateStep();

        private async Task<CloudQueue> GetQueueClientAsync(string queueName, string connectionString = "UseDevelopmentStorage=true")
        {
            var storageAccount = CloudStorageAccount.Parse(connectionString);

            var queueClient = storageAccount.CreateCloudQueueClient();

            var queue = queueClient.GetQueueReference(queueName);

            var wasCreated = await queue.CreateIfNotExistsAsync();
            var msg = wasCreated
                ? $"Queue '{queueName}' created"
                : $"Queue '{queueName}' exists";
            Console.WriteLine(msg);

            return queue;
        }

        private async Task<CloudBlobContainer> GetStorageContainerAsync(string containerName, string connectionString = "UseDevelopmentStorage=true")
        {
            var storageAccount = CloudStorageAccount.Parse(connectionString);
            var storage = storageAccount.CreateCloudBlobClient();

            var storageContainer = storage.GetContainerReference(containerName);
            var wasCreated = await storageContainer.CreateIfNotExistsAsync();
            var msg = wasCreated
                ? $"Storage container '{containerName}' created"
                : $"Storage container '{containerName}' exists";
            Console.WriteLine(msg);

            return storageContainer;
        }
    }
}
