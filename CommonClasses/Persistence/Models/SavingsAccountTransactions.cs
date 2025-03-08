namespace CommonClasses.Persistence.Models
{
    internal class SavingsAccountTransactions
    {
        public required string SavingsAccountKey { get; init; }
        public required string ReportingPeriodKey { get; init; }
        public required decimal Change { get; init; }
        public required string TransactionDate { get; init; }
        public int? CountsToISALimit { get; init; }
        public required decimal BalanceAfterTransaction { get; init; }
    }
}
