using System.Collections.Generic;
using CleanEnergy.Buildings;
using CleanEnergy.Grid;
using UnityEngine;

namespace CleanEnergy.Placement
{
    public sealed class MaxSlopeRule : IPlacementRule
    {
        public string RuleId => "max_slope";

        public bool Evaluate(PlacementContext context, List<string> failureReasons)
        {
            var cells = BuildingFootprint.GetCells(
                context.Definition, context.Coordinate, context.Rotation);
            var ok = true;
            for (var i = 0; i < cells.Count; i++)
            {
                if (!context.Grid.TryGetCell(cells[i], out var cell))
                {
                    failureReasons.Add("Cell is out of bounds.");
                    return false;
                }

                if (cell.Slope <= context.Definition.MaxSlopeDegrees)
                {
                    continue;
                }

                failureReasons.Add(
                    $"Slope exceeds limit ({cell.Slope:F1}° > {context.Definition.MaxSlopeDegrees:F1}°).");
                ok = false;
                break;
            }

            return ok;
        }
    }

    public sealed class MinWaterFlowRule : IPlacementRule
    {
        public string RuleId => "min_water_flow";

        public bool Evaluate(PlacementContext context, List<string> failureReasons)
        {
            if (context.Definition.MinWaterFlow <= 0f)
            {
                return true;
            }

            if (!context.Grid.TryGetCell(context.Coordinate, out var cell))
            {
                failureReasons.Add("Cell is out of bounds.");
                return false;
            }

            var bestFlow = cell.WaterFlow;
            foreach (var neighbor in GetNeighbors(context.Coordinate, context.Grid))
            {
                if (context.Grid.TryGetCell(neighbor, out var n) && n.WaterFlow > bestFlow)
                {
                    bestFlow = n.WaterFlow;
                }
            }

            if (bestFlow >= context.Definition.MinWaterFlow)
            {
                return true;
            }

            failureReasons.Add(
                $"Water flow too low ({bestFlow:F1} < {context.Definition.MinWaterFlow:F1}).");
            return false;
        }

