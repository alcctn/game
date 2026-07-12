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

            building.MaintenanceLevel = MaxLevel;
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
                    targets.Add(instance);
                    totalCost += ManualRepairCost(instance);
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

            for (var i = 0; i < targets.Count; i++)
            {
                targets[i].MaintenanceLevel = MaxLevel;
            }

            repairedCount = targets.Count;
            return true;
        }

        private static int Manhattan(GridCoordinate a, GridCoordinate b)
        {
            return Mathf.Abs(a.X - b.X) + Mathf.Abs(a.Y - b.Y);
        }
    }
}
