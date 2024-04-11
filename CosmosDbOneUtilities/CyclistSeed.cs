using CosmosDbOneApp;
using HtmlAgilityPack;
using Microsoft.Azure.Cosmos;
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
            var id = "";
            var idNode = record.ChildNodes.FirstOrDefault();
            if (idNode != null)
            {
                id = idNode.InnerText;
            }

            var fullName = "";
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
            }

            var team = "";
            var teamNode = record.ChildNodes.ElementAtOrDefault(4)?.SelectSingleNode("a");
            if (teamNode != null)
            {
                team = teamNode.InnerText;
            }

            var country = "";

            Cyclist cyclist = new(Guid.NewGuid(), fullName, country, team);

            Cyclist created = await container.CreateItemAsync(cyclist, new PartitionKey(cyclist.Id));

            // output.WriteLine($"Id: {id} {fullName}");
            output.WriteLine($"Cyclist: {created}");
        }


    }
}
