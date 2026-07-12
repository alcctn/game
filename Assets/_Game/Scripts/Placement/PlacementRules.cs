using System.Collections.Generic;
using CleanEnergy.Grid;

namespace CleanEnergy.Placement
{
    public sealed class MaxSlopeRule : IPlacementRule
    {
        public string RuleId => "max_slope";

        public bool Evaluate(PlacementContext context, List<string> failureReasons)
        {
            if (!context.Grid.TryGetCell(context.Coordinate, out var cell))
            {
                failureReasons.Add("Cell is out of bounds.");
                return false;
            }

            if (cell.Slope <= context.Definition.MaxSlopeDegrees)
            {
                return true;
            }

            failureReasons.Add(
                $"Slope exceeds limit ({cell.Slope:F1}° > {context.Definition.MaxSlopeDegrees:F1}°).");
            return false;
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

            if (!context.Grid.TryGetCell(context.Coordinate, out _))
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

                    var n = new GridCoordinate(context.Coordinate.X + dx, context.Coordinate.Y + dy);
                    if (context.Grid.TryGetCell(n, out var neighbor) && neighbor.IsWater)
                    {
                        return true;
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

            if (!context.Grid.TryGetCell(context.Coordinate, out var cell))
            {
                failureReasons.Add("Cell is out of bounds.");
                return false;
            }

            if (cell.SolarPotential >= context.Definition.MinSolarPotential)
            {
                return true;
            }

            failureReasons.Add(
                $"Solar potential too low ({cell.SolarPotential:F2} < {context.Definition.MinSolarPotential:F2}).");
            return false;
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

            if (!context.Grid.TryGetCell(context.Coordinate, out var cell))
            {
                failureReasons.Add("Cell is out of bounds.");
                return false;
            }

            if (cell.WindPotential >= context.Definition.MinWindPotential)
            {
                return true;
            }

            failureReasons.Add(
                $"Wind potential too low ({cell.WindPotential:F2} < {context.Definition.MinWindPotential:F2}).");
            return false;
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
            foreach (var pair in context.Occupancy.Occupied)
            {
                var other = pair.Value;
                if (other?.Definition == null || other.Definition.Id != id)
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
            if (!context.Occupancy.IsOccupied(context.Coordinate))
            {
                return true;
            }

            failureReasons.Add("Cell is already occupied.");
            return false;
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

            if (!context.Grid.TryGetCell(context.Coordinate, out var cell))
            {
                failureReasons.Add("Cell is out of bounds.");
                return false;
            }

            // Water-adjacent hydro can sit on a non-buildable shore edge if still not water itself.
            if (context.Definition.RequiresAdjacentWater && !cell.IsWater)
            {
                return true;
            }

            if (cell.IsBuildable && !cell.IsWater)
            {
                return true;
            }

            failureReasons.Add("Terrain is not buildable.");
            return false;
        }
    }

    public sealed class AffordabilityRule : IPlacementRule
    {
        public string RuleId => "affordability";

        public bool Evaluate(PlacementContext context, List<string> failureReasons)
        {
            if (context.Wallet != null && context.Wallet.CanAfford(context.Definition.Cost))
            {
                return true;
            }

            failureReasons.Add(
                $"Not enough money (need {context.Definition.Cost:F0}, have {context.Wallet?.Money:F0}).");
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
}
