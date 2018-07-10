namespace AzureMessageProcessing.Generator.DnDCharacters
{
    public class Program
    {
        static void Main(string[] args)
        {
            var generator = new DnDGenerator("DnD Characters", interval: 100, numberOfItems: 1000);

            generator.Run();
        }
    }
}
