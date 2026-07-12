using CleanEnergy.Buildings;
using CleanEnergy.Grid;
using CleanEnergy.Placement;
using UnityEngine;

namespace CleanEnergy.Energy
{
    /// <summary>
    /// Same-type wind turbine wake penalty (Chebyshev neighbors within spacing).
    /// </summary>
    public static class WindWakeFactor
    {
        public const float PenaltyPerNeighbor = 0.12f;
        public const float MinFactor = 0.4f;

        public static bool AppliesTo(BuildingDefinition definition)
        {
            return definition != null && definition.Id == "small_wind";
        }

        public static float Compute(
            BuildingDefinition definition,
            GridCoordinate coordinate,
            GridOccupancyService occupancy,
            string excludeInstanceId = null)
        {
            if (!AppliesTo(definition) || occupancy == null)
            {
                return 1f;
            }

            var spacing = definition.MinSameTypeSpacing;
            if (spacing <= 0)
            {
                return 1f;
            }

            var neighbors = CountSameTypeNeighbors(
                definition.Id,
                coordinate,
                occupancy,
                spacing,
                excludeInstanceId);
            return ClampFactor(neighbors);
        }

        public static float ClampFactor(int neighborCount)
        {
            return Mathf.Clamp(1f - neighborCount * PenaltyPerNeighbor, MinFactor, 1f);
        }

        public static int CountSameTypeNeighbors(
            string buildingId,
            GridCoordinate coordinate,
            GridOccupancyService occupancy,
            int maxChebyshevDistance,
            string excludeInstanceId = null)
        {
            if (occupancy == null || string.IsNullOrEmpty(buildingId) || maxChebyshevDistance <= 0)
            {
                return 0;
            }

            var count = 0;
            var seen = new System.Collections.Generic.HashSet<string>();
            foreach (var pair in occupancy.Occupied)
            {
                var other = pair.Value;
                if (other?.Definition == null || other.Definition.Id != buildingId)
                {
                    continue;
                }

                if (!seen.Add(other.InstanceId))
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(excludeInstanceId) && other.InstanceId == excludeInstanceId)
                {
                    continue;
                }

                if (other.Coordinate.Equals(coordinate))
                {
                    continue;
                }

                if (Chebyshev(coordinate, other.Coordinate) <= maxChebyshevDistance)
                {
                    count++;
                }
            }

            return count;
        }

        public static int Chebyshev(GridCoordinate a, GridCoordinate b)
        {
            var dx = a.X - b.X;
            var dy = a.Y - b.Y;
            if (dx < 0) dx = -dx;
            if (dy < 0) dy = -dy;
            return dx > dy ? dx : dy;
        }
    }
}
