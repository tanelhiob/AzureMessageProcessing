using AzureMessageProcessing.Core.Generators;
using McMaster.Extensions.CommandLineUtils;
using System;

namespace AzureMessageProcessing.Generator
{
    public class Program
    {
        public static int Main(string[] args)
             => CommandLineApplication.Execute<Program>(args);

        [Option(ShortName = "g", LongName = "generator", ValueName = "fruit,dnd,hello", Description = "Generator to use")]
        public string GeneratorName { get; }

        [Option(CommandOptionType.NoValue, ShortName = "f", LongName = "forever", Description = "Generate messages until program is closed")]
        public bool RunForever { get; set; }

        [Option(ShortName = "m", LongName = "messages", Description = "How many messages to generate. Only values over 0 have effect")]
        public int? MessagesCount { get; set; }

        [Option(ShortName = "n", LongName = "items-in-messages", Description = "How many items to include in one message")]
        public int? ItemsInMessages { get; set; }

        [Option(ShortName = "i", LongName = "interval", Description = "How often new message is generated, in milliseconds")]
        public int? Interval { get; set; }

        private void OnExecute()
        {
            var generator = GetGenerator();

            if (RunForever) { generator.NumberOfMessages = -1; }
            else if (MessagesCount.HasValue) { generator.NumberOfMessages = MessagesCount.Value; }

            if (ItemsInMessages.HasValue) { generator.NumberOfItemsInMessage = ItemsInMessages.Value; }
            if (Interval.HasValue) { generator.IntervalInMilliseconds = Interval.Value; }

            generator.RunAsync().Wait();
        }

        private BaseGenerator GetGenerator()
        {
            BaseGenerator generator;
            switch (GeneratorName ?? "hello")
            {
                case "fruit":
                    generator = new FruitGenerator("FreshFruits");
                    break;
                case "dnd":
                    generator = new DnDGenerator("DnD Characters");
                    break;
                case "hello":
                    generator = new HelloWorldGenerator("Hello World");
                    break;
                default:
                    throw new NotImplementedException($"Generator for {GeneratorName} does not exist");
            }
            return generator;
        }
    }
}
