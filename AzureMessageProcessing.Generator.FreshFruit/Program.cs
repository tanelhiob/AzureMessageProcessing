using AzureMessageProcessing.Core.Generators;

namespace AzureMessageProcessing.Generator.FreshFruit
{
    public class Program
    {
        static void Main(string[] args)
        {
            var generator = new FruitGenerator("FreshFruits")
            {
                NumberOfItemsInMessage = 100,
                NumberOfMessages = 10
            };

            generator.RunAsync().Wait();
        }
    }
}