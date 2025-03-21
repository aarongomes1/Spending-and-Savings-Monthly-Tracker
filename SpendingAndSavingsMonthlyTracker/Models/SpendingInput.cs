
namespace SpendingAndSavingsMonthlyTracker.Models
{
    internal class SpendingInput
    {
        public required string Name { get; init; }
        public required string Category { get; init; }
        public required decimal? Debit { get; init; }
        public required decimal? Refund { get; init; }
    }
}
