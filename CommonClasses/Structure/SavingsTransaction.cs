
namespace CommonClasses.Structure
{
    public class SavingsTransaction
    {
        public required SavingsAccount SavingsAccount { get; init; }

        public required ReportingPeriod ReportingPeriod { get; init; }

        public required decimal Change {  get; init; }

        public required DateOnly TransactionDate { get; init; }


        public bool? CountsToISALimit { get; init; }

        public required decimal BalanceAfterTransaction { get; init; }
    }
}
