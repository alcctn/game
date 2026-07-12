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
    /// Runs all placement rules and aggregates failure reasons.
    /// </summary>
    public sealed class PlacementValidator
    {
        private readonly List<IPlacementRule> _rules;

        public PlacementValidator(IEnumerable<IPlacementRule> rules = null)
        {
            _rules = rules != null
                ? new List<IPlacementRule>(rules)
                : CreateDefaultRules();
        }

        public PlacementValidationResult Validate(
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
            var reasons = new List<string>();
            if (definition == null)
            {
                reasons.Add("No building selected.");
                return PlacementValidationResult.Failure(reasons);
            }

            if (grid == null || !grid.IsInitialized)
            {
                reasons.Add("Grid is not ready.");
                return PlacementValidationResult.Failure(reasons);
            }

            if (!grid.InBounds(coordinate))
            {
                reasons.Add("Cell is out of bounds.");
                return PlacementValidationResult.Failure(reasons);
            }

            var context = new PlacementContext(
                definition, coordinate, grid, occupancy, wallet, buildingUnlocks, rotation,
                settlement, workers, level);
            var allPassed = true;
            for (var i = 0; i < _rules.Count; i++)
            {
                if (!_rules[i].Evaluate(context, reasons))
                {
                    allPassed = false;
                }
            }

            return allPassed
                ? PlacementValidationResult.Success()
                : PlacementValidationResult.Failure(reasons);
        }

        public static List<IPlacementRule> CreateDefaultRules(
            IActiveSettlementQuery settlement = null,
            IWorkerQuery workers = null)
        {
            return new List<IPlacementRule>
            {
                new GridOccupancyRule(),
                new BuildableCellRule(),
                new MaxSlopeRule(),
                new AdjacentToWaterRule(),
                new MinWaterFlowRule(),
                new MinSolarPotentialRule(),
                new MinWindPotentialRule(),
                new MinSameTypeSpacingRule(),
                new AffordabilityRule(),
                new TechnologyUnlockedRule(),
                new SettlementRadiusRule(settlement),
                new WorkerRequirementRule(workers),
                new NetworkConnectionRule()
            };
        }

        public static PlacementValidator CreateForLevel(
            IActiveSettlementQuery settlement,
            IWorkerQuery workers)
        {
            return new PlacementValidator(CreateDefaultRules(settlement, workers));
        }
    }
}
