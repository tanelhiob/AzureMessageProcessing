using System.Threading.Tasks;
using System.Xml.Linq;
using AzureMessageProcessing.Core.Models;
using Microsoft.Azure.WebJobs.Host;

namespace AzureMessageProcessing.Processes.Processors
{
    public class CharacterProcessor : IProcessor
    {
        public async Task ProcessAsync(PipelineMessage step, TraceWriter traceWriter)
        {
            traceWriter.Info($"Processing list of DnD characters. Step id {step.Id}");

            XDocument xDoc = XDocument.Parse(step.Body);

            foreach (var character in xDoc.Descendants("Character"))
            {
                var name = character.Element("Name").Value;
                var @class = character.Element("Class").Value;
                var race = character.Element("Race").Value;

                int.TryParse(character.Element("Level").Value, out int level);
                int.TryParse(character.Element("Experience").Value, out int exp);

                int.TryParse(character.Element("Dexterity").Value, out int dex);
                int.TryParse(character.Element("Intelligence").Value, out int @int);
                int.TryParse(character.Element("Charisma").Value, out int cha);
                int.TryParse(character.Element("Wisdom").Value, out int wis);
                int.TryParse(character.Element("Strength").Value, out int str);

                traceWriter.Info($"Processing {name} (Level {level} {@class})");
                traceWriter.Info($"Race: {race}");
                traceWriter.Info($"Exp: {exp}");
                traceWriter.Info($"Int: {@int}; Wis: {wis}; Dex: {dex}; Cha: {cha}; Str: {str}");

                if (level >= 5)
                {
                    traceWriter.Info($"Has highest level");
                }
            }

            traceWriter.Info($"Done processing step {step.Id}");
        }
    }
}
