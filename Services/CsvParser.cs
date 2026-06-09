using CsvHelper;
using CsvHelper.Configuration;
using Rev3_Technical_Assessment.Models;
using System.Globalization;

namespace Rev3_Technical_Assessment.Services;

public static class CsvParser
{
    public static List<CallRecord> Parse(Stream stream)
    {
        using var reader = new StreamReader(stream);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        csv.Context.RegisterClassMap<CallRecordMap>();
        return csv.GetRecords<CallRecord>().ToList();
    }
}

public sealed class CallRecordMap : ClassMap<CallRecord>
{
    public CallRecordMap()
    {
        Map(m => m.NurseName).Name("nurse name", "Nurse Name", "NurseName", "Name");
        Map(m => m.PatientStatus).Name("patient's status", "Patient Status", "PatientStatus", "status", "StatusName");
        Map(m => m.CallMinutes).Name("call minutes", "Call Minutes", "CallMinutes", "minutes");
        Map(m => m.CallMadeAt).Name("when the call was made", "Call Made At", "CallMadeAt", "call time", "timestamp", "CreatedOn");
    }
}
