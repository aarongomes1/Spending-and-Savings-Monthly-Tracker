
namespace TransactionsToSpendingSavings.Models
{
    internal class Unmapped
    {
        public required string TransactionName { get; set; }
        public required decimal Amount { get; set; }
        public required string Account {  get; set; }

        public override bool Equals(object? obj)
        {
            var other = obj as Unmapped;

            if (other is null)
            {
                return false;
            }

            return TransactionName.Equals(other.TransactionName, StringComparison.OrdinalIgnoreCase)
                && Amount == other.Amount
                && Account.Equals(other.Account, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(TransactionName, Account, Amount);
        }
    }
}
