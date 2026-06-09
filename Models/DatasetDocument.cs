namespace Rev3_Technical_Assessment.Models
{
    public sealed class DatasetDocument
    {
        public string id { get; set; } = Guid.NewGuid().ToString();
        public string Type { get; set; } = "dataset";

        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public List<string> Tags { get; set; } = [];

        public string OriginalFileName { get; set; } = "";
        public string BlobName { get; set; } = "";

        public string Status { get; set; } = "Queued";
        public int Progress { get; set; } = 0;
        public string? StatusMessage { get; set; }

        public AnalysisResult? Result { get; set; }

        public DateTimeOffset UploadedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? StartedAt { get; set; }
        public DateTimeOffset? CompletedAt { get; set; }
    }
}
