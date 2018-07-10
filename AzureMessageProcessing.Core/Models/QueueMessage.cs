using System;

namespace AzureMessageProcessing.Core.Models
{
    public class QueueMessage
    {
        public DateTimeOffset CreatedOn { get; set; }

        public Guid MessageId { get; set; }
    }
}
