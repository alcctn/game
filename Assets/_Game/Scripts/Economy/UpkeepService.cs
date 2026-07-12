using System.Collections.Generic;
using CleanEnergy.Buildings;
using CleanEnergy.Grid;
using UnityEngine;

namespace CleanEnergy.Economy
{
    /// <summary>
    /// Sums building MaintenanceCost each tick and deducts from the money wallet.
    /// </summary>
    public sealed class UpkeepService
    {
        public float LastUpkeepTotal { get; private set; }
        public bool CouldNotAffordFullUpkeep { get; private set; }

        public float CalculateTotal(IReadOnlyDictionary<GridCoordinate, BuildingInstance> occupied)
        {
            if (occupied == null || occupied.Count == 0)
            {
                return 0f;
            }

            var seen = new HashSet<string>();
            var total = 0f;
            foreach (var pair in occupied)
            {
                var instance = pair.Value;
                if (instance?.Definition == null || !seen.Add(instance.InstanceId))
                {
                    continue;
                }

                total += Mathf.Max(0f, instance.Definition.MaintenanceCost)
                         * MaintenanceMultiplier(instance.MaintenanceLevel);
            }

            return total;
        }

        public static float MaintenanceMultiplier(float maintenanceLevel)
        {
            return Mathf.Clamp(2f - Mathf.Clamp01(maintenanceLevel), 1f, 2f);
        }

        public void ProcessTick(
            IReadOnlyDictionary<GridCoordinate, BuildingInstance> occupied,
            Wallet wallet)
        {
            LastUpkeepTotal = CalculateTotal(occupied);
            CouldNotAffordFullUpkeep = false;

            if (LastUpkeepTotal <= 0f || wallet == null)
            {
                return;
            }

            if (!wallet.TrySpend(LastUpkeepTotal))
            {
                CouldNotAffordFullUpkeep = true;
                wallet.SetMoney(0f);
            }
        }
    }
}
