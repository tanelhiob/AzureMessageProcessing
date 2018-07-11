namespace AzureMessageProcessing.Generator.DnDCharacters
{
    public class Program
    {
        static void Main(string[] args)
        {
            var generator = new DnDGenerator("DnD Characters")
            {
                NumberOfMessages = 2
            };

            generator.Run();
        }
    }
}
