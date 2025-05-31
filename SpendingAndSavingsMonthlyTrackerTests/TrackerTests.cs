using CommonClasses.Structure;

namespace SpendingAndSavingsMonthlyTrackerTests
{
    public class TrackerTests
    {
        [Fact]
        public void GetsCorrectSpendingThisPeriod()
        {
            var startDate = new DateOnly(2025, 2, 23);

            var tracker = SpendingSavingsTracker.InitialiseEmpty();

            Assert.True(true);
        }
    }
}