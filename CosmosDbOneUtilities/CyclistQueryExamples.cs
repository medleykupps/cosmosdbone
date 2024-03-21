using CosmosDbOneApp;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace CosmosDbOneUtilities;

public class CyclistQueryExamples(ITestOutputHelper output)
{
    private async Task OutputFeed(FeedIterator<Cyclist> feed)
    {
        while (feed.HasMoreResults)
        {
            FeedResponse<Cyclist> response = await feed.ReadNextAsync();

            output.WriteLine("Activity Id: {0}", response.ActivityId);
            output.WriteLine("Request charge: {0}", response.RequestCharge);

            foreach (var cyclist in response)
            {
                output.WriteLine("Cyclist {0}", cyclist);
            }

        }
    }

    private Container GetCyclistContainer(CosmosClient client)
    {
        var db = client.GetDatabase(DatabaseOperations.Database);

        return db.GetContainer(DatabaseOperations.CyclistsContainer);
    }

    [Fact]
    public async Task GetItemQueryIterator_returns_cyclists()
    {
        CosmosDbClientFactory factory = new();
        using var client = factory.CreateClient();
        var container = GetCyclistContainer(client);

        FeedIterator<Cyclist> feed = 
            container.GetItemQueryIterator<Cyclist>("SELECT * FROM cyclists");

        await OutputFeed(feed);
    }

    [Fact]
    public async Task GetItemQueryIterator_QueryDefinition_returns_cyclists()
    {
        CosmosDbClientFactory factory = new();
        using var client = factory.CreateClient();
        var container = GetCyclistContainer(client);

        var query = new QueryDefinition("SELECT * FROM cyclists c WHERE c.country = @country")
            .WithParameter("@country", "AUSTRALIA");

        FeedIterator<Cyclist> feed = 
            container.GetItemQueryIterator<Cyclist>(query);
        await OutputFeed(feed);
    }
}
