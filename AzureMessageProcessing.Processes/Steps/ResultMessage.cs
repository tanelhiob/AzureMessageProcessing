using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace AzureMessageProcessing.Processes.Steps
{
    public class ResultMessage : TableEntity
    {
        public string ContentId { get; set; }

        public string From { get; set; }

        public DateTimeOffset Created { get; set; }

        public DateTimeOffset Completed { get; set; }
    }
}
