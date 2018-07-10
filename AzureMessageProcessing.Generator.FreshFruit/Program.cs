namespace AzureMessageProcessing.Generator.FreshFruit
{
    public class Program
    {
        static void Main(string[] args)
        {
            var generator = new FruitGenerator("FreshFruits", interval: 200, numberOfItems: 2000);

            generator.Run();
        }
    }
}