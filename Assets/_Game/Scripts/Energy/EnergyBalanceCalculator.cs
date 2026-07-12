using System.Collections.Generic;
using CleanEnergy.Simulation;

namespace CleanEnergy.Energy
{
    public sealed class EnergyBalanceResult
    {
        public float Production { get; }
        public float Demand { get; }
        public float Stored { get; }
        public float SurplusSold { get; }
        public float Shortage { get; }
        public bool HasShortage => Shortage > 0.0001f;

        /// <summary>
        /// Fraction of demand met after production and battery discharge (0–1).
        /// </summary>
        public float CoverageRatio
        {
            get
            {
                if (Demand <= 0.0001f)
                {
                    return 1f;
                }

                var met = Demand - Shortage;
                if (met <= 0f)
                {
                    return 0f;
                }

                return met >= Demand ? 1f : met / Demand;
            }
        }

        public EnergyBalanceResult(
            float production,
            float demand,
            float stored,
            float surplusSold,
            float shortage)
        {
            Production = production;
            Demand = demand;
            Stored = stored;
            SurplusSold = surplusSold;
            Shortage = shortage;
        }
    }

    /// <summary>
    /// Deterministic energy balance for one network component.
    /// </summary>
    public sealed class EnergyBalanceCalculator
    {
        private readonly StorageDispatchService _storageDispatch = new StorageDispatchService();

        public EnergyBalanceResult Calculate(
            EnergyNetworkComponent component,
            SimulationContext context)
        {
            var producers = new List<IEnergyProducer>();
            var consumers = new List<IEnergyConsumer>();
            var storages = new List<IEnergyStorage>();

            for (var i = 0; i < component.Nodes.Count; i++)
            {
                var node = component.Nodes[i];
                if (node.Producer != null) producers.Add(node.Producer);
                if (node.Consumer != null) consumers.Add(node.Consumer);
                if (node.Storage != null) storages.Add(node.Storage);
            }

            var production = 0f;
            for (var i = 0; i < producers.Count; i++)
            {
                production += producers[i].GetAvailableProduction(context);
            }

            var demand = 0f;
            for (var i = 0; i < consumers.Count; i++)
            {
                demand += consumers[i].GetDemand(context);
            }

            var available = production;
            var shortage = 0f;
            var surplusSold = 0f;

            if (available >= demand)
            {
                available -= demand;
                var charged = _storageDispatch.ChargeAll(storages, available);
                available -= charged;
                surplusSold = available;
            }
            else
            {
                var deficit = demand - available;
                var discharged = _storageDispatch.DischargeAll(storages, deficit);
                deficit -= discharged;
                shortage = deficit;
            }

            var stored = _storageDispatch.TotalStored(storages);
            return new EnergyBalanceResult(production, demand, stored, surplusSold, shortage);
        }

        public EnergyBalanceResult CalculateNetwork(
            EnergyNetworkGraph graph,
            SimulationContext context)
        {
            var production = 0f;
            var demand = 0f;
            var stored = 0f;
            var sold = 0f;
            var shortage = 0f;

            foreach (var component in graph.Components)
            {
                var result = Calculate(component, context);
                production += result.Production;
                demand += result.Demand;
                stored += result.Stored;
                sold += result.SurplusSold;
                shortage += result.Shortage;
            }

            return new EnergyBalanceResult(production, demand, stored, sold, shortage);
        }
    }
}
