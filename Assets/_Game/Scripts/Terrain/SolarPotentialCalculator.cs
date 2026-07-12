using System;
using CleanEnergy.Grid;
using CleanEnergy.Map;
using UnityEngine;

namespace CleanEnergy.TerrainGeneration
{
    /// <summary>
    /// Prototype solar potential: climate × aspect × slope × tree cover × cloud.
    /// </summary>
    public sealed class SolarPotentialCalculator
    {
        // Temperate northern-hemisphere preferred facing: +Z is treated as south for the prototype.
        private static readonly Vector2 PreferredSunFacing = new Vector2(0f, 1f);

        public void Calculate(GridService grid, MapGenerationSettings settings)
        {
            if (grid == null) throw new ArgumentNullException(nameof(grid));
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            if (!grid.IsInitialized)
            {
                throw new InvalidOperationException("[Solar] Grid must be initialized.");
            }

            var seedOffset = SeedUtility.HashToOffset(settings.Seed);

            for (var x = 0; x < grid.Width; x++)
            {
                for (var y = 0; y < grid.Height; y++)
                {
                    var coordinate = new GridCoordinate(x, y);
                    var cell = grid.GetCell(coordinate);
                    var aspectFactor = AspectFactor(cell.Aspect);
                    var slopeFactor = SlopeFactor(cell.Slope);
                    var forested = SeedUtility.IsForested(x, y, settings, seedOffset);
                    var treeFactor = forested
                        ? settings.TreeCoverFactorForest
                        : settings.TreeCoverFactorPlains;

                    if (cell.IsWater)
                    {
                        treeFactor *= 0.25f;
                    }

                    var solar = settings.BaseClimateSolar
                                * aspectFactor
                                * slopeFactor
                                * treeFactor
                                * settings.CloudFactor;
                    grid.SetSolarPotential(coordinate, solar);
                }
            }
        }

        public static float AspectFactor(Vector2 aspect)
        {
            if (aspect.sqrMagnitude < 1e-6f)
            {
                return 0.85f;
            }

            var alignment = Vector2.Dot(aspect.normalized, PreferredSunFacing);
            return Mathf.Clamp01(0.55f + 0.45f * alignment);
        }

        public static float SlopeFactor(float slopeDegrees)
        {
            if (slopeDegrees <= 5f)
            {
                return 1f;
            }

            if (slopeDegrees >= 45f)
            {
                return 0.25f;
            }

            return Mathf.Lerp(1f, 0.25f, (slopeDegrees - 5f) / 40f);
        }
    }
}
