using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using AzureMessageProcessing.Core.Models;

namespace AzureMessageProcessing.Generator.DnDCharacters
{
    public class DnDGenerator : BaseGenerator
    {
        public DnDGenerator(string source, int interval) : base(source, interval)
        {
        }

        public override Step GenerateStep()
        {
            // to xml
            var characters = GenerateCharacters().ToList();

            XmlSerializer xsSubmit = new XmlSerializer(typeof(Character));

            var xml = "";

            using (var sww = new StringWriter())
            {
                using (XmlWriter writer = XmlWriter.Create(sww))
                {
                    xsSubmit.Serialize(writer, characters);
                    xml = sww.ToString(); // Your XML
                }
            }

            return new Step
            {
                Body = xml,
                Id = Guid.NewGuid()
            };
        }

        private IEnumerable<Character> GenerateCharacters()
        {
            yield return new Character()
            {
                Charisma = 0,
                Name = "lkf;lfkd;g",
                Level = 8
            };
        }
    }
}
