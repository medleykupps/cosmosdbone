using Microsoft.Azure.Cosmos;
using System.Text.Json;

namespace CosmosDbOneApp;

internal class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Starting CosmosDbOne...");


        //Container container = await db.CreateContainerIfNotExistsAsync(id: "cyclists", partitionKeyPath: "/country");

        //var item = new
        //{
        //    id = Guid.NewGuid().ToString(),
        //    name = "Sam Welsford",
        //    country = "Australia"
        //};
        //var response = await container.UpsertItemAsync(item);

        //var responseJson = JsonSerializer.Serialize(response, options: new JsonSerializerOptions()
        //{
        //    WriteIndented = true
        //});
        //Console.WriteLine(responseJson);

        Console.WriteLine("Done");
    }
}

public class CosmosDbClientFactory
{
    public CosmosClient CreateClient()
    {
        CosmosClientOptions options = new()
        {
            HttpClientFactory = () => new HttpClient(
                new HttpClientHandler()
                {
                    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                }
            ),
            SerializerOptions = new CosmosSerializationOptions
            {
                IgnoreNullValues = true,
                PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
            },
            ConnectionMode = ConnectionMode.Gateway
        };

        return new CosmosClient(
            accountEndpoint: "https://localhost:8081/",
            authKeyOrResourceToken: "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==",
            clientOptions: options
        );
    }
}
