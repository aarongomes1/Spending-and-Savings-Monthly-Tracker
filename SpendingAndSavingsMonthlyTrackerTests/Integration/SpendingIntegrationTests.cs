using CommonClasses.Structure;
using SpendingAndSavingsMonthlyTracker;
using SpendingAndSavingsMonthlyTracker.Models;

namespace SpendingAndSavingsMonthlyTrackerTests.Integration
{
    public class SpendingIntegrationTests
    {
        [Theory]
        [InlineData(10)]
        [InlineData(-5)]
        public void RefundAppliedToSpending_ReducesAmountSpent(decimal refundAmount)
        {
            var startDate = new DateOnly(2026, 5, 30);
            var endDate = new DateOnly(2026, 5, 30);

            var tracker = SpendingSavingsTracker.InitialiseEmpty();
            var reportingPeriod = tracker.Creator.GetOrCreateReportingPeriod(startDate, endDate);

            // GIVEN a partial refund on a bill
            var spendingRecords = new List<SpendingInput>()
            {
                new() { Category = "Bills", Debit = 100, Name = "BillToGetPartRefunded", Refund = null },
                new() { Category = "Bills", Debit = null, Name = "BillToGetPartRefunded", Refund = refundAmount },
            };

            Normaliser.NormaliseSpending(tracker, spendingRecords, reportingPeriod);

            // WHEN the spending for the current reporting period is called
            var spendingStats = StatsExtractor.GetSpendingThisPeriod(reportingPeriod);

            Assert.Single(spendingStats);

            var statRecord = spendingStats.Single();

            var totalWithRefund = 100 - Math.Abs(refundAmount);

            // THEN the refund should be applied to the bill
            Assert.Equal("BillToGetPartRefunded", statRecord.SpendingPlace);
            Assert.Equal(2, statRecord.NumberOfVisits);
            Assert.Equal(totalWithRefund, statRecord.TotalAmountSpent);
        }

        [Fact]
        public void NegativeRefundAmount_ReducesAmountSpent()
        {
            var startDate = new DateOnly(2026, 5, 30);
            var endDate = new DateOnly(2026, 5, 30);

            var tracker = SpendingSavingsTracker.InitialiseEmpty();
            var reportingPeriod = tracker.Creator.GetOrCreateReportingPeriod(startDate, endDate);

            // GIVEN a partial refund on a bill
            var spendingRecords = new List<SpendingInput>()
            {
                new() { Category = "Bills", Debit = 100, Name = "BillToGetPartRefunded", Refund = null },
                new() { Category = "Bills", Debit = null, Name = "BillToGetPartRefunded", Refund = -10 },
            };

            Normaliser.NormaliseSpending(tracker, spendingRecords, reportingPeriod);

            // WHEN the spending for the current reporting period is called
            var spendingStats = StatsExtractor.GetSpendingThisPeriod(reportingPeriod);

            Assert.Single(spendingStats);

            var statRecord = spendingStats.Single();

            // THEN the refund should be applied to the bill
            Assert.Equal("BillToGetPartRefunded", statRecord.SpendingPlace);
            Assert.Equal(2, statRecord.NumberOfVisits);
            Assert.Equal(90, statRecord.TotalAmountSpent);
        }

        [Fact]
        public void RefundFromPreviousReportingPeriod_WhenBiggerThanAnySpendingThisPeriod_MakesSependingNegative()
        {
            var startDate = new DateOnly(2026, 5, 30);
            var endDate = new DateOnly(2026, 5, 30);

            var tracker = SpendingSavingsTracker.InitialiseEmpty();
            var reportingPeriod = tracker.Creator.GetOrCreateReportingPeriod(startDate, endDate);

            // GIVEN a refund larger than the total spend on this reporting period
            var spendingRecords = new List<SpendingInput>()
            {
                new() { Category = "Bills", Debit = null, Name = "Food Shopping", Refund = 100 },
                new() { Category = "Bills", Debit = 4.5m, Name = "Food Shopping", Refund = null },
                new() { Category = "Bills", Debit = 10, Name = "Food Shopping", Refund = null },
                new() { Category = "Bills", Debit = 1.75m, Name = "Food Shopping", Refund = null },
                new() { Category = "Bills", Debit = 2.32m, Name = "Food Shopping", Refund = null },
            };

            Normaliser.NormaliseSpending(tracker, spendingRecords, reportingPeriod);

            // WHEN the spending for the current reporting period is called
            var spendingStats = StatsExtractor.GetSpendingThisPeriod(reportingPeriod);

            Assert.Single(spendingStats);

            var statRecord = spendingStats.Single();

            // THEN the spend for this reporting period should be negative
            Assert.Equal("Food Shopping", statRecord.SpendingPlace);
            Assert.Equal(5, statRecord.NumberOfVisits);
            Assert.Equal(-81.43m, statRecord.TotalAmountSpent);
        }

        [Fact]
        public void SpendingTransactionCannotHaveBothDebitAndRefund()
        {
            var startDate = new DateOnly(2026, 5, 30);
            var endDate = new DateOnly(2026, 5, 30);

            var tracker = SpendingSavingsTracker.InitialiseEmpty();
            var reportingPeriod = tracker.Creator.GetOrCreateReportingPeriod(startDate, endDate);

            // GIVEN a spending record that has both a debit and a refund
            var spendingRecords = new List<SpendingInput>()
            {
                new() { Category = "Bills", Debit = 10, Name = "Food Shopping", Refund = 100 },
            };

            // WHEN the record is normalised
            // THEN an InvalidDataException should be thrown
            Assert.Throws<InvalidDataException>(() => Normaliser.NormaliseSpending(tracker, spendingRecords, reportingPeriod));
        }

        [Fact]
        public void SpendingTransactionMustHaveEitherARefundOrDebit()
        {
            var startDate = new DateOnly(2026, 5, 30);
            var endDate = new DateOnly(2026, 5, 30);

            var tracker = SpendingSavingsTracker.InitialiseEmpty();
            var reportingPeriod = tracker.Creator.GetOrCreateReportingPeriod(startDate, endDate);

            // GIVEN a spending record that has neither a debit or refund
            var spendingRecords = new List<SpendingInput>()
            {
                new() { Category = "Bills", Debit = null, Name = "Food Shopping", Refund = null },
            };

            // WHEN the record is normalised
            // THEN an InvalidDataException should be thrown
            Assert.Throws<InvalidDataException>(() => Normaliser.NormaliseSpending(tracker, spendingRecords, reportingPeriod));
        }
    }
}