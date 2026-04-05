namespace TransactionsToSpendingSavings.Models
{
    internal class SavingsInput
    {
        public required string AccountName { get; init; }
        public required decimal Deposit { get; init; }
        public required string TransactionDate { get; init; }
        public bool? IsISA { get; init; }
        public bool? BalanceCountsToISALimit { get; init; }
    }
}
