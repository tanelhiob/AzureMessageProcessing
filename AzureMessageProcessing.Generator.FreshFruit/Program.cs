namespace AzureMessageProcessing.Generator.FreshFruit
{
    public class Program
    {
        static void Main(string[] args)
        {
            var generator = new FruitGenerator("FreshFruits")
            {
                IntervalInMilliseconds = 200,
                NumberOfItemsInMessage = 2000
            };

            generator.RunAsync().Wait();
        }
    }
}