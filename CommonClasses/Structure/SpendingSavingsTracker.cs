using CommonClasses.Persistence;

namespace CommonClasses.Structure
{
    public class SpendingSavingsTracker
    {
        public List<SavingsAccount> SavingsAccounts { get; init; }
        public List<SpendingCategory> SpendingCategories { get; init; }
        public List<ReportingPeriod> ReportingPeriods { get; init; }

        public readonly Creator Creator;

        public SpendingSavingsTracker(List<SavingsAccount> savingAccounts, List<ReportingPeriod> reportingPeriods, List<SpendingCategory> spendingCategories)
        {
            SavingsAccounts = savingAccounts;
            ReportingPeriods = reportingPeriods;
            SpendingCategories = spendingCategories;

            Creator = new Creator(this);
        }

        public static SpendingSavingsTracker Load(string filePath)
        {
            var tracker = Loader.Load(filePath);
            return tracker;
        }

        public static SpendingSavingsTracker InitialiseEmpty()
        {
            var tracker = new SpendingSavingsTracker([], [], []);

            return tracker;
        }

        public void Save(string filePath)
        {
            Saver.Save(filePath, this);
        } 
    }
}
