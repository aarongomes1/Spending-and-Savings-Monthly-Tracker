using TransactionsToSpendingSavings.Models;

namespace TransactionsToSpendingSavings.Mappers
{
    internal class SpendingMapper
    {
        public static (List<SpendingInput> MappedTransactions, List<Unmapped> Unmapped) TranslateSpending(
            List<Barclaycard> barclaycardTransactions,
            List<Barclays> barclaysTransactions,
            List<SpendingMapping> spendingMapping)
        {
            (var barclaysSpending, var barclaysUnmappedSpending) = TranslateBarclaysSpending(barclaysTransactions, spendingMapping);
            (var barclaycardSpending, var barclaycardUnmappedSpending) = TranslateBarclaycardSpending(barclaycardTransactions, spendingMapping);

            var totalSpendingRecords = barclaysSpending.Concat(barclaycardSpending).ToList();
            var totalUnmappedSpendingRecords = barclaysUnmappedSpending.Concat(barclaycardUnmappedSpending).ToList();

            return (totalSpendingRecords, totalUnmappedSpendingRecords);
        }

        private static (List<SpendingInput> MappedTransactions, List<Unmapped> Unmapped) TranslateBarclaysSpending(
        List<Barclays> barclaysTransactions,
        List<SpendingMapping> spendingMapping)
        {
            var outputRecords = new List<SpendingInput>();
            var unusedMappings = new List<Unmapped>();

            foreach (var transaction in barclaysTransactions)
            {
                var mapping = GetSpendingMapping(transaction.Memo, spendingMapping);

                if (mapping is not null)
                {
                    var amount = decimal.Parse(transaction.Amount);

                    var mappedRecord = new SpendingInput
                    {
                        Category = mapping.Category,
                        Debit = amount < 0 ? Math.Abs(amount) : null,
                        Name = mapping.MappedName,
                        Refund = amount > 0 ? Math.Abs(amount) : null,
                    };

                    outputRecords.Add(mappedRecord);
                }
                else
                {
                    var amount = decimal.Parse(transaction.Amount);

                    var unmappedRecord = new Unmapped
                    {
                        TransactionName = transaction.Memo,
                        Amount = Math.Abs(amount),
                        Account = "Barclays"
                    };

                    unusedMappings.Add(unmappedRecord);
                }
            }

            return (outputRecords, unusedMappings);
        }

        private static (List<SpendingInput> MappedTransactions, List<Unmapped> Unmapped) TranslateBarclaycardSpending(
            List<Barclaycard> barclaycardTransactions,
            List<SpendingMapping> spendingMapping)
        {
            var outputRecords = new List<SpendingInput>();
            var unusedMappings = new List<Unmapped>();

            foreach (var transaction in barclaycardTransactions)
            {
                var mapping = GetSpendingMapping(transaction.Name, spendingMapping);

                if (mapping is not null)
                {
                    var mappedRecord = new SpendingInput
                    {
                        Category = mapping.Category,
                        Debit = transaction.Credit is not null ? Math.Abs((decimal)transaction.Credit) : null,
                        Name = mapping.MappedName,
                        Refund = transaction.Debit is not null ? Math.Abs((decimal)transaction.Debit) : null,
                    };

                    outputRecords.Add(mappedRecord);
                }
                else
                {
                    var unmappedRecord = new Unmapped
                    {
                        TransactionName = transaction.Name,
                        Amount = Math.Abs(transaction.Debit ?? transaction.Credit ?? 0),
                        Account = "Barclaycard"
                    };

                    unusedMappings.Add(unmappedRecord);
                }
            }

            return (outputRecords, unusedMappings);
        }

        private static SpendingMapping? GetSpendingMapping(string name, List<SpendingMapping> mappings)
        {
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
