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
