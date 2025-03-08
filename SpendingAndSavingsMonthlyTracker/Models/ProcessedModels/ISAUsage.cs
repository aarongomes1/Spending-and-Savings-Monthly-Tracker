
namespace SpendingAndSavingsMonthlyTracker.Models.ProcessedModels
{
    public class ISAUsage
    {
        public required string Month {  get; init; }
        public required decimal TotalLimitUsed { get; init; }
    }
}
