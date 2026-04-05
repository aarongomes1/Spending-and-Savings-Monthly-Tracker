using TransactionsToSpendingSavings.Models;

namespace TransactionsToSpendingSavings.Mappers
{
    internal class SavingsMapper
    {
        public static (List<SavingsInput> MappedTransactions, List<Unmapped> Unmapped) TranslateSavings(
            List<Barclays> barclaysTransactions,
            List<Barclaycard> barclaycardTransactions,
            List<SavingsMapping> savingsMapping)
        {
            var outputRecords = new List<SavingsInput>();
            var unusedMappings = new List<Unmapped>();

            foreach (var transaction in barclaysTransactions)
            {
                var mapping = GetSavingsMapping(transaction.Memo, savingsMapping);

                if (mapping is not null)
                {
                    var mappedRecord = new SavingsInput
                    {
                        AccountName = mapping.MappedName,
                        Deposit = Math.Abs(decimal.Parse(transaction.Amount)),
                        TransactionDate = transaction.Date,
                        BalanceCountsToISALimit = mapping.BalanceCountsToISA,
                        IsISA = mapping.IsISA,
                    };

                    outputRecords.Add(mappedRecord);
                }
                else
                {
                    var unmappedRecord = new Unmapped
                    {
                        TransactionName = transaction.Memo,
                        Amount = Math.Abs(decimal.Parse(transaction.Amount)),
                        Account = "Barclays",
                    };

                    unusedMappings.Add(unmappedRecord);
                }

                barclaycardTransactions = barclaycardTransactions.Where(x => x.Credit is not null).ToList();

                foreach (var unmappedBarclaycard in barclaycardTransactions)
                {
                    var unmappedRecord = new Unmapped
                    {
                        TransactionName = unmappedBarclaycard.Name,
                        Amount = (decimal) unmappedBarclaycard.Credit!,
                        Account = "Barclaycard",
                    };

                    unusedMappings.Add(unmappedRecord);
                }
            }

            return (outputRecords, unusedMappings);
        }

        private static SavingsMapping? GetSavingsMapping(string name, List<SavingsMapping> mappings)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            var matchedMappings = mappings.Where(x =>
            x.SearchType.Equals("equals", StringComparison.OrdinalIgnoreCase)
            ? name.Trim().Equals(x.TransactionName.Trim(), StringComparison.OrdinalIgnoreCase)
            : name.Trim().Contains(x.TransactionName.Trim(), StringComparison.OrdinalIgnoreCase)).ToList();

            if (matchedMappings.Count == 1)
            {
                // If there's only 1 match then return that
                return matchedMappings.Single();
            }
            else if (matchedMappings.Count > 1)
            {
                // If there's multiple matches then pick the longest matched name
                // If there's multplie matches with the same length then pick the first
                return matchedMappings.OrderByDescending(x => x.TransactionName).First();
            }

            return null;
        }
    }
}
