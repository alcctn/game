using System;
using CleanEnergy.Grid;
using UnityEngine;

namespace CleanEnergy.TerrainGeneration
{
    /// <summary>
    /// Computes per-cell slope (degrees) and aspect from neighboring elevations.
    /// </summary>
    public sealed class SlopeCalculator
    {
        /// <summary>
        /// Writes slope and aspect into the grid. Buildability is finalized later by BuildabilityCalculator.
        /// </summary>
        public void Calculate(
            float[,] heightMap,
            GridService grid,
            float maxHeight,
            float cellSize,
            float maxBuildableSlopeDegrees)
        {
            if (heightMap == null) throw new ArgumentNullException(nameof(heightMap));
            if (grid == null) throw new ArgumentNullException(nameof(grid));
            if (!grid.IsInitialized)
            {
                throw new InvalidOperationException("[Slope] Grid must be initialized before calculating slope.");
            }

            var width = grid.Width;
            var height = grid.Height;

            if (heightMap.GetLength(0) != width || heightMap.GetLength(1) != height)
            {
                throw new ArgumentException("[Slope] Height map dimensions must match the grid.");
            }

            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    var slope = CalculateCellSlopeDegrees(heightMap, x, y, width, height, maxHeight, cellSize);
                    var aspect = CalculateCellAspect(heightMap, x, y, width, height);
                    var coordinate = new GridCoordinate(x, y);
                    grid.SetSlope(coordinate, slope, slope <= maxBuildableSlopeDegrees);
                    grid.SetAspect(coordinate, aspect);
                }
            }
        }

        public static float CalculateCellSlopeDegrees(
            float[,] heightMap,
            int x,
            int y,
            int width,
            int height,
            float maxHeight,
            float cellSize)
        {
            SampleGradients(heightMap, x, y, width, height, maxHeight, cellSize, out var dzDx, out var dzDy);
            var gradient = Math.Sqrt(dzDx * dzDx + dzDy * dzDy);
            var degrees = (float)(Math.Atan(gradient) * (180.0 / Math.PI));
            if (float.IsNaN(degrees) || float.IsInfinity(degrees))
            {
                return 0f;
            }

            return degrees;
        }

        /// <summary>
        /// Returns the downslope direction in XZ (normalized). Flat cells return zero.
        /// </summary>
        public static Vector2 CalculateCellAspect(float[,] heightMap, int x, int y, int width, int height)
        {
            SampleGradients(heightMap, x, y, width, height, 1f, 1f, out var dzDx, out var dzDy);
            var down = new Vector2(-(float)dzDx, -(float)dzDy);
            if (down.sqrMagnitude < 1e-8f)
            {
                return Vector2.zero;
            }

            return down.normalized;
        }

        private static void SampleGradients(
            float[,] heightMap,
            int x,
            int y,
            int width,
            int height,
            float maxHeight,
            float cellSize,
            out double dzDx,
            out double dzDy)
        {
            var xLeft = Math.Max(x - 1, 0);
            var xRight = Math.Min(x + 1, width - 1);
            var yDown = Math.Max(y - 1, 0);
            var yUp = Math.Min(y + 1, height - 1);

            dzDx = (heightMap[xRight, y] - heightMap[xLeft, y]) * maxHeight /
                   Math.Max(cellSize * (xRight - xLeft), 1e-6f);
            dzDy = (heightMap[x, yUp] - heightMap[x, yDown]) * maxHeight /
                   Math.Max(cellSize * (yUp - yDown), 1e-6f);
        }
    }
}
