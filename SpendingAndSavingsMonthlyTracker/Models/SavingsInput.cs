
namespace SpendingAndSavingsMonthlyTracker.Models
{
    internal class SavingsInput
    {
        public required string AccountName { get; init; }
        public required decimal Deposit { get; init; }
    }
}
