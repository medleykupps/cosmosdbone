using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmosDbOneApp;

public class Cyclist(Guid id, string name, string country, string team = "")
{
    [JsonProperty("id")]
    public string Id { get; set; } = id.ToString();
    [JsonProperty("cyclistId")]
    public string CyclistId { get; set; } = id.ToString();
    public string Type { get; init; } = "cyclist";
    public string Name { get; set; } = name;
    public string Team { get; set; } = team;
    public string Country { get; set; } = country;

    public override string ToString()
    {
        return $"{Name} ({Country}) - {Id}";
    }
}

