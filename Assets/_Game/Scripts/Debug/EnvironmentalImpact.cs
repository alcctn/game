using CleanEnergy.Buildings;
using CleanEnergy.Grid;
using CleanEnergy.Placement;
using UnityEngine;

namespace CleanEnergy.DebugTools
{
    /// <summary>
    /// Stub environmental impact score from nearby producers (visual only).
    /// </summary>
    public static class EnvironmentalImpact
    {
        public static float ScoreAt(
            GridCoordinate coordinate,
            GridOccupancyService occupancy)
        {
            if (occupancy == null)
            {
                return 0f;
            }

            var count = 0;
            for (var dx = -1; dx <= 1; dx++)
            {
                for (var dy = -1; dy <= 1; dy++)
                {
                    var c = new GridCoordinate(coordinate.X + dx, coordinate.Y + dy);
                    if (!occupancy.TryGet(c, out var building) || building?.Definition == null)
                    {
                        continue;
                    }

                    if (building.Definition.IsProducer)
                    {
                        count++;
                    }
                }
            }

            return Mathf.Clamp01(count * 0.25f);
        }

        public static Color ColorForScore(float score)
        {
            var t = Mathf.Clamp01(score);
            return Color.Lerp(new Color(0.2f, 0.45f, 0.25f, 0.55f), new Color(0.75f, 0.35f, 0.1f, 0.75f), t);
        }
    }
}
