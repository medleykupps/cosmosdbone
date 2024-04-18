using Microsoft.Azure.Cosmos;
using System.Net.Http;

namespace CosmosDbOneApp
{
    public class CosmosDbClientFactory
    {
        public CosmosClient CreateClient()
        {
            var options = new CosmosClientOptions()
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
}
