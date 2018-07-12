using AzureMessageProcessing.Core.Generators;

namespace AzureMessageProcessing.Generator.FreshFruit
{
    public class Program
    {
        static void Main(string[] args)
        {
            var generator = new FruitGenerator("FreshFruits")
            {
                NumberOfItemsInMessage = 2000,
                NumberOfMessages = -1
            };

            generator.RunAsync().Wait();
        }
    }
}