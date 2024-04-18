using System;
using System.Collections;
using System.Collections.Generic;

namespace CosmosDbOne.Core.Cyclists
{
    public class CyclistRace
    {
        public string Id { get; set; } = "";
        public string CyclistId { get; set; } = "";
        public string Type { get; } = "cyclistRace";
        public DateTime? Date { get; set; }
        public CyclistRaceDetails? Race { get; set; }
        public int? Result { get; set; }
        public ICollection<CyclistRaceClassification> Classifications { get; set; } = new List<CyclistRaceClassification>();
    }

}
