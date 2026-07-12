using System;
using System.Collections.Generic;
using CleanEnergy.Grid;
using CleanEnergy.Map;
using UnityEngine;

namespace CleanEnergy.TerrainGeneration
{
    /// <summary>
    /// Simplified D8 flow routing with upstream accumulation and stream/lake marking.
    /// </summary>
    public sealed class WaterFlowCalculator
    {
        private static readonly Vector2Int[] D8 =
        {
            new Vector2Int(-1, -1), new Vector2Int(0, -1), new Vector2Int(1, -1),
            new Vector2Int(-1, 0),                         new Vector2Int(1, 0),
            new Vector2Int(-1, 1),  new Vector2Int(0, 1),  new Vector2Int(1, 1)
        };

        public void Calculate(float[,] heightMap, GridService grid, MapGenerationSettings settings)
        {
            if (heightMap == null) throw new ArgumentNullException(nameof(heightMap));
            if (grid == null) throw new ArgumentNullException(nameof(grid));
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            if (!grid.IsInitialized)
            {
                throw new InvalidOperationException("[Water] Grid must be initialized before water flow.");
            }

            var width = grid.Width;
            var height = grid.Height;
            if (heightMap.GetLength(0) != width || heightMap.GetLength(1) != height)
            {
                throw new ArgumentException("[Water] Height map dimensions must match the grid.");
            }

            var flowDir = new Vector2Int[width, height];
            var accumulation = new float[width, height];
            var order = new List<(int x, int y, float elevation)>(width * height);

            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    accumulation[x, y] = 1f;
                    flowDir[x, y] = FindSteepestDescent(heightMap, x, y, width, height);
                    order.Add((x, y, heightMap[x, y]));
                }
            }

            order.Sort((a, b) => b.elevation.CompareTo(a.elevation));

            for (var i = 0; i < order.Count; i++)
            {
                var (x, y, _) = order[i];
                var dir = flowDir[x, y];
                if (dir == Vector2Int.zero)
                {
                    continue;
                }

                var nx = x + dir.x;
                var ny = y + dir.y;
                if (nx < 0 || ny < 0 || nx >= width || ny >= height)
                {
                    continue;
                }

                accumulation[nx, ny] += accumulation[x, y];
            }

            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    var flow = accumulation[x, y];
                    var isPit = flowDir[x, y] == Vector2Int.zero;
                    var isStream = flow >= settings.StreamAccumulationThreshold;
                    var isLake = isPit && flow >= settings.LakeAccumulationThreshold;
                    var isWater = isStream || isLake;
                    grid.SetWaterData(new GridCoordinate(x, y), flow, flowDir[x, y], isWater);
                }
            }
        }

        /// <summary>
        /// Returns the D8 offset toward the lowest neighbor. Zero if the cell is a local pit.
        /// </summary>
        public static Vector2Int FindSteepestDescent(float[,] heightMap, int x, int y, int width, int height)
        {
            var center = heightMap[x, y];
            var bestDrop = 0f;
            var best = Vector2Int.zero;

            for (var i = 0; i < D8.Length; i++)
            {
                var offset = D8[i];
                var nx = x + offset.x;
                var ny = y + offset.y;
                if (nx < 0 || ny < 0 || nx >= width || ny >= height)
                {
                    continue;
                }

                var drop = center - heightMap[nx, ny];
                if (drop <= 0f)
                {
                    continue;
                }

                // Use drop per unit distance so cardinal neighbors beat diagonals on equal height steps.
                var distance = (offset.x != 0 && offset.y != 0) ? 1.41421356f : 1f;
                var steepness = drop / distance;
                if (steepness <= bestDrop)
                {
                    continue;
                }

                bestDrop = steepness;
                best = offset;
            }

            return best;
        }
    }
}
