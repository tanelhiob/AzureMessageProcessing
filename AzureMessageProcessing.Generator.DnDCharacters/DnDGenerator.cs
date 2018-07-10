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
        private readonly int _numberOfItems;

        public DnDGenerator(string source, int interval, int numberOfItems) 
            : base(source, interval, numberOfItems)
        {
            _numberOfItems = numberOfItems;
        }

        public override Step GenerateStep()
        {
            // Serialize to XML
            var characters = GenerateCharacters().ToList();

            XmlSerializer serializer = new XmlSerializer(typeof(List<Character>));

            var charactersXml = string.Empty;

            using (var stringWriter = new StringWriter())
            using (XmlWriter xmlWriter = XmlWriter.Create(stringWriter))
            {
                serializer.Serialize(xmlWriter, characters);
                charactersXml = stringWriter.ToString();
            }

            return new Step
            {
                Body = charactersXml,
                Id = Guid.NewGuid()
            };
        }

        private IEnumerable<Character> GenerateCharacters()
        {
            var classes = new string[] { "Barbarian", "Bard", "Cleric", "Druid", "Fighter", "Monk", "Paladin", "Ranger", "Rogue", "Sorcerer", "Warlock", "Wizard" };
            var races = new string[] { "Dwarf", "Elf", "Gnome", "Half-elf", "Half-orc", "Halfling", "Human" };

            var random = new Random();

            for (var i = 0; i < _numberOfItems; i++)
            {
                var level = random.Next(6);
                yield return new Character()
                {
                    Name = string.Join("", Guid.NewGuid().ToString().Where(x => char.IsLetter(x))).FirstLetterToUpperCase(),
                    Age = random.Next(5, 2000),
                    Race = races[random.Next(races.Length)],
                    Level = level,
                    Class = classes[random.Next(classes.Length)],
                    Experience = level * 1000,
                    Charisma = random.Next(5, 16),
                    Dexterity = random.Next(5, 16),
                    Intelligence = random.Next(5, 16),
                    Strength = random.Next(5, 16),
                    Wisdom = random.Next(5, 16)
                };
            }
        }
    }
}
