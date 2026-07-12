using CleanEnergy.Buildings;
using CleanEnergy.Grid;
using UnityEngine;

namespace CleanEnergy.Placement
{
    /// <summary>
    /// Distance-scaled cost for power_line and distribution_hub (GDD §8).
    /// </summary>
    public static class PowerLinePlacementCost
    {
        public const string PowerLineId = "power_line";
        public const string DistributionHubId = "distribution_hub";
        public const float DistanceCostFactor = 0.15f;

        public static bool AppliesTo(BuildingDefinition definition)
        {
            return definition != null
                   && (definition.Id == PowerLineId || definition.Id == DistributionHubId);
        }

        public static bool IsNetworkNode(BuildingDefinition definition)
        {
            if (definition == null)
            {
                return false;
            }

            return definition.IsNetworkHub
                   || definition.IsProducer
                   || definition.IsConsumer
                   || definition.IsStorage;
        }

        public static int NearestNetworkDistance(
            GridCoordinate coordinate,
            GridOccupancyService occupancy)
        {
            if (occupancy == null)
            {
                return 0;
            }

            var best = int.MaxValue;
            foreach (var pair in occupancy.Occupied)
            {
                var other = pair.Value;
                if (other?.Definition == null || !IsNetworkNode(other.Definition))
                {
                    continue;
                }

                var d = Manhattan(coordinate, pair.Key);
                if (d < best)
                {
                    best = d;
                }
            }

            return best == int.MaxValue ? 0 : best;
        }

        public static float ComputeEffectiveCost(
            BuildingDefinition definition,
            GridCoordinate coordinate,
            GridOccupancyService occupancy)
        {
            if (definition == null)
            {
                return 0f;
            }

            if (!AppliesTo(definition))
            {
                return Mathf.Max(0f, definition.Cost);
            }

            var d = NearestNetworkDistance(coordinate, occupancy);
            return Mathf.Max(0f, definition.Cost * (1f + DistanceCostFactor * d));
        }

        private static int Manhattan(GridCoordinate a, GridCoordinate b)
        {
            var dx = a.X - b.X;
            var dy = a.Y - b.Y;
            if (dx < 0) dx = -dx;
            if (dy < 0) dy = -dy;
            return dx + dy;
        }
    }
}
