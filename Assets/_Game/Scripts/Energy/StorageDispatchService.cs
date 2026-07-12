using System.Collections.Generic;
using UnityEngine;

namespace CleanEnergy.Energy
{
    /// <summary>
    /// Charges/discharges storages with per-tick rate limits.
    /// </summary>
    public sealed class StorageDispatchService
    {
        public float ChargeAll(IReadOnlyList<IEnergyStorage> storages, float surplus)
        {
            if (surplus <= 0f || storages == null || storages.Count == 0)
            {
                return 0f;
            }

            var remaining = surplus;
            var acceptedTotal = 0f;
            for (var i = 0; i < storages.Count && remaining > 0f; i++)
            {
                var accepted = storages[i].Charge(remaining);
                acceptedTotal += accepted;
                remaining -= accepted;
            }

            return acceptedTotal;
        }

        public float DischargeAll(IReadOnlyList<IEnergyStorage> storages, float deficit)
        {
            if (deficit <= 0f || storages == null || storages.Count == 0)
            {
                return 0f;
            }

            var remaining = deficit;
            var providedTotal = 0f;
            for (var i = 0; i < storages.Count && remaining > 0f; i++)
            {
                var provided = storages[i].Discharge(remaining);
                providedTotal += provided;
                remaining -= provided;
            }

            return providedTotal;
        }

        public float TotalStored(IReadOnlyList<IEnergyStorage> storages)
        {
            if (storages == null)
            {
                return 0f;
            }

            var sum = 0f;
            for (var i = 0; i < storages.Count; i++)
            {
                sum += Mathf.Max(0f, storages[i].StoredEnergy);
            }

            return sum;
        }
    }
}
