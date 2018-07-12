using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using AzureMessageProcessing.Core.Models;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;

namespace AzureMessageProcessing.Processes.Steps
{
    /// <summary>
    /// Converts characters xml to json. Uses anonymous type definition instead of class
    /// </summary>
    public class DnDCharactersConvertToJson : IStep
    {
        public async Task<PipelineMessage> ProcessAsync(PipelineMessage message, TraceWriter traceWriter)
        {
            traceWriter.Warning("Converting DnD characters XML to JSON");

            var xdoc = XDocument.Parse(message.Body);
            var json = JsonConvert.SerializeXNode(xdoc);

            var typeDefinition = new
            {
                ArrayOfCharacter = new
                {
                    Character = new[] {
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
                    }
                }
            };

            var anonymousObj = JsonConvert.DeserializeAnonymousType(json, typeDefinition);

            var characters = anonymousObj.ArrayOfCharacter.Character.ToList();

            traceWriter.Warning("Conversion finished.");

            message.Body = JsonConvert.SerializeObject(characters);

            return message;
        }
    }
}
