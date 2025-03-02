
namespace CommonClasses.Structure
{
    public class ReportingPeriod
    {
        public required Guid ReportingPeriodKey { get; init; } = Guid.NewGuid();
        public required DateTime StartDate {  get; init; }
        public required DateTime EndDate { get; init; }

        public List<SavingsTransaction> SavingsTransactionsThisPeriod { get; } = [];
        public List<Spending> SpendingTransactionsThisPeriod { get; } = [];
    }
}
