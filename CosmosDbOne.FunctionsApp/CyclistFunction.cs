using System.Net;
using CosmosDbOneApp.Core;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace CosmosDbOne.FunctionsApp;

public class CyclistFunction
{
    private readonly ILogger _logger;
    private readonly ICyclistRepository _repository;

    public CyclistFunction(ILoggerFactory loggerFactory, ICyclistRepository repository)
    {
        _logger = loggerFactory.CreateLogger<CyclistFunction>();
        _repository = repository;
    }

    [Function("Cyclists")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");

        var results = await _repository.Search();
        _logger.LogInformation("Found {0} cyclists", results.Count());

        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

        response.WriteString("Welcome to Azure Functions!");

        return response;
    }
}
