using CleanEnergy.Buildings;
using CleanEnergy.Grid;
using CleanEnergy.Map;
using CleanEnergy.Placement;
using CleanEnergy.Simulation;
using UnityEngine;

namespace CleanEnergy.Energy
{
    public readonly struct ProductionBreakdown
    {
        public float ResourcePotential { get; }
        public float PhaseFactor { get; }
        public float Efficiency { get; }
        public float Maintenance { get; }
        public float WakeFactor { get; }
        public float NetworkFactor { get; }
        public float Production { get; }

        public ProductionBreakdown(
            float resourcePotential,
            float phaseFactor,
            float efficiency,
            float maintenance,
            float wakeFactor,
            float production,
            float networkFactor = 1f)
        {
            ResourcePotential = resourcePotential;
            PhaseFactor = phaseFactor;
            Efficiency = efficiency;
            Maintenance = maintenance;
            WakeFactor = wakeFactor;
            NetworkFactor = networkFactor;
            Production = production;
        }
    }

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
            return BreakDown(
                definition, coordinate, grid, settings, context,
                occupancy, efficiencyBonus, maintenanceLevel, excludeInstanceId).Production;
        }

        public static ProductionBreakdown BreakDown(
            BuildingDefinition definition,
            GridCoordinate coordinate,
            GridService grid,
            MapGenerationSettings settings,
            SimulationContext context,
            GridOccupancyService occupancy = null,
            float efficiencyBonus = 0f,
            float maintenanceLevel = 1f,
            string excludeInstanceId = null,
            float networkFactor = 1f)
        {
            if (definition == null || definition.InstalledPower <= 0f || grid == null)
            {
                return new ProductionBreakdown(0f, 0f, 0f, 0f, 1f, 0f, networkFactor);
            }

            SampleResourceAndPhase(
                definition.Id, coordinate, grid, settings, context,
                out var resource, out var phase);
            var efficiency = Mathf.Max(0f, definition.Efficiency + efficiencyBonus);
            var maintenance = Mathf.Clamp01(maintenanceLevel);
            var wake = WindWakeFactor.Compute(
                definition, coordinate, occupancy, excludeInstanceId);
            var factor = Mathf.Clamp01(networkFactor);
            var production = Mathf.Max(
                0f,
                definition.InstalledPower * resource * phase * efficiency * maintenance * wake * factor);
            return new ProductionBreakdown(resource, phase, efficiency, maintenance, wake, production, factor);
        }

        public static float SamplePotential(
            string buildingId,
            GridCoordinate coordinate,
            GridService grid,
            MapGenerationSettings settings,
            SimulationContext context)
        {
            SampleResourceAndPhase(buildingId, coordinate, grid, settings, context, out var resource, out var phase);
            return resource * phase;
        }

        public static void SampleResourceAndPhase(
            string buildingId,
            GridCoordinate coordinate,
            GridService grid,
            MapGenerationSettings settings,
            SimulationContext context,
            out float resourcePotential,
            out float phaseFactor)
        {
            resourcePotential = 0f;
            phaseFactor = 1f;
            if (grid == null || !grid.TryGetCell(coordinate, out var cell))
            {
                return;
            }

            switch (buildingId)
            {
                case "water_wheel":
                case "small_hydro":
                    resourcePotential = SampleHydroPotential(coordinate, cell, grid, settings);
                    phaseFactor = 1f;
                    break;
                case "small_solar":
                    resourcePotential = Mathf.Clamp01(cell.SolarPotential);
                    phaseFactor = Mathf.Clamp01(context.DaylightFactor);
                    break;
                case "small_wind":
                    resourcePotential = Mathf.Clamp01(cell.WindPotential);
                    phaseFactor = Mathf.Max(0f, context.WindFactor);
                    break;
                default:
                    resourcePotential = 1f;
                    phaseFactor = 1f;
                    break;
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
