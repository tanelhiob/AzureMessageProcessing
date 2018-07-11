using System;
using System.Collections.Generic;
using System.Text;

namespace AzureMessageProcessing.Core.Models
{
    public class PipelineMessage
    {
        public string Body { get; set; }

        public Guid Id { get; set; }


    }
}
