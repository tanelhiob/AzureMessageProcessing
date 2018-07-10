using System;

namespace AzureMessageProcessing.Core.Models
{
    public class Message
    {
        public Guid ContentId { get; set; }

        public string From { get; set; }

        public DateTimeOffset Created { get; set; }
    }
}
