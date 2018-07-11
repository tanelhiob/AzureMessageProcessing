using AzureMessageProcessing.Core.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using System;
using System.Threading;

namespace AzureMessageProcessing.Generator
{
    public abstract class BaseGenerator
    {
        /// <summary>
        /// Default "onramp"
        /// </summary>
        public string QueueName { get; set; } = "onramp";
        /// <summary>
        /// Default "blob-storage"
        /// </summary>
        public string StorageContainerName { get; set; } = "blob-storage";
        /// <summary>
        /// default 100
        /// </summary>
        public int IntervalInMilliseconds { get; set; } = 100;
        /// <summary>
        /// Default 1000
        /// </summary>
        public int NumberOfItemsInMessage { get; set; } = 1000;
        /// <summary>
        /// Default 1
        /// </summary>
        public int NumberOfMessages { get; set; } = 1;

        private readonly string _source;

        public BaseGenerator(string source)
        {
            _source = source;
        }

        public void Run()
        {
            Console.WriteLine($"Starting generator for {_source}");
            Console.WriteLine($"Generating collection of {NumberOfItemsInMessage} every {Math.Round(IntervalInMilliseconds / 1000d, 2)} seconds " +
                $"until {(NumberOfMessages <= 0 ? "process is stopped" : $"{NumberOfMessages} has been generated") }");

            var queue = GetQueueClient(QueueName);
            var container = GetStorageContainer(StorageContainerName);

            Console.WriteLine("Start generation");

            if (NumberOfMessages <= 0)
            {
                while (true)
                {
                    Generate();
                }
            }
            else
            {
                for(var i = 0; i < NumberOfMessages; i++)
                {
                    Generate();
                }
            }

            void Generate()
            {
                var step = GenerateStep();

                Console.Write($"Uploading payload message '{step.Id}' to storage... ");

                var blockBlob = container.GetBlockBlobReference(step.Id.ToString());
                blockBlob.UploadTextAsync(JsonConvert.SerializeObject(step)).Wait();

                Console.WriteLine($"Done.");

                var message = new Message
                {
                    Created = DateTimeOffset.UtcNow,
                    ContentId = step.Id,
                    From = _source
                };

                Console.Write("Inserting new message to queue... ");
                queue.AddMessageAsync(new CloudQueueMessage(JsonConvert.SerializeObject(message))).Wait();
                Console.WriteLine("Done.");

                Thread.Sleep(IntervalInMilliseconds);
            }
        }

        public abstract Step GenerateStep();

        private CloudQueue GetQueueClient(string queueName, string connectionString = "UseDevelopmentStorage=true")
        {
            var storageAccount = CloudStorageAccount.Parse(connectionString);

            var queueClient = storageAccount.CreateCloudQueueClient();

            var queue = queueClient.GetQueueReference(queueName);

            var wasCreated = queue.CreateIfNotExistsAsync().Result;
            var msg = wasCreated
                ? $"Queue '{queueName}' created"
                : $"Queue '{queueName}' exists";
            Console.WriteLine(msg);

            return queue;

        }

        private CloudBlobContainer GetStorageContainer(string containerName, string connectionString = "UseDevelopmentStorage=true")
        {
            var storageAccount = CloudStorageAccount.Parse(connectionString);
            var storage = storageAccount.CreateCloudBlobClient();

            var storageContainer = storage.GetContainerReference(containerName);
            var wasCreated = storageContainer.CreateIfNotExistsAsync().Result;
            var msg = wasCreated
                ? $"Storage container '{containerName}' created"
                : $"Storage container '{containerName}' exists";
            Console.WriteLine(msg);

            return storageContainer;
        }
    }
}