        private static IEnumerable<GridCoordinate> GetNeighbors(GridCoordinate c, GridService grid)
        {
            for (var dx = -1; dx <= 1; dx++)
            {
                for (var dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0)
                    {
                        continue;
                    }

                    var n = new GridCoordinate(c.X + dx, c.Y + dy);
                    if (grid.InBounds(n))
                    {
                        yield return n;
                    }
                }
            }
        }
    }

    public sealed class AdjacentToWaterRule : IPlacementRule
    {
        public string RuleId => "adjacent_to_water";

        public bool Evaluate(PlacementContext context, List<string> failureReasons)
        {
            if (!context.Definition.RequiresAdjacentWater)
            {
                return true;
            }

            var cells = BuildingFootprint.GetCells(
                context.Definition, context.Coordinate, context.Rotation);
            for (var i = 0; i < cells.Count; i++)
            {
                if (!context.Grid.TryGetCell(cells[i], out _))
                {
                    failureReasons.Add("Cell is out of bounds.");
                    return false;
                }

                for (var dx = -1; dx <= 1; dx++)
                {
                    for (var dy = -1; dy <= 1; dy++)
                    {
                        if (dx == 0 && dy == 0)
                        {
                            continue;
                        }

                        var n = new GridCoordinate(cells[i].X + dx, cells[i].Y + dy);
                        if (context.Grid.TryGetCell(n, out var neighbor) && neighbor.IsWater)
                        {
                            return true;
                        }
                    }
                }
            }

            failureReasons.Add("Must be adjacent to a water cell.");
            return false;
        }
    }

    public sealed class MinSolarPotentialRule : IPlacementRule
    {
        public string RuleId => "min_solar";

        public bool Evaluate(PlacementContext context, List<string> failureReasons)
        {
            if (context.Definition.MinSolarPotential <= 0f)
            {
                return true;
            }

            var cells = BuildingFootprint.GetCells(
                context.Definition, context.Coordinate, context.Rotation);
            for (var i = 0; i < cells.Count; i++)
            {
                if (!context.Grid.TryGetCell(cells[i], out var cell))
                {
                    failureReasons.Add("Cell is out of bounds.");
                    return false;
                }

                if (cell.SolarPotential >= context.Definition.MinSolarPotential)
                {
                    continue;
                }

                failureReasons.Add(
                    $"Solar potential too low ({cell.SolarPotential:F2} < {context.Definition.MinSolarPotential:F2}).");
                return false;
            }

            return true;
        }
    }

    public sealed class MinWindPotentialRule : IPlacementRule
    {
        public string RuleId => "min_wind";

        public bool Evaluate(PlacementContext context, List<string> failureReasons)
        {
            if (context.Definition.MinWindPotential <= 0f)
            {
                return true;
            }

            var cells = BuildingFootprint.GetCells(
                context.Definition, context.Coordinate, context.Rotation);
            for (var i = 0; i < cells.Count; i++)
            {
                if (!context.Grid.TryGetCell(cells[i], out var cell))
                {
                    failureReasons.Add("Cell is out of bounds.");
                    return false;
                }

                if (cell.WindPotential >= context.Definition.MinWindPotential)
                {
                    continue;
                }

                failureReasons.Add(
                    $"Wind potential too low ({cell.WindPotential:F2} < {context.Definition.MinWindPotential:F2}).");
                return false;
            }

            return true;
        }
    }

    public sealed class MinSameTypeSpacingRule : IPlacementRule
    {
        public string RuleId => "same_type_spacing";

        public bool Evaluate(PlacementContext context, List<string> failureReasons)
        {
            var spacing = context.Definition.MinSameTypeSpacing;
            if (spacing <= 0 || context.Occupancy == null)
            {
                return true;
            }

            var id = context.Definition.Id;
            var seen = new HashSet<string>();
            foreach (var pair in context.Occupancy.Occupied)
            {
                var other = pair.Value;
                if (other?.Definition == null || other.Definition.Id != id)
                {
                    continue;
                }

                if (!seen.Add(other.InstanceId))
                {
                    continue;
                }

                var distance = Manhattan(context.Coordinate, other.Coordinate);
                if (distance < spacing)
                {
                    failureReasons.Add(
                        $"Too close to another {context.Definition.DisplayName} (need spacing {spacing}, have {distance}).");
                    return false;
                }
            }

            return true;
        }

        private static int Manhattan(GridCoordinate a, GridCoordinate b)
        {
            var dx = a.X - b.X;
            var dy = a.Y - b.Y;
            if (dx < 0) dx = -dx;
            if (dy < 0) dy = -dy;
            return dx + dy;
        }
    }

    public sealed class GridOccupancyRule : IPlacementRule
    {
        public string RuleId => "occupancy";

        public bool Evaluate(PlacementContext context, List<string> failureReasons)
        {
            var cells = BuildingFootprint.GetCells(
                context.Definition, context.Coordinate, context.Rotation);
            for (var i = 0; i < cells.Count; i++)
            {
                if (!context.Grid.InBounds(cells[i]))
                {
                    failureReasons.Add("Cell is out of bounds.");
                    return false;
                }

                if (!context.Occupancy.IsOccupied(cells[i]))
                {
                    continue;
                }

                failureReasons.Add("Cell is already occupied.");
                return false;
            }

            return true;
        }
    }

    public sealed class BuildableCellRule : IPlacementRule
    {
        public string RuleId => "buildable";

        public bool Evaluate(PlacementContext context, List<string> failureReasons)
        {
            if (!context.Definition.RequireBuildableCell)
            {
                return true;
            }

            var cells = BuildingFootprint.GetCells(
                context.Definition, context.Coordinate, context.Rotation);
            for (var i = 0; i < cells.Count; i++)
            {
                if (!context.Grid.TryGetCell(cells[i], out var cell))
                {
                    failureReasons.Add("Cell is out of bounds.");
                    return false;
                }

                // Water-adjacent hydro can sit on a non-buildable shore edge if still not water itself.
                if (context.Definition.RequiresAdjacentWater && !cell.IsWater)
                {
                    continue;
                }

                if (cell.IsBuildable && !cell.IsWater)
                {
                    continue;
                }

                failureReasons.Add("Terrain is not buildable.");
                return false;
            }

            return true;
        }
    }

    public sealed class AffordabilityRule : IPlacementRule
    {
        public string RuleId => "affordability";

        public bool Evaluate(PlacementContext context, List<string> failureReasons)
        {
            if (context.Wallet != null && context.Wallet.CanAfford(context.EffectiveCost))
            {
                return true;
            }

            failureReasons.Add(
                $"Not enough money (need {context.EffectiveCost:F0}, have {context.Wallet?.Money:F0}).");
            return false;
        }
    }

    public sealed class TechnologyUnlockedRule : IPlacementRule
    {
        public string RuleId => "technology";

        public bool Evaluate(PlacementContext context, List<string> failureReasons)
        {
            if (context.BuildingUnlocks == null)
            {
                return true;
            }

            if (context.BuildingUnlocks.IsBuildingUnlocked(context.Definition.Id))
            {
                return true;
            }

            failureReasons.Add($"Technology locked: {context.Definition.DisplayName}.");
            return false;
        }
    }

    /// <summary>
    /// Producers and storage must place within link range of an existing network node.
    /// Empty map: only hubs and consumers (village) may place freely.
    /// Distance uses each building's anchor cell only.
    /// </summary>
    public sealed class NetworkConnectionRule : IPlacementRule
    {
        public const string FailReason = "Must connect to an existing network node.";

        public string RuleId => "network_connection";

        public bool Evaluate(PlacementContext context, List<string> failureReasons)
        {
            var definition = context.Definition;
            if (definition == null || (!definition.IsProducer && !definition.IsStorage))
            {
                return true;
            }

            var occupancy = context.Occupancy;
            if (occupancy == null || occupancy.Occupied.Count == 0)
            {
                failureReasons.Add(FailReason);
                return false;
            }

            var placedRange = Mathf.Max(0, definition.ConnectionRange);
            var seen = new HashSet<string>();
            foreach (var pair in occupancy.Occupied)
            {
                var instance = pair.Value;
                var other = instance?.Definition;
                if (other == null || !IsNetworkNode(other))
                {
                    continue;
                }

                if (!seen.Add(instance.InstanceId))
                {
                    continue;
                }

                var allowed = Mathf.Max(placedRange, Mathf.Max(0, other.ConnectionRange));
                if (Manhattan(context.Coordinate, instance.Coordinate) <= allowed)
                {
                    return true;
                }
            }

            failureReasons.Add(FailReason);
            return false;
        }

        private static bool IsNetworkNode(BuildingDefinition definition)
        {
            return definition.IsNetworkHub
                   || definition.IsProducer
                   || definition.IsConsumer
                   || definition.IsStorage;
        }

        private static int Manhattan(GridCoordinate a, GridCoordinate b)
        {
            var dx = a.X - b.X;
            var dy = a.Y - b.Y;
            if (dx < 0) dx = -dx;
            if (dy < 0) dy = -dy;
            return dx + dy;
        }
    }
}
