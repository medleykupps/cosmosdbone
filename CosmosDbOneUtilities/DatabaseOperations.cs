using CosmosDbOneApp;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Scripts;
using Xunit;
using Xunit.Abstractions;

namespace CosmosDbOneUtilities;

public class DatabaseOperations(ITestOutputHelper output)
{
    public const string Database = "cosmosdbone";
    public const string CyclistsContainer = "cyclists";
    public const string RacesContainer = "races";

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

    [Fact]
    public async Task DeleteCyclistContainer()
    {
        CosmosDbClientFactory factory = new();
        using var client = factory.CreateClient();
        var db = client.GetDatabase(Database);
        var container = db.GetContainer(CyclistsContainer);
        var response = await container.DeleteContainerAsync();

        output.WriteLine($"{output}");        
    }

    [Fact]
    public async Task RecreateRacesContainer()
    {
        CosmosDbClientFactory cosmosDbClientFactory = new();
        using var client = cosmosDbClientFactory.CreateClient();
        var db = client.GetDatabase(Database);

        try
        {
            var container = db.GetContainer("races");
            await container.DeleteContainerAsync();
        }
        catch { }

        var response = await db.CreateContainerAsync(
            id: "races",
            partitionKeyPath: "/raceId",
            throughput: 400);

        output.WriteLine($"{response.StatusCode}");

        await InitRacesStoredProcedures(db);
    }

    [Fact]
    public async Task CreateRaceStoredProcedure()
    {
        CosmosDbClientFactory cosmosDbClientFactory = new();
        using var client = cosmosDbClientFactory.CreateClient();
        var db = client.GetDatabase(Database);

        await InitRacesStoredProcedures(db);
    }

    private async Task InitRacesStoredProcedures(Database database)
    {
        var container = database.GetContainer(RacesContainer);

        await container.Scripts.DeleteStoredProcedureAsync("setRaceFoundationYear");

        var response = await container.Scripts.CreateStoredProcedureAsync(new StoredProcedureProperties 
        { 
            Id = "setRaceFoundationYear",
            Body = @"
                function setRaceFoundationYear(raceId, foundationYear) {
                    var collection = getContext().getCollection();

                    console.log(`Incoming parameters '${raceId}', '${foundationYear}'`);

                    var url = `${collection.getAltLink()}/docs/${raceId}`;
                    console.log(`Retrieving race at '${url}'`);

                    collection.readDocument(url, function(err, race) {
                        if (err) throw err;

                        console.log(`Found race '${race.name}'`);
                        
                        race.foundationYear = foundationYear;
                        
                        collection.replaceDocument(race._self, race, function(err) {
                           if (err) throw err;            
                        });
                        
                    });
                }"
        });
    }

}
