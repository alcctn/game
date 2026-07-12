using System.Collections.Generic;
using CleanEnergy.Buildings;
using CleanEnergy.Economy;
using CleanEnergy.Grid;
using CleanEnergy.Scenario;
using CleanEnergy.Settlements;
using CleanEnergy.Workers;

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
        public IActiveSettlementQuery Settlement { get; }
        public IWorkerQuery Workers { get; }
        public LevelDefinition Level { get; }

        public float BuildCost => PowerLinePlacementCost.ComputeEffectiveCost(
            Definition, Coordinate, Occupancy);

        public float AutoConnectCost =>
            Level != null
                ? AutoConnectionCost.Compute(
                    Definition,
                    Coordinate,
                    Settlement,
                    Level.ConnectionCostPerCell,
                    Level.AutoConnectEnabled)
                : 0f;

        public float EffectiveCost => BuildCost + AutoConnectCost;

        public PlacementContext(
            BuildingDefinition definition,
            GridCoordinate coordinate,
            GridService grid,
            GridOccupancyService occupancy,
            Wallet wallet,
            IBuildingUnlockQuery buildingUnlocks = null,
            int rotation = 0,
            IActiveSettlementQuery settlement = null,
            IWorkerQuery workers = null,
            LevelDefinition level = null)
        {
            Definition = definition;
            Coordinate = coordinate;
            Rotation = rotation;
            Grid = grid;
            Occupancy = occupancy;
            Wallet = wallet;
            BuildingUnlocks = buildingUnlocks;
            Settlement = settlement;
            Workers = workers;
            Level = level;
        }
    }

    public interface IPlacementRule
    {
        string RuleId { get; }
        bool Evaluate(PlacementContext context, List<string> failureReasons);
    }
}
