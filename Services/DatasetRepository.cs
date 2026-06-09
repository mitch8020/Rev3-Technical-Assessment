using Microsoft.Azure.Cosmos;
using Rev3_Technical_Assessment.Models;

namespace Rev3_Technical_Assessment.Services;

public sealed class DatasetRepository
{
    private readonly Container _container;

    public DatasetRepository(IConfiguration config)
    {
        var endpoint = config["Cosmos:Endpoint"];
        var key = config["Cosmos:Key"];
        var databaseName = config["Cosmos:DatabaseName"];
        var containerName = config["Cosmos:ContainerName"];

        var client = new CosmosClient(endpoint, key);
        var database = client.CreateDatabaseIfNotExistsAsync(databaseName).GetAwaiter().GetResult();

        _container = database.Database.CreateContainerIfNotExistsAsync(
            containerName,
            partitionKeyPath: "/Type"
        ).GetAwaiter().GetResult();
    }

    public async Task CreateAsync(DatasetDocument doc, CancellationToken ct)
    {
        await _container.CreateItemAsync(doc, new PartitionKey(doc.Type), cancellationToken: ct);
    }

    public async Task<DatasetDocument> GetAsync(string id, CancellationToken ct)
    {
        var response = await _container.ReadItemAsync<DatasetDocument>(
            id,
            new PartitionKey("dataset"),
            cancellationToken: ct
        );

        return response.Resource;
    }

    public async Task UpdateAsync(DatasetDocument doc, CancellationToken ct)
    {
        await _container.UpsertItemAsync(doc, new PartitionKey(doc.Type), cancellationToken: ct);
    }

    public async Task<List<DatasetDocument>> GetQueuedAsync(CancellationToken ct)
    {
        var query = new QueryDefinition(
            "SELECT * FROM c WHERE c.Type = 'dataset' AND c.Status = 'Queued'"
        );

        var iterator = _container.GetItemQueryIterator<DatasetDocument>(query);
        var results = new List<DatasetDocument>();

        while (iterator.HasMoreResults)
        {
            var page = await iterator.ReadNextAsync(ct);
            results.AddRange(page);
        }

        return results;
    }

    public async Task<List<DatasetDocument>> GetAllAsync(CancellationToken ct)
    {
        var query = new QueryDefinition(
            "SELECT * FROM c WHERE c.Type = 'dataset' ORDER BY c.UploadedAt DESC"
        );

        var iterator = _container.GetItemQueryIterator<DatasetDocument>(query);
        var results = new List<DatasetDocument>();

        while (iterator.HasMoreResults)
        {
            var page = await iterator.ReadNextAsync(ct);
            results.AddRange(page);
        }

        return results;
    }
}