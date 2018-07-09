using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;

namespace AzureMessageProcessing.Console.FreshFruit
{
    class Program
    {
        private static readonly string queueName = "onramp";
        private static readonly string storageName = "storage";

        static void Main(string[] args)
        {
            // Parse the connection string and return a reference to the storage account.
            var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));

            var queueClient = storageAccount.CreateCloudQueueClient();

            var queue = queueClient.GetQueueReference(queueName);
            if (queue.CreateIfNotExists())
            {
                System.Console.WriteLine($"- Queue '{queueName}' exists");
            }
            else
            {
                System.Console.WriteLine($"- Queue '{queueName}' created");
            }

            var storage = storageAccount.CreateCloudBlobClient();

            var storageContainer = storage.GetContainerReference(storageName);
            if (storageContainer.CreateIfNotExists())
            {
                System.Console.WriteLine($"- Storage container '{storageName}' exists");
            }
            else
            {
                System.Console.WriteLine($"- Storage container '{storageName}' created");
            }

            var blob = storageContainer.GetBlockBlobReference("blob");


            var message = GenerateMessage();

            blob.UploadText(message);

            var queueMessage = new CloudQueueMessage(message);
            queue.AddMessage(queueMessage);



            //var m = queue.PeekMessage();

            //System.Console.WriteLine(m);
            System.Console.ReadLine();
        }

        private static string GenerateMessage()
        {
            return "hello";
        }
    }
}
