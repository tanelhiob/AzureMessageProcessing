namespace AzureMessageProcessing.Generator.DnDCharacters
{
    public class Program
    {
        static void Main(string[] args)
        {
            var generator = new DnDGenerator(100);

            generator.Run();
        }
    }
}
