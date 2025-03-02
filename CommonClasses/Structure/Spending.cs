
namespace CommonClasses.Structure
{
    public class Spending
    {
        public required ReportingPeriod ReportingPeriod { get; init; }
        public required SpendingPlace SpendingPlace { get; init; }
        public required decimal Amount { get; init; }
        public required int NumberOfTransactions { get; init; }
    }
}
