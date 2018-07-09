using AzureMessageProcessing.Core;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;

namespace AzureMessageProcessing.Generator.FreshFruit
{
    class Program
    {
        private static readonly string queueName = "onramp";
        private static readonly string storageName = "blob-storage";

        private static readonly int interval = 200; // In ms

        static void Main(string[] args)
        {
            // Parse the connection string and return a reference to the storage account.
            var storageAccount = CloudStorageAccount.Parse("UseDevelopmentStorage=true");

            var queue = GetQueueClient(storageAccount);
            var container = GetStorageContainer(storageAccount);

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

                Thread.Sleep(interval);
            }
        }

        private static CloudQueue GetQueueClient(CloudStorageAccount storageAccount)
        {
            var queueClient = storageAccount.CreateCloudQueueClient();

            var queue = queueClient.GetQueueReference(queueName);

            var wasCreated = queue.CreateIfNotExistsAsync().Result;
            var msg = wasCreated
                ? $"- Queue '{queueName}' exists"
                : $"- Queue '{queueName}' created";
            Console.WriteLine(msg);

            return queue;
        }

        private static CloudBlobContainer GetStorageContainer(CloudStorageAccount storageAccount)
        {
            var storage = storageAccount.CreateCloudBlobClient();

            var storageContainer = storage.GetContainerReference(storageName);
            var wasCreated = storageContainer.CreateIfNotExistsAsync().Result;
            var msg = wasCreated
                ? $"- Storage container '{storageName}' exists"
                : $"- Storage container '{storageName}' created";
            Console.WriteLine(msg);

            return storageContainer;
        }

        private static Message GenerateMessage() => new Message
        {
            Body = JsonConvert.SerializeObject(GenerateFruitCrates()),
            Id = Guid.NewGuid()
        };

        private static IEnumerable<Fruit> GenerateFruitCrates()
        {
            var fruits = new List<(string Name, double CrateWeight)> {
                ("Apple", 5),
                ("Pear", 5),
                ("Plum", 2),
                ("Kiwi", 3),
                ("Cherry",1 ),
                ("Grape",10),
                ("Banana", 5.5),
                ("Orange", 10),
                ("Peach", 3.25),
                ("Apricot", 4),
                ("Pineapple", 25),
                ("Watermelon", 50),
                ("Lemon", 20),
                ("Lime",5)
            };
            var countries = new string[] { "Colombia", "Brazil", "Kenya", "India", "Indonesia", "Guyana", "Ecuador", "Panama", "Greece", "Italy", "Spain", "Turkey", "China", "Thailand", "Malaysia", "Argentina" };
            var random = new Random();

            for (var i = 0; i < 100; i++)
            {
                var (Name, CrateWeight) = fruits[random.Next(fruits.Count - 1)];
                yield return new Fruit
                {
                    Name = Name,
                    CountryOfOrigin = countries[random.Next(countries.Length - 1)],
                    IsFairTrade = random.NextDouble() > 0.5,
                    CrateWeight = CrateWeight
                };
            }
        }
    }
}