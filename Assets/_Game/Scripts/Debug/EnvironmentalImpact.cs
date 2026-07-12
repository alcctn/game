using System.Collections.Generic;
using CleanEnergy.Buildings;
using CleanEnergy.Grid;
using CleanEnergy.Placement;
using UnityEngine;

namespace CleanEnergy.DebugTools
{
    /// <summary>
    /// Cell density environmental score (0–1) from nearby producers; drives F10 overlay and upkeep.
    /// </summary>
    public static class EnvironmentalImpact
    {
        public const float DensityPerProducer = 0.25f;
        public const float HighDensityThreshold = 0.6f;
        public const float HighDensityUpkeepMultiplier = 1.15f;

        public static float ScoreAt(
            GridCoordinate coordinate,
            GridOccupancyService occupancy)
        {
            return ScoreAt(coordinate, occupancy?.Occupied);
        }

        public static float ScoreAt(
            GridCoordinate coordinate,
            IReadOnlyDictionary<GridCoordinate, BuildingInstance> occupied)
        {
            if (occupied == null)
            {
                return 0f;
            }

            var count = 0;
            for (var dx = -1; dx <= 1; dx++)
            {
                for (var dy = -1; dy <= 1; dy++)
                {
                    var c = new GridCoordinate(coordinate.X + dx, coordinate.Y + dy);
                    if (!occupied.TryGetValue(c, out var building) || building?.Definition == null)
                    {
                        continue;
                    }

                    if (building.Definition.IsProducer)
                    {
                        count++;
                    }
                }
            }

            return Mathf.Clamp01(count * DensityPerProducer);
        }

        /// <summary>
        /// Producer upkeep multiplier when local density exceeds <see cref="HighDensityThreshold"/>.
        /// </summary>
        public static float UpkeepMultiplierForDensity(float densityScore)
        {
            return densityScore > HighDensityThreshold
                ? HighDensityUpkeepMultiplier
                : 1f;
        }

        public static float UpkeepMultiplierAt(
            GridCoordinate coordinate,
            IReadOnlyDictionary<GridCoordinate, BuildingInstance> occupied)
        {
            return UpkeepMultiplierForDensity(ScoreAt(coordinate, occupied));
        }

        /// <summary>
        /// Mean density across unique producer cells (0 when none).
        /// </summary>
        public static float MeanDensity(IReadOnlyDictionary<GridCoordinate, BuildingInstance> occupied)
        {
            if (occupied == null || occupied.Count == 0)
            {
                return 0f;
            }

            var seen = new HashSet<string>();
            var sum = 0f;
            var count = 0;
            foreach (var pair in occupied)
            {
                var building = pair.Value;
                if (building?.Definition == null
                    || !building.Definition.IsProducer
                    || string.IsNullOrEmpty(building.InstanceId)
                    || !seen.Add(building.InstanceId))
                {
                    continue;
                }

                sum += ScoreAt(building.Coordinate, occupied);
                count++;
            }

            return count == 0 ? 0f : sum / count;
        }

        /// <summary>
        /// Number of unique producer sites whose local density exceeds <see cref="HighDensityThreshold"/>.
        /// </summary>
        public static int CountHighDensitySites(
            IReadOnlyDictionary<GridCoordinate, BuildingInstance> occupied)
        {
            if (occupied == null || occupied.Count == 0)
            {
                return 0;
            }

            var seen = new HashSet<string>();
            var high = 0;
            foreach (var pair in occupied)
            {
                var building = pair.Value;
                if (building?.Definition == null
                    || !building.Definition.IsProducer
                    || string.IsNullOrEmpty(building.InstanceId)
                    || !seen.Add(building.InstanceId))
                {
                    continue;
                }

                if (ScoreAt(building.Coordinate, occupied) > HighDensityThreshold)
                {
                    high++;
                }
            }

            return high;
        }

        /// <summary>
        /// Compact HUD line: mean density, or high-site count when any are elevated.
        /// </summary>
        public static string FormatHudMeter(IReadOnlyDictionary<GridCoordinate, BuildingInstance> occupied)
        {
            var high = CountHighDensitySites(occupied);
            if (high > 0)
            {
                return $"High density sites: {high}";
            }

            var mean = MeanDensity(occupied);
            if (mean <= 0.0001f)
            {
                return string.Empty;
            }

            return $"Env density {mean.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)}";
        }

        public static Color ColorForScore(float score)
        {
            var t = Mathf.Clamp01(score);
            return Color.Lerp(new Color(0.2f, 0.45f, 0.25f, 0.55f), new Color(0.75f, 0.35f, 0.1f, 0.75f), t);
        }
    }
}
