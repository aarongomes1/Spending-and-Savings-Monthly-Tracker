using TransactionsToSpendingSavings.Models;

namespace TransactionsToSpendingSavings.Mappers
{
    internal class Excluder
    {
        public static bool IsExcluded(string name, List<Exclusion> exclusionRecords)
        {
            // Exclude any records that don't have a name, they can't be mapped
            if (string.IsNullOrWhiteSpace(name))
            {
                return true;
            }

            var matchedMappings = exclusionRecords.Where(x =>
            x.SearchType.Equals("equals", StringComparison.OrdinalIgnoreCase)
            ? name.Trim().Equals(x.TransactionName.Trim(), StringComparison.OrdinalIgnoreCase)
            : name.Trim().Contains(x.TransactionName.Trim(), StringComparison.OrdinalIgnoreCase)).ToList();

            return matchedMappings.Count != 0;
        }
    }
}
