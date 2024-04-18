using System;

namespace CosmosDbOne.Core.Races
{
    public class Race
    {
        public string Id { get; set; } = "";
        public string RaceId { get; set; } = "";
        public string Type { get; } = "race";
        public string Name { get; set; } = "";
        public int Season { get; set; }
        public DateTime? StartDate { get; set; }
    }



}
