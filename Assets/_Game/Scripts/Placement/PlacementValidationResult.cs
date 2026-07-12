using System.Collections.Generic;

namespace CleanEnergy.Placement
{
    public sealed class PlacementValidationResult
    {
        public bool IsValid { get; }
        public IReadOnlyList<string> FailureReasons { get; }

        public PlacementValidationResult(bool isValid, IReadOnlyList<string> failureReasons)
        {
            IsValid = isValid;
            FailureReasons = failureReasons ?? System.Array.Empty<string>();
        }

        public static PlacementValidationResult Success() =>
            new PlacementValidationResult(true, System.Array.Empty<string>());

        public static PlacementValidationResult Failure(IReadOnlyList<string> reasons) =>
            new PlacementValidationResult(false, reasons);
    }
}
