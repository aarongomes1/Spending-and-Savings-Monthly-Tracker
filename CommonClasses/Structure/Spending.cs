
namespace CommonClasses.Structure
{
    public class Spending
    {
        public required ReportingPeriod ReportingPeriod { get; init; }

        public required SpendingPlace SpendingPlace { get; init; }

        public required decimal Amount { get; set; }

        public required int NumberOfTransactions { get; set; }
    }
}
