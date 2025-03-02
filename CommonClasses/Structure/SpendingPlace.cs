
namespace CommonClasses.Structure
{
    public class SpendingPlace
    {
        public required Guid SpendingPlaceKey { get; init; } = Guid.NewGuid();
        public required string SpendingPlaceName { get; init; }
        public required SpendingCategory SpendingCategory { get; init; }
    }
}
