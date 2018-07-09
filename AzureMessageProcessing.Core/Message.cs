using System;

namespace AzureMessageProcessing.Core
{
    public class Message
    {
        public Guid Id { get; set; }

        public string Body { get; set; }

        public string Error { get; set; }

        public int? PreviousStep { get; set; }

        public int? NextStep { get; set; }
    }
}
