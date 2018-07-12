using System;
using System.Collections.Generic;
using AzureMessageProcessing.Core.Models;

namespace AzureMessageProcessing.Core.Generators
{
    public class HelloWorldGenerator : BaseGenerator
    {
        public HelloWorldGenerator(string source)
            : base(source)
        {
        }

        public override PipelineMessage GeneratePipelineMessage()
        {
            return new PipelineMessage
            {
                Body = string.Join(Environment.NewLine, GenerateContent()),
                Id = Guid.NewGuid()
            };
        }

        private IEnumerable<string> GenerateContent()
        {
            for (var i = 0; i < NumberOfItemsInMessage; i++)
            {
                yield return Guid.NewGuid().ToString();
            }
        }
    }
}
