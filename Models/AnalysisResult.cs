namespace Rev3_Technical_Assessment.Models
{
    public sealed class AnalysisResult
    {
        public List<ClusterSummary> Clusters { get; set; } = [];
        public List<HeatmapCell> Heatmap { get; set; } = [];
        public List<HourlySummary> HourlySummaries { get; set; } = [];
    }

    public sealed class ClusterSummary
    {
        public uint ClusterId { get; set; }
        public int Count { get; set; }
        public double AverageHour { get; set; }
        public double AverageCallMinutes { get; set; }
    }

    public sealed class HeatmapCell
    {
        public int Hour { get; set; }
        public int MinuteBucketStart { get; set; }
        public int Count { get; set; }
        public double AverageMinutes { get; set; }
    }

    public sealed class HourlySummary
    {
        public int Hour { get; set; }
        public int Count { get; set; }
        public double AverageCallMinutes { get; set; }
    }
}
