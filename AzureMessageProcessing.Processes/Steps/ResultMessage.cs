using AzureMessageProcessing.Core.Models;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureMessageProcessing.Processes.Steps
{
    public class ResultMessage : TableEntity
    {
        public QueueMessage QueueMessage { get; set; }
    }
}
