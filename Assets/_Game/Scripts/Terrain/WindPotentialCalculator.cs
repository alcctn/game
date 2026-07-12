using System;
using CleanEnergy.Grid;
using CleanEnergy.Map;
using UnityEngine;

namespace CleanEnergy.TerrainGeneration
{
    /// <summary>
    /// Prototype wind potential: base + elevation + ridge - obstacle.
    /// </summary>
    public sealed class WindPotentialCalculator
    {
        public void Calculate(float[,] heightMap, GridService grid, MapGenerationSettings settings)
        {
            if (heightMap == null) throw new ArgumentNullException(nameof(heightMap));
            if (grid == null) throw new ArgumentNullException(nameof(grid));
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            if (!grid.IsInitialized)
            {
                throw new InvalidOperationException("[Wind] Grid must be initialized.");
            }

            var windDir = settings.PrevailingWindDirection.sqrMagnitude > 1e-6f
                ? settings.PrevailingWindDirection.normalized
                : Vector2.right;
            var seedOffset = SeedUtility.HashToOffset(settings.Seed);

            for (var x = 0; x < grid.Width; x++)
            {
                for (var y = 0; y < grid.Height; y++)
                {
                    var coordinate = new GridCoordinate(x, y);
                    var cell = grid.GetCell(coordinate);
                    var elevationNorm = settings.MaxHeight > 0f
                        ? Mathf.Clamp01(cell.Elevation / settings.MaxHeight)
                        : 0f;

                    var ridgeBonus = IsRidge(heightMap, x, y, grid.Width, grid.Height, windDir)
                        ? settings.RidgeWindBonus
                        : 0f;
                    var forested = SeedUtility.IsForested(x, y, settings, seedOffset);
                    var obstacle = forested || cell.IsWater
                        ? settings.ObstacleWindPenalty
                        : 0f;

                    var wind = settings.BaseWind
                               + elevationNorm * settings.ElevationWindBonus
                               + ridgeBonus
                               - obstacle;
                    grid.SetWindPotential(coordinate, wind);
                }
            }
        }

        public static bool IsRidge(float[,] heightMap, int x, int y, int width, int height, Vector2 windDir)
        {
            var center = heightMap[x, y];
            var ox = Mathf.RoundToInt(Mathf.Sign(windDir.x));
            var oy = Mathf.RoundToInt(Mathf.Sign(windDir.y));
            if (ox == 0 && oy == 0)
            {
                ox = 1;
            }

            var upX = Mathf.Clamp(x - ox, 0, width - 1);
            var upY = Mathf.Clamp(y - oy, 0, height - 1);
            var downX = Mathf.Clamp(x + ox, 0, width - 1);
            var downY = Mathf.Clamp(y + oy, 0, height - 1);

            return center >= heightMap[upX, upY] && center >= heightMap[downX, downY]
                   && (center > heightMap[upX, upY] || center > heightMap[downX, downY]);
        }
    }
}
