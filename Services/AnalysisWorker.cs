using Rev3_Technical_Assessment.Models;

namespace Rev3_Technical_Assessment.Services;

public sealed class AnalysisWorker : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<AnalysisWorker> _logger;

    public AnalysisWorker(IServiceProvider services, ILogger<AnalysisWorker> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _services.CreateScope();

                var repo = scope.ServiceProvider.GetRequiredService<DatasetRepository>();
                var blobStorage = scope.ServiceProvider.GetRequiredService<BlobStorageService>();

                var queued = await repo.GetQueuedAsync(stoppingToken);

                foreach (var doc in queued)
                {
                    await ProcessAsync(doc, repo, blobStorage, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Background analysis worker failed.");
            }

            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
        }
    }

    private static async Task ProcessAsync(
        DatasetDocument doc,
        DatasetRepository repo,
        BlobStorageService blobStorage,
        CancellationToken ct)
    {
        try
        {
            doc.Status = "Running";
            doc.Progress = 10;
            doc.StatusMessage = "Downloading CSV";
            doc.StartedAt = DateTimeOffset.UtcNow;
            await repo.UpdateAsync(doc, ct);

            await Task.Delay(750, ct);

            await using var stream = await blobStorage.DownloadAsync(doc.BlobName, ct);

            doc.Progress = 30;
            doc.StatusMessage = "Parsing CSV";
            await repo.UpdateAsync(doc, ct);

            await Task.Delay(750, ct);

            var records = CsvParser.Parse(stream);

            doc.Progress = 55;
            doc.StatusMessage = "Training ML.NET clustering model";
            await repo.UpdateAsync(doc, ct);

            await Task.Delay(1000, ct);

            var analysis = new CallAnalysisService().Analyze(records);

            doc.Progress = 85;
            doc.StatusMessage = "Generating heatmap data";
            await repo.UpdateAsync(doc, ct);

            await Task.Delay(1000, ct);

            doc.Result = analysis;
            doc.Status = "Completed";
            doc.Progress = 100;
            doc.StatusMessage = "Analysis complete";
            doc.CompletedAt = DateTimeOffset.UtcNow;

            await repo.UpdateAsync(doc, ct);
        }
        catch (Exception ex)
        {
            doc.Status = "Failed";
            doc.StatusMessage = ex.Message;
            await repo.UpdateAsync(doc, ct);
        }
    }
}
