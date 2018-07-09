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
        private readonly string _queueName = "onramp";

        private readonly string _storageContainerName = "blob-storage";

        private readonly int _interval;

        public BaseGenerator(int interval)
        {
            _interval = interval;
        }

        public void Run()
        {
            var queue = GetQueueClient(_queueName);
            var container = GetStorageContainer(_storageContainerName);

            while (true)
            {
                var message = GenerateMessage();

                Console.WriteLine(JsonConvert.SerializeObject(message));

                Console.Write($"Uploading payload message '{message.Id}' to storage... ");

                var blockBlob = container.GetBlockBlobReference(message.Id.ToString());
                blockBlob.UploadTextAsync(JsonConvert.SerializeObject(message)).Wait();

                Console.WriteLine($"Done.");

                var queueMessage = new QueueMessage
                {
                    CreatedOn = DateTimeOffset.UtcNow,
                    MessageId = message.Id
                };

                Console.Write("Inserting new message to queue... ");
                queue.AddMessageAsync(new CloudQueueMessage(JsonConvert.SerializeObject(queueMessage))).Wait();
                Console.WriteLine("Done.");

                Thread.Sleep(_interval);
            }
        }

        public abstract Message GenerateMessage();

        private CloudQueue GetQueueClient(string queueName, string connectionString = "UseDevelopmentStorage=true")
        {
            var storageAccount = CloudStorageAccount.Parse(connectionString);

            var queueClient = storageAccount.CreateCloudQueueClient();

            var queue = queueClient.GetQueueReference(queueName);

            var wasCreated = queue.CreateIfNotExistsAsync().Result;
            var msg = wasCreated
                ? $"- Queue '{queueName}' exists"
                : $"- Queue '{queueName}' created";
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
                ? $"- Storage container '{containerName}' exists"
                : $"- Storage container '{containerName}' created";
            Console.WriteLine(msg);

            return storageContainer;
        }
    }
}
