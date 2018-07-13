using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AzureMessageProcessing.Core.Models;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;

namespace AzureMessageProcessing.Processes.Steps
{
    public class DnDCharactersStatsByClass : IStep
    {
        public async Task<PipelineMessage> ProcessAsync(PipelineMessage message, TraceWriter traceWriter)
        {
            traceWriter.Warning("Calculating average stats by class");

            var typeDefinition = new[]
            {
                new {
                    Name = "",
                    Class = "",
                    Race = "",
                    Age = 0,
                    Level = 0,
                    Experience = 0,
                    Intelligence = 0,
                    Charisma = 0,
                    Wisdom = 0,
                    Dexterity = 0,
                    Strength = 0
                }
            };

            var characters = JsonConvert.DeserializeAnonymousType(message.Body, typeDefinition);

            var averages = characters
                .GroupBy(x => x.Class)
                .Select(x =>
                (
                    Class: x.Key,
                    Count: x.Count(),
                    AverageIntelligence: x.Average(y => y.Intelligence),
                    AverageWisdom: x.Average(y => y.Wisdom),
                    AverageDexterity: x.Average(y => y.Dexterity),
                    AverageCharisma: x.Average(y => y.Charisma),
                    Averagestrength: x.Average(y => y.Strength)
                ));

            foreach (var (Class, Count, AverageIntelligence, AverageWisdom, AverageDexterity, AverageCharisma, Averagestrength) in averages)
            {
                traceWriter.Warning($"- Class '{Class}' ({Count})");
                traceWriter.Warning($"-- Int: {Math.Round(AverageIntelligence, 2)}");
                traceWriter.Warning($"-- Dex: {Math.Round(AverageDexterity, 2)}");
                traceWriter.Warning($"-- Cha: {Math.Round(AverageCharisma, 2)}");
                traceWriter.Warning($"-- Wis: {Math.Round(AverageWisdom, 2)}");
                traceWriter.Warning($"-- Str: {Math.Round(Averagestrength, 2)}");
            }

            return message;
        }
    }
}
