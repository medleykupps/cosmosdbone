using CosmosDbOne.Core.Cyclists;
using CosmosDbOne.Core.Races;
using CosmosDbOneApp;
using HtmlAgilityPack;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Scripts;
using System.Globalization;
using Xunit;
using Xunit.Abstractions;

namespace CosmosDbOneUtilities;

public class CyclistSeed(ITestOutputHelper output)
{
    [Fact]
    public async Task Add_seed_cyclists()
    {
        CosmosDbClientFactory factory = new();

        using var client = factory.CreateClient();

        var db = client.GetDatabase(DatabaseOperations.Database);

        Container container = db.GetContainer(DatabaseOperations.CyclistsContainer);

        async Task<Cyclist> Create(string name, string country)
        {
            Cyclist cyclist = new(Guid.NewGuid(), name, country);

            return await container.CreateItemAsync(
                item: cyclist,
                partitionKey: new PartitionKey(cyclist.CyclistId)
            );
        }

        await Create("MATTHEWS Michael", "AUSTRALIA");
        await Create("PHILIPSEN Jasper", "BELGIUM");
        await Create("POGACAR Tadej", "SLOVENIA");
    }

    [Fact]
    public async Task Seed_Cyclist_details_from_UCI_page()
    {
        const int attempts = 999;

        CosmosDbClientFactory factory = new();
        using var client = factory.CreateClient();
        var container = client.GetDatabase(DatabaseOperations.Database).GetContainer(DatabaseOperations.CyclistsContainer);

        var current = 0;
        HtmlWeb web = new();
        var feed = container.GetItemQueryIterator<Cyclist>("SELECT * FROM cyclists c WHERE c.type = 'cyclist'");
        while (feed.HasMoreResults)
        {
            var response = await feed.ReadNextAsync();

            foreach (var cyclist in response)
            {
                var url = $"https://www.procyclingstats.com/{cyclist.ProCyclingStatsUrl}";

                output.WriteLine($"Retrieving details for cyclist {cyclist.Name} from {url}");

                if (current < attempts)
                {
                    var doc = web.Load(url);

                    var tableNode = doc.DocumentNode.SelectSingleNode("//table[@class='rdrResults']");
                    if (tableNode != null)
                    {
                        var resultNodes = tableNode.SelectNodes("//tbody/tr");
                        if (resultNodes != null)
                        {
                            var raceName = "";
                            foreach (var resultNode in resultNodes)
                            {
                                var isMain = resultNode.GetAttributeValue("data-main", "") == "1";
                                if (isMain)
                                {
                                    var raceNameNode = resultNode.ChildNodes.ElementAtOrDefault(4)?.Element("a");
                                    raceName = (raceNameNode?.GetDirectInnerText() ?? "").Trim();
                                    //raceName = (resultNode.ChildNodes.ElementAtOrDefault(4)?.ChildNodes.ElementAtOrDefault(0)?.InnerText ?? "").Trim();
                                }

                                int position = -1;
                                string positionNode = resultNode.ChildNodes.ElementAtOrDefault(1)?.InnerText ?? string.Empty;
                                if (string.IsNullOrEmpty(positionNode))
                                {
                                    continue;
                                }
                                Int32.TryParse(positionNode, out position);

                                var dateNode = resultNode.ChildNodes.ElementAtOrDefault(0)?.InnerText;
                                
                                // Skip classification results
                                if (string.IsNullOrEmpty(dateNode))
                                {
                                    continue;
                                }

                                DateTime parsedDate;
                                if (!DateTime.TryParseExact(dateNode, "dd.MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate))
                                {
                                    continue;
                                }

                                var pointsNodes = resultNode.ChildNodes.ElementAtOrDefault(7)?.InnerText;

                                output.WriteLine($"Race: {raceName}, Date: {dateNode}, Position: {positionNode}, Points: {pointsNodes}");

                                var cyclistRace = new CyclistRace
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    CyclistId = cyclist.CyclistId,
                                    Date = parsedDate,
                                    Race = new CyclistRaceDetails
                                    {
                                        Name = raceName
                                    },
                                    Result = position
                                };

                                var insertResponse = await container.CreateItemAsync(cyclistRace, new PartitionKey(cyclistRace.CyclistId));
                                if (insertResponse.StatusCode != System.Net.HttpStatusCode.Created)
                                {
                                    output.WriteLine($"Error inserting record {insertResponse.StatusCode} '{insertResponse}'");
                                }
                            }
                        }
                    }
                }

                current++;
            }

        }

    }

    [Fact]
    public async Task Seed_cyclists_from_UCI_ranking_page()
    {
        CosmosDbClientFactory factory = new();
        using var client = factory.CreateClient();
        var container = client.GetDatabase(DatabaseOperations.Database).GetContainer(DatabaseOperations.CyclistsContainer);
        
        HtmlWeb web = new();
        var doc = web.Load("https://www.procyclingstats.com/rankings.php");
        var table = doc.DocumentNode.SelectSingleNode("//table");

        var tableRecords = table.SelectNodes("//tbody/tr");
        foreach (var record in tableRecords)
        {
            var fullName = "";
            var proCyclingStatsUrl = "";
            var nameNode = record.ChildNodes.ElementAtOrDefault(3)?.SelectSingleNode("a");
            if (nameNode != null)
            {
                var firstName = nameNode.ChildNodes.ElementAtOrDefault(1)?.InnerText?.Trim();
                var surname = "";
                var surnameNode = nameNode.ChildNodes.FirstOrDefault();
                if (surnameNode != null)
                {
                    surname = surnameNode.InnerText.Trim();
                }
                if ((firstName + surname).Length > 0)
                {
                    fullName = $"{surname?.ToUpperInvariant()} {firstName}";
                }

                proCyclingStatsUrl = nameNode.GetAttributeValue("href", "");
            }

            var team = "";
            var teamNode = record.ChildNodes.ElementAtOrDefault(4)?.SelectSingleNode("a");
            if (teamNode != null)
            {
                team = teamNode.InnerText;
            }

            var country = "";

            Cyclist cyclist = new(Guid.NewGuid(), fullName, country, team, proCyclingStatsUrl);

            Cyclist created = await container.CreateItemAsync(cyclist, new PartitionKey(cyclist.Id));

            // output.WriteLine($"Id: {id} {fullName}");
            output.WriteLine($"Cyclist: {created}");
        }
    }

    [Fact]
    public async Task CreateDummyRace()
    {
        CosmosDbClientFactory factory = new();
        using var client = factory.CreateClient();
        var container = client.GetDatabase(DatabaseOperations.Database).GetContainer(DatabaseOperations.RacesContainer);

        var id = Guid.NewGuid();
        var race = new Race
        {
            Id = id.ToString(),
            RaceId = id.ToString(),
            Name = "Dummy Race",
            Season = 2024,
            StartDate = new DateTime(2024, 1, 1),
        };

        await container.CreateItemAsync(race, null, new ItemRequestOptions
        {
            PreTriggers = new List<string>() { "validateRace" },
            //PostTriggers = new List<string>() { "postInsertRace" }
        });

        output.WriteLine($"Created dummy race {id}");
    }

    [Fact]
    public async Task UpdateRace()
    {
        // This is intended to fire off a post-trigger
        CosmosDbClientFactory factory = new();
        using var client = factory.CreateClient();
        var container = client.GetDatabase(DatabaseOperations.Database).GetContainer(DatabaseOperations.RacesContainer);

        Race item = await container.ReadItemAsync<Race>("cf8605f8-0797-46b4-b202-872693b3a626", new PartitionKey("cf8605f8-0797-46b4-b202-872693b3a626"));
        if (item != null)
        {
            item.Name = item.Name + "U";
            await container.ReplaceItemAsync(item, item.Id, new PartitionKey(item.RaceId),
                new ItemRequestOptions
                {
                    PostTriggers = new List<string>() { "postUpdateRace" }
                });
        }
    }

    [Fact]
    public async Task RunStoredProcedure()
    {
        CosmosDbClientFactory factory = new();
        using var client = factory.CreateClient();
        var container = client.GetDatabase(DatabaseOperations.Database).GetContainer(DatabaseOperations.RacesContainer);

        var response = await container.Scripts.ExecuteStoredProcedureAsync<string>(
            "incrementRaceStageCount", 
            new PartitionKey("cf8605f8-0797-46b4-b202-872693b3a626"), 
            new[] { "cf8605f8-0797-46b4-b202-872693b3a626" },
            new StoredProcedureRequestOptions
            {
                EnableScriptLogging = true,
            });

        output.WriteLine(response);
    }

    [Fact]
    public async Task SetRaceFoundationYear()
    {
        CosmosDbClientFactory factory = new();
        using var client = factory.CreateClient();
        var container = client.GetDatabase(DatabaseOperations.Database).GetContainer(DatabaseOperations.RacesContainer);

        var response = await container.Scripts.ExecuteStoredProcedureAsync<string>(
            "setRaceFoundationYear", 
            new PartitionKey("cf8605f8-0797-46b4-b202-872693b3a626"), 
            new dynamic[] { "cf8605f8-0797-46b4-b202-872693b3a626", 1920 },
            new StoredProcedureRequestOptions
            {
                EnableScriptLogging = true,
            });

        output.WriteLine(response);
    }

    [Fact]
    public async Task Aggregate_race_names()
    {
        CosmosDbClientFactory factory = new();
        using var client = factory.CreateClient();
        var container = client.GetDatabase(DatabaseOperations.Database).GetContainer(DatabaseOperations.CyclistsContainer);

        Dictionary<string, Guid?> raceNames = new();

        var feed = container.GetItemQueryIterator<CyclistRace>("SELECT * FROM cyclists c WHERE c.type = 'cyclistRace'");
        while (feed.HasMoreResults)
        {
            var cyclistRaces = await feed.ReadNextAsync();
            foreach (var cyclistRace in cyclistRaces)
            {
                if (!raceNames.ContainsKey(cyclistRace.Race.Name))
                {
                    raceNames.Add(cyclistRace.Race.Name, null);
                }
            }
        }

        var racesContainer = client.GetDatabase(DatabaseOperations.Database).GetContainer("races");

        var racesFeed = racesContainer.GetItemQueryIterator<Race>("SELECT * FROM races r WHERE r.type = 'race'");
        while (racesFeed.HasMoreResults)
        {
            var racesResult = await racesFeed.ReadNextAsync();
            foreach (var race in racesResult)
            {
                await racesContainer.DeleteItemAsync<Race>(race.Id, new PartitionKey(race.RaceId));
            }
        }

        foreach (var raceName in raceNames)
        {
            var id = Guid.NewGuid();

            Race race = new()
            {
                Id = id.ToString(),
                RaceId = id.ToString(),
                Name = raceName.Key,
                Season = 2024
            };

            var raceResult = await racesContainer.CreateItemAsync<Race>(race);

            output.WriteLine($"Created race '{raceName}' ({id}) {raceResult.StatusCode}");

            raceNames[raceName.Key] = id;
        }

        feed = container.GetItemQueryIterator<CyclistRace>("SELECT * FROM cyclists c WHERE c.type = 'cyclistRace'");
        while (feed.HasMoreResults)
        {
            var cyclistRaces = await feed.ReadNextAsync();
            foreach (var cyclistRace in cyclistRaces)
            {
                var raceName = cyclistRace.Race?.Name ?? "";
                var raceDetails = raceNames[raceName];
                if (raceDetails.HasValue)
                {
                    cyclistRace.Race = cyclistRace.Race ?? new CyclistRaceDetails();
                    cyclistRace.Race.Id = raceDetails.Value.ToString();
                    
                    await container.ReplaceItemAsync(cyclistRace, cyclistRace.Id, new PartitionKey(cyclistRace.CyclistId));
                }
            }
        }
    }
}
