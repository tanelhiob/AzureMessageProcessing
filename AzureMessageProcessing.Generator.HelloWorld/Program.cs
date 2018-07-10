namespace AzureMessageProcessing.Generator.HelloWorld
{
    class Program
    {
        static void Main(string[] args)
        {
            var generator = new HelloWorldGenerator("Hello World", interval: 1000, numberofItems: 25);

            generator.Run();
        }
    }
}
