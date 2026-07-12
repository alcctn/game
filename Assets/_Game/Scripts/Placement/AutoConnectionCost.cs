using CleanEnergy.Buildings;
using CleanEnergy.Grid;
using CleanEnergy.Settlements;
using UnityEngine;

namespace CleanEnergy.Placement
{
    /// <summary>Auto grid link cost: distance to active settlement × cost per cell.</summary>
    public static class AutoConnectionCost
    {
        public static bool AppliesTo(BuildingDefinition definition)
        {
            return definition != null
                   && (definition.IsProducer || definition.IsStorage)
                   && definition.Id != PowerLinePlacementCost.PowerLineId
                   && definition.Id != PowerLinePlacementCost.DistributionHubId
                   && definition.Id != "village";
        }

        public static int DistanceToSettlement(
            GridCoordinate coordinate,
            IActiveSettlementQuery settlement)
        {
            if (settlement == null || !settlement.HasActiveSettlement)
            {
                return 0;
            }

            var dx = settlement.Coordinate.X - coordinate.X;
            var dy = settlement.Coordinate.Y - coordinate.Y;
            if (dx < 0) dx = -dx;
            if (dy < 0) dy = -dy;
            return dx + dy;
        }

        public static float Compute(
            BuildingDefinition definition,
            GridCoordinate coordinate,
            IActiveSettlementQuery settlement,
            float costPerCell,
            bool autoConnectEnabled)
        {
            if (!autoConnectEnabled || !AppliesTo(definition))
            {
                return 0f;
            }

            return DistanceToSettlement(coordinate, settlement) * Mathf.Max(0f, costPerCell);
        }
    }
}
