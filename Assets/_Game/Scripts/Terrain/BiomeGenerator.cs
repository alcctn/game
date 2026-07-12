using System;
using CleanEnergy.Grid;
using CleanEnergy.Map;
using UnityEngine;

namespace CleanEnergy.TerrainGeneration
{
    /// <summary>
    /// Assigns coarse biomes from water, slope and seeded forest noise.
    /// </summary>
    public sealed class BiomeGenerator
    {
        public void Calculate(GridService grid, MapGenerationSettings settings)
        {
            if (grid == null) throw new ArgumentNullException(nameof(grid));
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            if (!grid.IsInitialized)
            {
                throw new InvalidOperationException("[Biome] Grid must be initialized.");
            }

            var seedOffset = SeedUtility.HashToOffset(settings.Seed);

            for (var x = 0; x < grid.Width; x++)
            {
                for (var y = 0; y < grid.Height; y++)
                {
                    var coordinate = new GridCoordinate(x, y);
                    var cell = grid.GetCell(coordinate);
                    grid.SetBiome(coordinate, ResolveBiome(cell, x, y, settings, seedOffset));
                }
            }
        }

        private static BiomeType ResolveBiome(
            GridCellData cell,
            int x,
            int y,
            MapGenerationSettings settings,
            Vector2 seedOffset)
        {
            if (cell.IsWater)
            {
                return cell.WaterFlow >= settings.LakeAccumulationThreshold
                    ? BiomeType.Lake
                    : BiomeType.River;
            }

            if (cell.Slope >= settings.RidgeSlopeDegrees)
            {
                return BiomeType.Ridge;
            }

            if (cell.Slope >= settings.HillsSlopeDegrees)
            {
                return BiomeType.Hills;
            }

            if (SeedUtility.IsForested(x, y, settings, seedOffset))
            {
                return BiomeType.Forest;
            }

            return BiomeType.Plains;
        }
    }
}
