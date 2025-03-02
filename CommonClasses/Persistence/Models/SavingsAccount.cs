namespace CommonClasses.Persistence.Models
{
    internal class SavingsAccount
    {
        public required string SavingsAccountKey { get; init; }
        public required string SavingsAccountName { get; init; }
        public required decimal Balance { get; init; }
    }
}
