using CosmosDbOneApp;
using Microsoft.Azure.Cosmos;
using System.Text.Json;
using Xunit;
using Xunit.Abstractions;

namespace CosmosDbOneUtilities;

public class CyclistReadExamples(ITestOutputHelper output)
{
    const string Id = "8ddabbb4-e675-406f-8ce6-02e763586792";
    const string Country = "AUSTRALIA";

    [Fact]
    public async Task ReadItemAsync_reads_a_cyclist_asynchronously()
    {
        CosmosDbClientFactory factory = new();

        using var client = factory.CreateClient();

        var db = client.GetDatabase(DatabaseOperations.Database);

        Container container = db.GetContainer(DatabaseOperations.CyclistsContainer);

        ItemResponse<Cyclist> response = await container.ReadItemAsync<Cyclist>(
        // Cyclist cyclist = await container.ReadItemAsync<Cyclist>(
            id: Id,
            partitionKey: new PartitionKey(Country)
        );

        Cyclist cyclist = response.Resource;

        //output.WriteLine("Retrieved cyclist {0}", JsonSerializer.Serialize(cyclist));
        output.WriteLine("Retrieved response {0}", JsonSerializer.Serialize(response));
    }

    // `ReadItemStreamAsync` is handy when the document does not need to be deserialised
    // immediately by the calling code and can be passed off to other code to do this.
    // Doing it this way will improve performance.
    [Fact]
    public async Task ReadItemStreamAsync_reads_a_cyclist_as_a_stream()
    {
        CosmosDbClientFactory factory = new();

        using var client = factory.CreateClient();

        var db = client.GetDatabase(DatabaseOperations.Database);

        Container container = db.GetContainer(DatabaseOperations.CyclistsContainer);

        using ResponseMessage response = await container.ReadItemStreamAsync(
            id: Id,
            partitionKey: new PartitionKey(Country)
        );

        using StreamReader reader = new(response.Content);

        var document = await reader.ReadToEndAsync();

        output.WriteLine("Document read from stream '{0}'", document);

    }
}