namespace AzureMessageProcessing.Generator.FreshFruit
{
    public class Program
    {
        static void Main(string[] args)
        {
            var generator = new FruitGenerator("FreshFruits", 200);

            generator.Run();
        }
    }
}