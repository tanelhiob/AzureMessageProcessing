﻿using System;
using System.Collections.Generic;
using AzureMessageProcessing.Core.Models;
using Newtonsoft.Json;

namespace AzureMessageProcessing.Core.Generators
{
    public class FruitGenerator : BaseGenerator
    {
        public FruitGenerator(string source) 
            : base(source)
        {
        }

        public override PipelineMessage GeneratePipelineMessage() => new PipelineMessage
        {
            Id = Guid.NewGuid(),
            Body = JsonConvert.SerializeObject(GenerateFruitCrates())
        };

        private IEnumerable<Fruit> GenerateFruitCrates()
        {
            var fruits = new List<(string Name, double CrateWeight)> {
                ("Apple", 5),
                ("Pear", 5),
                ("Plum", 2),
                ("Kiwi", 3),
                ("Cherry",1 ),
                ("Grape",10),
                ("Banana", 5.5),
                ("Orange", 10),
                ("Peach", 3.25),
                ("Apricot", 4),
                ("Pineapple", 25),
                ("Watermelon", 50),
                ("Lemon", 20),
                ("Lime",5)
            };
            var countries = new string[] { "Colombia", "Brazil", "Kenya", "India", "Indonesia", "Guyana", "Ecuador", "Panama", "Greece", "Italy", "Spain", "Turkey", "China", "Thailand", "Malaysia", "Argentina" };
            var random = new Random();

            for (var i = 0; i < NumberOfItemsInMessage; i++)
            {
                var (Name, CrateWeight) = fruits[random.Next(fruits.Count)];
                yield return new Fruit
                {
                    Name = Name,
                    CountryOfOrigin = countries[random.Next(countries.Length)],
                    IsFairTrade = random.NextDouble() > 0.5,
                    CrateWeight = CrateWeight
                };
            }
        }
    }
}
