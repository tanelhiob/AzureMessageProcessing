using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace AzureMessageProcessing.Processes
{
    public class Person : TableEntity
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }
        public string DataSetId { get; set; }
    }

    public class Message
    {
        public Guid ContentId { get; set; }
        public string From { get; set; }
        public DateTimeOffset Created { get; set; }
    }

    public static class Workers
    {
        [FunctionName("Hello")]
        public static async Task<HttpResponseMessage> Hello(
            [HttpTrigger] HttpRequestMessage request,
            [Queue("onramp")] CloudQueue queue,
            [Table("data")] CloudTable table,
            TraceWriter traceWriter)
        {
            var dataSetId = Guid.NewGuid();

            var op = new TableBatchOperation
            {
                TableOperation.Insert(new Person { RowKey = Guid.NewGuid().ToString(), PartitionKey = string.Empty, FirstName = "Tanel", LastName = "Hiob", Age = 28, DataSetId = dataSetId.ToString() }),
                TableOperation.Insert(new Person { RowKey = Guid.NewGuid().ToString(), PartitionKey = string.Empty, FirstName = "Liisi", LastName = "Mõtshärg", Age = 26, DataSetId = dataSetId.ToString() })
            };

            await table.CreateIfNotExistsAsync();
            await table.ExecuteBatchAsync(op);

            var message = new Message
            {
                ContentId = dataSetId,
                Created = DateTimeOffset.Now,
                From = "Tanel",
            };

            await queue.AddMessageAsync(new CloudQueueMessage(JsonConvert.SerializeObject(message)));

            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("Message added to the queue!") };
        }

        [FunctionName("OnRamp")]
        public static async Task OnRampMessage(
            [QueueTrigger("onramp")] Message message,
            [Queue("consumption")] CloudQueue consumptionQueue,
            [Queue("dedicated")] CloudQueue dedicatedQueue,
            TraceWriter traceWriter)
        {
            if (message.From == "Tanel")
            {
                await dedicatedQueue.AddMessageAsync(new CloudQueueMessage(JsonConvert.SerializeObject(message)));
            }
            else
            {
                await consumptionQueue.AddMessageAsync(new CloudQueueMessage(JsonConvert.SerializeObject(message)));
            }
        }

        private static async Task Processor(Message message, CloudTable table, TraceWriter traceWriter)
        {
            var query = new TableQuery<Person>().Where(TableQuery.GenerateFilterCondition(nameof(Person.DataSetId), QueryComparisons.Equal, message.ContentId.ToString()));

            List<Person> persons = new List<Person>();

            TableContinuationToken token = null;
            do
            {
                var segment = await table.ExecuteQuerySegmentedAsync(query, token);
                token = segment.ContinuationToken;
                persons.AddRange(segment.Results);
            } while (token != null);

            var highestAge = persons.Select(x => x.Age).DefaultIfEmpty(0).Max();
            var personWithHighestAge = persons.FirstOrDefault(x => x.Age == highestAge);

            if (personWithHighestAge != null)
            {
                traceWriter.Info($"{personWithHighestAge.FirstName} {personWithHighestAge.LastName} {personWithHighestAge.Age}");
            }
            else
            {
                traceWriter.Warning("no persons found at all");
            }

            traceWriter.Warning($"message processing time {DateTimeOffset.Now - message.Created}");
        }

        [FunctionName("ProcessorForConsumption")]
        public static async Task ProcessorForConsumption(
            [QueueTrigger("consumption")] Message message,
            [Table("data")] CloudTable table,
            TraceWriter traceWriter)
        {
            await Processor(message, table, traceWriter);
        }

        [FunctionName("ProcessorForDedicated")]
        public static async Task ProcessorForDedicated(
            [QueueTrigger("dedicated")] Message message,
            [Table("data")] CloudTable table,
            TraceWriter traceWriter)
        {
            await Processor(message, table, traceWriter);
        }
    }
}
