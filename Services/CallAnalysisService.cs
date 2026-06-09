using Microsoft.ML;
using Microsoft.ML.Data;
using Rev3_Technical_Assessment.Models;

namespace Rev3_Technical_Assessment.Services;

public sealed class CallAnalysisService
{
    public AnalysisResult Analyze(List<CallRecord> records)
    {
        if (records.Count == 0)
            return new AnalysisResult();

        var ml = new MLContext(seed: 1);

        var rows = records.Select(r =>
        {
            var hour = r.CallMadeAt.Hour + r.CallMadeAt.Minute / 60f;

            return new MlCallInput
            {
                HourSin = MathF.Sin(2f * MathF.PI * hour / 24f),
                HourCos = MathF.Cos(2f * MathF.PI * hour / 24f),
                CallMinutes = r.CallMinutes
            };
        }).ToList();

        var data = ml.Data.LoadFromEnumerable(rows);

        var pipeline = ml.Transforms.Concatenate(
                "Features",
                nameof(MlCallInput.HourSin),
                nameof(MlCallInput.HourCos),
                nameof(MlCallInput.CallMinutes)
            )
            .Append(ml.Transforms.NormalizeMinMax("Features"))
            .Append(ml.Clustering.Trainers.KMeans(
                featureColumnName: "Features",
                numberOfClusters: Math.Min(4, Math.Max(2, records.Count / 5))
            ));

        var model = pipeline.Fit(data);
        var predictions = model.Transform(data);

        var predictedRows = ml.Data
            .CreateEnumerable<MlCallPrediction>(predictions, reuseRowObject: false)
            .ToList();

        var enriched = records.Zip(predictedRows, (record, prediction) => new
        {
            Record = record,
            ClusterId = prediction.PredictedLabel
        }).ToList();

        var clusters = enriched
            .GroupBy(x => x.ClusterId)
            .Select(g => new ClusterSummary
            {
                ClusterId = g.Key,
                Count = g.Count(),
                AverageHour = g.Average(x => x.Record.CallMadeAt.Hour + x.Record.CallMadeAt.Minute / 60.0),
                AverageCallMinutes = g.Average(x => x.Record.CallMinutes)
            })
            .OrderBy(x => x.ClusterId)
            .ToList();

        var heatmap = records
            .GroupBy(r => new
            {
                Hour = r.CallMadeAt.Hour,
                MinuteBucketStart = ((int)r.CallMinutes / 5) * 5
            })
            .Select(g => new HeatmapCell
            {
                Hour = g.Key.Hour,
                MinuteBucketStart = g.Key.MinuteBucketStart,
                Count = g.Count(),
                AverageMinutes = g.Average(x => x.CallMinutes)
            })
            .OrderBy(x => x.MinuteBucketStart)
            .ThenBy(x => x.Hour)
            .ToList();

        var hourlySummaries = records
            .GroupBy(r => r.CallMadeAt.Hour)
            .Select(g => new HourlySummary
            {
                Hour = g.Key,
                Count = g.Count(),
                AverageCallMinutes = g.Average(x => x.CallMinutes)
            })
            .OrderBy(x => x.Hour)
            .ToList();

        return new AnalysisResult
        {
            Clusters = clusters,
            Heatmap = heatmap,
            HourlySummaries = hourlySummaries
        };
    }

    private sealed class MlCallInput
    {
        public float HourSin { get; set; }
        public float HourCos { get; set; }
        public float CallMinutes { get; set; }
    }

    private sealed class MlCallPrediction
    {
        [ColumnName("PredictedLabel")]
        public uint PredictedLabel { get; set; }

        [ColumnName("Score")]
        public float[] Score { get; set; } = [];
    }
}
