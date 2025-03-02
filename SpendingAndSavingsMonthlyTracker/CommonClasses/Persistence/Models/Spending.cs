namespace CommonClasses.Persistence.Models
{
    internal class Spending
    {
        public required string ReportingPeriodKey { get; init; }
        public required string SpendingPlaceKey { get; init; }
        public required decimal Amount { get; init; }
        public required int NumberOfTransactions { get; init; }
    }
}
