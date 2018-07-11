using System.Threading.Tasks;

namespace AzureMessageProcessing.Generator.DnDCharacters
{
    public class Program
    {
        static void Main(string[] args)
        {
            var generator = new DnDGenerator("DnD Characters")
            {
                IntervalInMilliseconds = 200,
                NumberOfMessages = -1
            };

            generator.RunAsync().Wait();
        }
    }
}
