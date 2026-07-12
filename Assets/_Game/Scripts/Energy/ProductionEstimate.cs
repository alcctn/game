using CleanEnergy.Buildings;
using CleanEnergy.Grid;
using CleanEnergy.Map;
using CleanEnergy.Placement;
using CleanEnergy.Simulation;
using UnityEngine;

namespace CleanEnergy.Energy
{
    /// <summary>
    /// Shared production estimate used by adapters and placement yield UI.
    /// </summary>
    public static class ProductionEstimate
    {
        public static float Estimate(
            BuildingDefinition definition,
            GridCoordinate coordinate,
            GridService grid,
            MapGenerationSettings settings,
            SimulationContext context,
            GridOccupancyService occupancy = null,
            float efficiencyBonus = 0f,
            float maintenanceLevel = 1f,
            string excludeInstanceId = null)
        {
            if (definition == null || definition.InstalledPower <= 0f || grid == null)
            {
                return 0f;
            }

            var potential = SamplePotential(definition.Id, coordinate, grid, settings, context);
            var efficiency = Mathf.Max(0f, definition.Efficiency + efficiencyBonus);
            var maintenance = Mathf.Clamp01(maintenanceLevel);
            var wake = WindWakeFactor.Compute(
                definition, coordinate, occupancy, excludeInstanceId);
            return Mathf.Max(0f, definition.InstalledPower * potential * efficiency * maintenance * wake);
        }

        public static float SamplePotential(
            string buildingId,
            GridCoordinate coordinate,
            GridService grid,
            MapGenerationSettings settings,
            SimulationContext context)
        {
            if (grid == null || !grid.TryGetCell(coordinate, out var cell))
            {
                return 0f;
            }

            switch (buildingId)
            {
                case "water_wheel":
                case "small_hydro":
                    return SampleHydroPotential(coordinate, cell, grid, settings);
                case "small_solar":
                    return Mathf.Clamp01(cell.SolarPotential) * Mathf.Clamp01(context.DaylightFactor);
                case "small_wind":
                    return Mathf.Clamp01(cell.WindPotential) * Mathf.Max(0f, context.WindFactor);
                default:
                    return 1f;
            }
        }

        private static float SampleHydroPotential(
            GridCoordinate coordinate,
            GridCellData cell,
            GridService grid,
            MapGenerationSettings settings)
        {
            var bestFlow = cell.WaterFlow;
            for (var dx = -1; dx <= 1; dx++)
            {
                for (var dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0)
                    {
                        continue;
                    }

                    var n = new GridCoordinate(coordinate.X + dx, coordinate.Y + dy);
                    if (grid.TryGetCell(n, out var neighbor) && neighbor.WaterFlow > bestFlow)
                    {
                        bestFlow = neighbor.WaterFlow;
                    }
                }
            }

            var threshold = settings != null ? Mathf.Max(1f, settings.StreamAccumulationThreshold) : 12f;
            return Mathf.Clamp01(bestFlow / threshold);
        }
    }
}
