
namespace CommonClasses.Structure
{
    public class SavingsAccount
    {
        public Guid SavingsAccountKey { get; init; } = Guid.NewGuid();

        public required string SavingsAccountName { get; init; }

        public required decimal Balance { get; set; }

        public List<SavingsTransaction> Transactions { get; } = [];

        public required bool IsISA { get; init; }

        public override bool Equals(object? obj)
        {
            if (obj is null || obj.GetType() != typeof(SavingsAccount))
            {
                return false;
            }

            var other = (obj as SavingsAccount)!;

            return other.SavingsAccountName.Equals(SavingsAccountName, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode()
        {
            return SavingsAccountKey.GetHashCode();
        }
    }
}
