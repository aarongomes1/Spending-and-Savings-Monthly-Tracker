namespace CommonClasses.Persistence.Models
{
    internal class ReportingPeriod
    {
        public required string ReportingPeriodKey { get; init; }
        public required string StartDate { get; init; }
        public required string EndDate { get; init; }
    }
}
