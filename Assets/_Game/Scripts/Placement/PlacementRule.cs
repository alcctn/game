using System.Collections.Generic;
using CleanEnergy.Buildings;
using CleanEnergy.Economy;
using CleanEnergy.Grid;

namespace CleanEnergy.Placement
{
    /// <summary>
    /// Query used by TechnologyUnlockedRule to gate buildings behind research.
    /// </summary>
    public interface IBuildingUnlockQuery
    {
        bool IsBuildingUnlocked(string buildingId);
    }

    /// <summary>
    /// Context passed to every placement rule.
    /// </summary>
    public sealed class PlacementContext
    {
        public BuildingDefinition Definition { get; }
        public GridCoordinate Coordinate { get; }
        public int Rotation { get; }
        public GridService Grid { get; }
        public GridOccupancyService Occupancy { get; }
        public Wallet Wallet { get; }
        public IBuildingUnlockQuery BuildingUnlocks { get; }

        public float EffectiveCost => PowerLinePlacementCost.ComputeEffectiveCost(
            Definition, Coordinate, Occupancy);

        public PlacementContext(
            BuildingDefinition definition,
            GridCoordinate coordinate,
            GridService grid,
            GridOccupancyService occupancy,
            Wallet wallet,
            IBuildingUnlockQuery buildingUnlocks = null,
            int rotation = 0)
        {
            Definition = definition;
            Coordinate = coordinate;
            Rotation = rotation;
            Grid = grid;
            Occupancy = occupancy;
            Wallet = wallet;
            BuildingUnlocks = buildingUnlocks;
        }
    }

    public interface IPlacementRule
    {
        string RuleId { get; }
        bool Evaluate(PlacementContext context, List<string> failureReasons);
    }
}
