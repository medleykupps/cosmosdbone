using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmosDbOne.Core.Cyclists
{
    public class Cyclist
    {
        public Cyclist(Guid id, string name, string country, string team = "", string proCyclingStatsUrl = "")
        {
            Id = id.ToString();
            CyclistId = id.ToString();
            Name = name;
            Team = team;
            Country = country;
            ProCyclingStatsUrl = proCyclingStatsUrl;
        }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("cyclistId")]
        public string CyclistId { get; set; }
        public string Type { get; } = "cyclist";
        public string Name { get; set; }
        public string Team { get; set; }
        public string Country { get; set; }

        public string ProCyclingStatsUrl { get; set; } = "";

        public override string ToString()
        {
            return $"{Name} ({Country}) - {Id}";
        }
    }



}
