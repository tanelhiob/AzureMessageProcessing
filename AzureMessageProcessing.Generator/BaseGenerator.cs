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
        private readonly string _source;
        private readonly int _interval;

        public BaseGenerator(string source, int interval)
        {
            _source = source;
            _interval = interval;
        }

        public void Run()
        {
            var queue = GetQueueClient(_queueName);
            var container = GetStorageContainer(_storageContainerName);

            while (true)
            {
                var step = GenerateStep();

                Console.WriteLine(JsonConvert.SerializeObject(step));

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

                Thread.Sleep(_interval);
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
