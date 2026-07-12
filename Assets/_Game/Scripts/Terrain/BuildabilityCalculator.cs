using System;
using CleanEnergy.Grid;
using CleanEnergy.Map;

namespace CleanEnergy.TerrainGeneration
{
    /// <summary>
    /// Final buildability pass: water and excessive slope are not buildable.
    /// </summary>
    public sealed class BuildabilityCalculator
    {
        public void Calculate(GridService grid, MapGenerationSettings settings)
        {
            if (grid == null) throw new ArgumentNullException(nameof(grid));
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            if (!grid.IsInitialized)
            {
                throw new InvalidOperationException("[Buildability] Grid must be initialized.");
            }

            for (var x = 0; x < grid.Width; x++)
            {
                for (var y = 0; y < grid.Height; y++)
                {
                    var coordinate = new GridCoordinate(x, y);
                    var cell = grid.GetCell(coordinate);
                    var buildable = !cell.IsWater && cell.Slope <= settings.MaxBuildableSlopeDegrees;
                    grid.SetBuildable(coordinate, buildable);
                }
            }
        }
    }
}
