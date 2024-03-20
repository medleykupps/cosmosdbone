using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmosDbOneApp;

public class Cyclist(Guid id, string name, string country)
{
    public string Id { get; set; } = id.ToString();
    public string Type { get; init; } = "cyclist";
    public string Name { get; set; } = name;
    public string Country { get; set; } = country;
}

