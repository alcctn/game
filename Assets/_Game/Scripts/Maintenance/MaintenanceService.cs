using System.Collections.Generic;
using CleanEnergy.Buildings;
using CleanEnergy.Grid;
using CleanEnergy.Placement;
using UnityEngine;

namespace CleanEnergy.Maintenance
{
    /// <summary>
    /// Applies per-tick maintenance decay/repair for energy producers.
    /// </summary>
    public sealed class MaintenanceService
    {
        public const float DecayPerTick = 0.005f;
        public const float RepairPerTick = 0.01f;
        public const float MinLevel = 0.4f;
        public const float MaxLevel = 1f;
        public const float LowThreshold = 0.7f;
        public const string DepotBuildingId = "maintenance_depot";

        public int LowMaintenanceCount { get; private set; }

        public void ProcessTick(IReadOnlyDictionary<GridCoordinate, BuildingInstance> occupied)
        {
            LowMaintenanceCount = 0;
            if (occupied == null || occupied.Count == 0)
            {
                return;
            }

            var depots = new List<BuildingInstance>();
            var producers = new List<BuildingInstance>();
            var seen = new HashSet<string>();

            foreach (var pair in occupied)
            {
                var instance = pair.Value;
                if (instance?.Definition == null || !seen.Add(instance.InstanceId))
                {
                    continue;
                }

                if (instance.Definition.Id == DepotBuildingId)
                {
                    depots.Add(instance);
                }
                else if (instance.Definition.IsProducer)
                {
                    producers.Add(instance);
                }
            }

            for (var i = 0; i < producers.Count; i++)
            {
                var producer = producers[i];
                var covered = IsCoveredByDepot(producer, depots);
                if (covered)
                {
                    producer.MaintenanceLevel = Mathf.Min(
                        MaxLevel,
                        producer.MaintenanceLevel + RepairPerTick);
                }
                else
                {
                    producer.MaintenanceLevel = Mathf.Max(
                        MinLevel,
                        producer.MaintenanceLevel - DecayPerTick);
                }

                if (producer.MaintenanceLevel < LowThreshold)
                {
                    LowMaintenanceCount++;
                }
            }
        }

        public static bool IsCoveredByDepot(
            BuildingInstance producer,
            IReadOnlyList<BuildingInstance> depots)
        {
            if (producer == null || depots == null)
            {
                return false;
            }

            for (var i = 0; i < depots.Count; i++)
            {
                var depot = depots[i];
                if (depot?.Definition == null)
                {
                    continue;
                }

                var range = depot.Definition.ConnectionRange > 0
                    ? depot.Definition.ConnectionRange
                    : 5;
                if (Manhattan(producer.Coordinate, depot.Coordinate) <= range)
                {
                    return true;
                }
            }

            return false;
        }

        public static float ManualRepairCost(BuildingInstance building)
        {
            if (building?.Definition == null)
            {
                return 0f;
            }

            return Mathf.Max(25f, building.Definition.MaintenanceCost * 5f);
        }

        public static bool TryManualRepair(BuildingInstance building, Economy.Wallet wallet, out string failure)
        {
            return TryManualRepair(building, wallet, null, out failure);
        }

        public static bool TryManualRepair(
            BuildingInstance building,
            Economy.Wallet wallet,
            RepairUndoService undo,
            out string failure)
        {
            failure = null;
            if (building?.Definition == null || !building.Definition.IsProducer)
            {
                failure = "Not a producer.";
                return false;
            }

            if (building.MaintenanceLevel >= MaxLevel - 0.001f)
            {
                failure = "Already at full maintenance.";
                return false;
            }

            var cost = ManualRepairCost(building);
            if (wallet == null || !wallet.TrySpend(cost))
            {
                failure = $"Need {cost:F0} money.";
                return false;
            }

            var previous = building.MaintenanceLevel;
            building.MaintenanceLevel = MaxLevel;
            undo?.Push(new List<RepairUndoSnapshot>
            {
                new RepairUndoSnapshot(building.InstanceId, previous, cost)
            });
            return true;
        }

