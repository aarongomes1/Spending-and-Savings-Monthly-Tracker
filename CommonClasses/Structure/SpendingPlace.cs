
namespace CommonClasses.Structure
{
    public class SpendingPlace
    {
        public Guid SpendingPlaceKey { get; init; } = Guid.NewGuid();
        public required string SpendingPlaceName { get; init; }
        public required SpendingCategory SpendingCategory { get; init; }

        public override bool Equals(object? obj)
        {
            if (obj is null || obj.GetType() != typeof(SpendingPlace))
            {
                return false;
            }

            var other = (obj as SpendingPlace)!;

            return other.SpendingPlaceName.Equals(SpendingPlaceName, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode()
        {
            return SpendingPlaceKey.GetHashCode();
        }
    }
}
