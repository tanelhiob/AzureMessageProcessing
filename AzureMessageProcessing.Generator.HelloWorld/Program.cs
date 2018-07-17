using AzureMessageProcessing.Core.Generators;

namespace AzureMessageProcessing.Generator.HelloWorld
{
    class Program
    {
        static void Main(string[] args)
        {
            var generator = new HelloWorldGenerator("Hello World")
            {
                IntervalInMilliseconds = 1000,
                NumberOfItemsInMessage = 100,
                NumberOfMessages = 10
            };

            generator.RunAsync().Wait();
        }
    }
}
