using Azure.Storage.Blobs;

namespace Rev3_Technical_Assessment.Services;

public sealed class BlobStorageService
{
    private readonly BlobContainerClient _container;

    public BlobStorageService(IConfiguration config)
    {
        var connectionString = config["Storage:ConnectionString"];
        var containerName = config["Storage:ContainerName"];

        // Azurite doesn't support the newest service API versions used by the
        // Azure SDK. When running against a local emulator detect it from the
        // connection string and force a compatible service version.
        if (UsesAzurite(connectionString))
        {
            var options = new BlobClientOptions(BlobClientOptions.ServiceVersion.V2020_10_02);
            var service = new BlobServiceClient(connectionString, options);
            _container = service.GetBlobContainerClient(containerName);
        }
        else
        {
            _container = new BlobContainerClient(connectionString, containerName);
        }

        _container.CreateIfNotExists();
    }

    private static bool UsesAzurite(string? connectionString)
    {
        return !string.IsNullOrWhiteSpace(connectionString) &&
            (connectionString.Contains("UseDevelopmentStorage=true", StringComparison.OrdinalIgnoreCase) ||
             connectionString.Contains("127.0.0.1", StringComparison.OrdinalIgnoreCase) ||
             connectionString.Contains("localhost", StringComparison.OrdinalIgnoreCase) ||
             connectionString.Contains("devstoreaccount1", StringComparison.OrdinalIgnoreCase));
    }

    public async Task<string> UploadCsvAsync(IFormFile file, CancellationToken ct)
    {
        var blobName = $"{Guid.NewGuid()}/{Path.GetFileName(file.FileName)}";
        var blob = _container.GetBlobClient(blobName);

        await using var stream = file.OpenReadStream();
        await blob.UploadAsync(stream, overwrite: false, cancellationToken: ct);

        return blobName;
    }

    public async Task<Stream> DownloadAsync(string blobName, CancellationToken ct)
    {
        var blob = _container.GetBlobClient(blobName);
        var response = await blob.DownloadStreamingAsync(cancellationToken: ct);
        return response.Value.Content;
    }
}
