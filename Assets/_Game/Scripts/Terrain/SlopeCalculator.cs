using System;
using CleanEnergy.Grid;

namespace CleanEnergy.TerrainGeneration
{
    /// <summary>
    /// Computes per-cell slope in degrees from neighboring elevations.
    /// </summary>
    public sealed class SlopeCalculator
    {
        /// <summary>
        /// Writes slope (degrees) and buildability into the grid.
        /// </summary>
        /// <param name="heightMap">Normalized elevations matching grid size.</param>
        /// <param name="maxHeight">World max height used to convert normalized elevation to meters.</param>
        /// <param name="cellSize">World size of one cell edge.</param>
        /// <param name="maxBuildableSlopeDegrees">Cells steeper than this are not buildable.</param>
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
                    var buildable = slope <= maxBuildableSlopeDegrees;
                    grid.SetSlope(new GridCoordinate(x, y), slope, buildable);
                }
            }
        }

        /// <summary>
        /// Central-difference slope in degrees. Edge cells use available neighbors.
        /// </summary>
        public static float CalculateCellSlopeDegrees(
            float[,] heightMap,
            int x,
            int y,
            int width,
            int height,
            float maxHeight,
            float cellSize)
        {
            var xLeft = Math.Max(x - 1, 0);
            var xRight = Math.Min(x + 1, width - 1);
            var yDown = Math.Max(y - 1, 0);
            var yUp = Math.Min(y + 1, height - 1);

            var dzDx = (heightMap[xRight, y] - heightMap[xLeft, y]) * maxHeight /
                       Math.Max(cellSize * (xRight - xLeft), 1e-6f);
            var dzDy = (heightMap[x, yUp] - heightMap[x, yDown]) * maxHeight /
                       Math.Max(cellSize * (yUp - yDown), 1e-6f);

            var gradient = Math.Sqrt(dzDx * dzDx + dzDy * dzDy);
            var degrees = (float)(Math.Atan(gradient) * (180.0 / Math.PI));
            if (float.IsNaN(degrees) || float.IsInfinity(degrees))
            {
                return 0f;
            }

            return degrees;
        }
    }
}
