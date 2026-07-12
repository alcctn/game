using System.Collections.Generic;
using CleanEnergy.Buildings;
using CleanEnergy.Economy;
using CleanEnergy.Grid;

namespace CleanEnergy.Placement
{
    /// <summary>
    /// Context passed to every placement rule.
    /// </summary>
    public sealed class PlacementContext
    {
        public BuildingDefinition Definition { get; }
        public GridCoordinate Coordinate { get; }
        public GridService Grid { get; }
        public GridOccupancyService Occupancy { get; }
        public Wallet Wallet { get; }

        public PlacementContext(
            BuildingDefinition definition,
            GridCoordinate coordinate,
            GridService grid,
            GridOccupancyService occupancy,
            Wallet wallet)
        {
            Definition = definition;
            Coordinate = coordinate;
            Grid = grid;
            Occupancy = occupancy;
            Wallet = wallet;
        }
    }

    public interface IPlacementRule
    {
        string RuleId { get; }
        bool Evaluate(PlacementContext context, List<string> failureReasons);
    }
}
