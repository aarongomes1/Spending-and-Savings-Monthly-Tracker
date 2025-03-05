
namespace SpendingAndSavingsMonthlyTracker.Models.ProcessedModels
{
    public class SpendingThisPeriod
    {
        public required string SpendingCategory { get; init; }
        public required string SpendingPlace { get; init; }
        public required decimal TotalAmountSpent { get; init; }
        public required int NumberOfVisits { get; init; }
    }
}
