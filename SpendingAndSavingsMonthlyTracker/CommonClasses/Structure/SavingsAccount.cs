
namespace CommonClasses.Structure
{
    public class SavingsAccount
    {
        public required Guid SavingsAccountKey { get; init; } = Guid.NewGuid();
        public required string SavingsAccountName { get; init; }
        public required decimal Balance { get; init; }
        public List<SavingsTransaction> Transactions { get; } = [];
    }
}
