
namespace SpendingAndSavingsMonthlyTracker.Models.ProcessedModels
{
    public class SpendingOverTime
    {
        public required string SpendingCategory { get; init; }
        public required string ReportingPeriod { get; init; }
        public required decimal Amount { get; init; }
    }
}