        public static bool TryBulkRepairInDepotRange(
            BuildingInstance depot,
            IReadOnlyDictionary<GridCoordinate, BuildingInstance> occupied,
            Economy.Wallet wallet,
            out int repairedCount,
            out float totalCost,
            out string failure)
        {
            return TryBulkRepairInDepotRange(
                depot, occupied, wallet, null, out repairedCount, out totalCost, out failure);
        }

        public static bool TryBulkRepairInDepotRange(
            BuildingInstance depot,
            IReadOnlyDictionary<GridCoordinate, BuildingInstance> occupied,
            Economy.Wallet wallet,
            RepairUndoService undo,
            out int repairedCount,
            out float totalCost,
            out string failure)
        {
            repairedCount = 0;
            totalCost = 0f;
            failure = null;
            if (depot?.Definition == null || depot.Definition.Id != DepotBuildingId)
            {
                failure = "Select a maintenance depot.";
                return false;
            }

            if (occupied == null)
            {
                failure = "No buildings.";
                return false;
            }

            var targets = new List<BuildingInstance>();
            var costs = new List<float>();
            var seen = new HashSet<string>();
            var depots = new List<BuildingInstance> { depot };
            foreach (var pair in occupied)
            {
                var instance = pair.Value;
                if (instance?.Definition == null
                    || !instance.Definition.IsProducer
                    || !seen.Add(instance.InstanceId))
                {
                    continue;
                }

                if (instance.MaintenanceLevel >= MaxLevel - 0.001f)
                {
                    continue;
                }

                if (IsCoveredByDepot(instance, depots))
                {
                    var cost = ManualRepairCost(instance);
                    targets.Add(instance);
                    costs.Add(cost);
                    totalCost += cost;
                }
            }

            if (targets.Count == 0)
            {
                failure = "No producers need repair in range.";
                return false;
            }

            if (wallet == null || wallet.Money + 0.0001f < totalCost)
            {
                failure = $"Need {totalCost:F0} money.";
                totalCost = 0f;
                return false;
            }

            if (!wallet.TrySpend(totalCost))
            {
                failure = $"Need {totalCost:F0} money.";
                totalCost = 0f;
                return false;
            }

            PushRepairBatch(undo, targets, costs);
            for (var i = 0; i < targets.Count; i++)
            {
                targets[i].MaintenanceLevel = MaxLevel;
            }

            repairedCount = targets.Count;
            return true;
        }

        /// <summary>
        /// Atomically repairs every producer below max maintenance (no depot required).
        /// </summary>
        public static bool TryGlobalRepairAllProducers(
            IReadOnlyDictionary<GridCoordinate, BuildingInstance> occupied,
            Economy.Wallet wallet,
            out int repairedCount,
            out float totalCost,
            out string failure)
        {
            return TryGlobalRepairAllProducers(
                occupied, wallet, null, out repairedCount, out totalCost, out failure);
        }

        public static bool TryGlobalRepairAllProducers(
            IReadOnlyDictionary<GridCoordinate, BuildingInstance> occupied,
            Economy.Wallet wallet,
            RepairUndoService undo,
            out int repairedCount,
            out float totalCost,
            out string failure)
        {
            repairedCount = 0;
            totalCost = 0f;
            failure = null;
            if (occupied == null)
            {
                failure = "No buildings.";
                return false;
            }

            var targets = new List<BuildingInstance>();
            var costs = new List<float>();
            var seen = new HashSet<string>();
            foreach (var pair in occupied)
            {
                var instance = pair.Value;
                if (instance?.Definition == null
                    || !instance.Definition.IsProducer
                    || !seen.Add(instance.InstanceId))
                {
                    continue;
                }

                if (instance.MaintenanceLevel >= MaxLevel - 0.001f)
                {
                    continue;
                }

                var cost = ManualRepairCost(instance);
                targets.Add(instance);
                costs.Add(cost);
                totalCost += cost;
            }

            if (targets.Count == 0)
            {
                failure = "No producers need repair.";
                return false;
            }

            if (wallet == null || wallet.Money + 0.0001f < totalCost)
            {
                failure = $"Need {totalCost:F0} money.";
                totalCost = 0f;
                return false;
            }

            if (!wallet.TrySpend(totalCost))
            {
                failure = $"Need {totalCost:F0} money.";
                totalCost = 0f;
                return false;
            }

            PushRepairBatch(undo, targets, costs);
            for (var i = 0; i < targets.Count; i++)
            {
                targets[i].MaintenanceLevel = MaxLevel;
            }

            repairedCount = targets.Count;
            return true;
        }

