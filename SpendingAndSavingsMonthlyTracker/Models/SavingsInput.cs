
namespace SpendingAndSavingsMonthlyTracker.Models
{
    internal class SavingsInput
    {
        public required string AccountName { get; init; }
        public required decimal Deposit { get; init; }
        public required string TransactionDate { get; init; }
        public string? IsISA { get; init; }
        public string? BalanceCountsToISALimit { get; init; }
    }
}
