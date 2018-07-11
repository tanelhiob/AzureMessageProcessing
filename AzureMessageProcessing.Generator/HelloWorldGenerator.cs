using System;
using System.Collections.Generic;
using AzureMessageProcessing.Core.Models;

namespace AzureMessageProcessing.Generator
{
    public class HelloWorldGenerator : BaseGenerator
    {
        public HelloWorldGenerator(string source)
            : base(source)
        {
        }

        public override Step GenerateStep()
        {
            return new Step
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
