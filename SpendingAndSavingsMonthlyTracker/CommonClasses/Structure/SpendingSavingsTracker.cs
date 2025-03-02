namespace CommonClasses.Structure
{
    public class SpendingSavingsTracker
    {
        public List<SavingsAccount> SavingsAccounts { get; init; }
        public List<SpendingCategory> SpendingCategories { get; init; }
        public List<ReportingPeriod> ReportingPeriods { get; init; }

        public SpendingSavingsTracker(List<SavingsAccount> savingAccounts, List<ReportingPeriod> reportingPeriods, List<SpendingCategory> spendingCategories)
        {
            SavingsAccounts = savingAccounts;
            ReportingPeriods = reportingPeriods;
            SpendingCategories = spendingCategories;
        }
    }
}
