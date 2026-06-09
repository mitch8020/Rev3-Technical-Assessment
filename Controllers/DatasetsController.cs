using Microsoft.AspNetCore.Mvc;
using Rev3_Technical_Assessment.Models;
using Rev3_Technical_Assessment.Services;
using Rev3_Technical_Assessment.ViewModels;

namespace Rev3_Technical_Assessment.Controllers;

public sealed class DatasetsController : Controller
{
    private readonly BlobStorageService _blobStorage;
    private readonly DatasetRepository _repo;

    public DatasetsController(BlobStorageService blobStorage, DatasetRepository repo)
    {
        _blobStorage = blobStorage;
        _repo = repo;
    }

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var datasets = await _repo.GetAllAsync(ct);
        return View(datasets);
    }

    [HttpGet]
    public IActionResult Upload()
    {
        return View(new DatasetUploadViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(DatasetUploadViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return View(model);

        if (!model.CsvFile.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(nameof(model.CsvFile), "Only CSV files are allowed.");
            return View(model);
        }

        var blobName = await _blobStorage.UploadCsvAsync(model.CsvFile, ct);

        var doc = new DatasetDocument
        {
            Name = model.Name,
            Description = model.Description,
            Tags = model.Tags?
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList() ?? [],
            OriginalFileName = model.CsvFile.FileName,
            BlobName = blobName,
            Status = "Queued",
            Progress = 0,
            StatusMessage = "Waiting to start analysis"
        };

        await _repo.CreateAsync(doc, ct);

        return RedirectToAction(nameof(Details), new { id = doc.id });
    }

    public async Task<IActionResult> Details(string id, CancellationToken ct)
    {
        var doc = await _repo.GetAsync(id, ct);
        return View(doc);
    }

    [HttpGet]
    public async Task<IActionResult> Progress(string id, CancellationToken ct)
    {
        var doc = await _repo.GetAsync(id, ct);

        return Json(new
        {
            id = doc.id,
            status = doc.Status,
            progress = doc.Progress,
            message = doc.StatusMessage,
            result = doc.Result
        });
    }
}
