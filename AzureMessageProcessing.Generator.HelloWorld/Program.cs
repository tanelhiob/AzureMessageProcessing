namespace AzureMessageProcessing.Generator.HelloWorld
{
    class Program
    {
        static void Main(string[] args)
        {
            var generator = new HelloWorldGenerator("Hello World")
            {
                IntervalInMilliseconds = 1000,
                NumberOfItemsInMessage = 50,
                NumberOfMessages = -1
            };

            generator.RunAsync().Wait();
        }
    }
}
