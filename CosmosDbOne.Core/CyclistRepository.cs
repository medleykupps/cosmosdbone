using CosmosDbOne.Core.Cyclists;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmosDbOneApp.Core
{
    public interface ICyclistRepository
    {
        Task<Cyclist> GetSingle(Guid id);
        Task<IEnumerable<Cyclist>> Search();
    }

    public class CyclistRepository : ICyclistRepository
    {
        private readonly CosmosClient _client;
        private readonly ILogger<CyclistRepository> _logger;

        private Container Cyclists
        {
            get
            {
                return _cyclists ??= _client.GetDatabase("cosmosdbone").GetContainer("cyclists");
            }
        }

        private Container _cyclists;

        public CyclistRepository(CosmosClient client, ILoggerFactory loggerFactory)
        {
            _client = client;
            _logger = loggerFactory.CreateLogger<CyclistRepository>();
        }

        public async Task<Cyclist> GetSingle(Guid id)
        {
            var item = await Cyclists.ReadItemAsync<Cyclist>(
                id: id.ToString(),
                partitionKey: new PartitionKey(id.ToString())
            );

            return item;
        }

        public async Task<IEnumerable<Cyclist>> Search()
        {
            var cyclists = new List<Cyclist>();

            var query = new QueryDefinition("SELECT * FROM cyclists c");

            var feed = Cyclists.GetItemQueryIterator<Cyclist>(query);
            while (feed.HasMoreResults)
            {
                var response = await feed.ReadNextAsync();

                _logger.LogInformation("Activity Id: {0}; Request charge: {1}", response.ActivityId, response.RequestCharge);

                cyclists.AddRange(response);
            }

            return cyclists;
        }
    }
}
