using System;
using System.Collections.Generic;
using AzureMessageProcessing.Core.Models;

namespace AzureMessageProcessing.Generator.HelloWorld
{
    public class HelloWorldGenerator : BaseGenerator
    {
        private readonly int _numberOfItems;

        public HelloWorldGenerator(string source, int interval, int numberofItems)
            : base(source, interval, numberofItems)
        {
            _numberOfItems = numberofItems;
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
            for (var i = 0; i < _numberOfItems; i++)
            {
                yield return Guid.NewGuid().ToString();
            }
        }
    }
}
