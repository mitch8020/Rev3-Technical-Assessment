using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Rev3_Technical_Assessment.ViewModels;

public sealed class DatasetUploadViewModel
{
    [Required]
    public string Name { get; set; } = "";

    public string? Description { get; set; }

    public string? Tags { get; set; }

    [Required]
    public IFormFile CsvFile { get; set; } = default!;
}