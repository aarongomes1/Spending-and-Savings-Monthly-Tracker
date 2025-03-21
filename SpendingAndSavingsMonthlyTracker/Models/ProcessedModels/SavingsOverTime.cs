
namespace SpendingAndSavingsMonthlyTracker.Models.ProcessedModels
{
    public class SavingsOverTime
    {
        public required string Account {  get; init; }
        public required string ReportingPeriod { get; init; }
        public required decimal AmountAdded { get; init; }
    }
}
