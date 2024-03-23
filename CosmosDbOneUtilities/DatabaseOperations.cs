using CosmosDbOneApp;
using Microsoft.Azure.Cosmos;
using Xunit;

namespace CosmosDbOneUtilities;

public class DatabaseOperations
{
    public const string Database = @"cosmosdbone";
    public const string CyclistsContainer = @"cyclists";

    [Fact]
    public async Task CreateDatabase()
    {
        CosmosDbClientFactory factory = new();

        using var client = factory.CreateClient();

        Database db = await client.CreateDatabaseIfNotExistsAsync(
            id: Database,
            throughput: 400
        );
    }

    [Fact]
    public async Task CreateContainer()
    {
        CosmosDbClientFactory factory = new();

        using var client = factory.CreateClient();

        var db = client.GetDatabase(Database);

        Container container = await db.CreateContainerIfNotExistsAsync(
            id: CyclistsContainer, 
            partitionKeyPath: "/cyclistId", 
            throughput: 400
        );
    }

}