        /// <summary>
        /// Atomically repairs selected producers (max 8 cells, no depot required).
        /// </summary>
        public static bool TryRepairSelectedProducers(
            IReadOnlyList<GridCoordinate> coordinates,
            IReadOnlyDictionary<GridCoordinate, BuildingInstance> occupied,
            Economy.Wallet wallet,
            out int repairedCount,
            out float totalCost,
            out string failure)
        {
            return TryRepairSelectedProducers(
                coordinates, occupied, wallet, null, out repairedCount, out totalCost, out failure);
        }

        public static bool TryRepairSelectedProducers(
            IReadOnlyList<GridCoordinate> coordinates,
            IReadOnlyDictionary<GridCoordinate, BuildingInstance> occupied,
            Economy.Wallet wallet,
            RepairUndoService undo,
            out int repairedCount,
            out float totalCost,
            out string failure)
        {
            repairedCount = 0;
            totalCost = 0f;
            failure = null;
            if (coordinates == null || coordinates.Count == 0 || occupied == null)
            {
                failure = "No selection.";
                return false;
            }

            var targets = new List<BuildingInstance>();
            var costs = new List<float>();
            var seen = new HashSet<string>();
            var limit = Mathf.Min(coordinates.Count, 8);
            for (var i = 0; i < limit; i++)
            {
                if (!occupied.TryGetValue(coordinates[i], out var instance)
                    || instance?.Definition == null
                    || !instance.Definition.IsProducer
                    || !seen.Add(instance.InstanceId))
                {
                    continue;
                }

                if (instance.MaintenanceLevel >= MaxLevel - 0.001f)
                {
                    continue;
                }

                var cost = ManualRepairCost(instance);
                targets.Add(instance);
                costs.Add(cost);
                totalCost += cost;
            }

            if (targets.Count == 0)
            {
                failure = "No selected producers need repair.";
                return false;
            }

            if (wallet == null || wallet.Money + 0.0001f < totalCost)
            {
                failure = $"Need {totalCost:F0} money.";
                totalCost = 0f;
                return false;
            }

            if (!wallet.TrySpend(totalCost))
            {
                failure = $"Need {totalCost:F0} money.";
                totalCost = 0f;
                return false;
            }

            PushRepairBatch(undo, targets, costs);
            for (var i = 0; i < targets.Count; i++)
            {
                targets[i].MaintenanceLevel = MaxLevel;
            }

            repairedCount = targets.Count;
            return true;
        }

        private static void PushRepairBatch(
            RepairUndoService undo,
            IReadOnlyList<BuildingInstance> targets,
            IReadOnlyList<float> costs)
        {
            if (undo == null || targets == null || costs == null)
            {
                return;
            }

            var group = new List<RepairUndoSnapshot>(targets.Count);
            for (var i = 0; i < targets.Count; i++)
            {
                var building = targets[i];
                if (building == null)
                {
                    continue;
                }

                group.Add(new RepairUndoSnapshot(
                    building.InstanceId,
                    building.MaintenanceLevel,
                    i < costs.Count ? costs[i] : 0f));
            }

            undo.Push(group);
        }

        private static int Manhattan(GridCoordinate a, GridCoordinate b)
        {
            return Mathf.Abs(a.X - b.X) + Mathf.Abs(a.Y - b.Y);
        }
    }
}
