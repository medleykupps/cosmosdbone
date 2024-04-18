namespace CosmosDbOne.Core.Races
{
    public class RaceStage
    {
        public string Id { get; set; } = "";
        public string RaceId { get; set; } = "";
        public int Stage { get; set; }
        public string Type { get; } = "raceStage";
        public string StartLocation { get; set; } = "";
    }



}
