
namespace CommonClasses.Structure
{
    public class ReportingPeriod
    {
        public Guid ReportingPeriodKey { get; init; } = Guid.NewGuid();
        public required DateTime StartDate {  get; init; }
        public required DateTime EndDate { get; init; }

        public List<SavingsTransaction> SavingsTransactionsThisPeriod { get; } = [];
        public List<Spending> SpendingTransactionsThisPeriod { get; } = [];

        public override bool Equals(object? obj)
        {
            if (obj is null || obj.GetType() != typeof(ReportingPeriod))
            {
                return false;
            }

            var other = (obj as ReportingPeriod)!;

            return other.StartDate.Equals(StartDate) && other.EndDate.Equals(EndDate);
        }

        public override int GetHashCode()
        {
            return ReportingPeriodKey.GetHashCode();
        }

        public override string ToString()
        {
            var startDate = StartDate.ToString("dd/MM/yyyy");
            var endDate = EndDate.ToString("dd/MM/yyyy");

            var formattedDates = $"{startDate} - {endDate}";

            return formattedDates;
        }
    }
}
