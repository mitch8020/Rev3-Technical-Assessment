namespace Rev3_Technical_Assessment.Models
{
    public sealed class CallRecord
    {
        public string NurseName { get; set; } = "";
        public string PatientStatus { get; set; } = "";
        public float CallMinutes { get; set; }
        public DateTime CallMadeAt { get; set; }
    }
}
