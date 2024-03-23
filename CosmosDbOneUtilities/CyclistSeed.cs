using CosmosDbOneApp;
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
}
