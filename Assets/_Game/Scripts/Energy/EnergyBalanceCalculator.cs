using System.Collections.Generic;
using CleanEnergy.Grid;
using CleanEnergy.Simulation;
using UnityEngine;

namespace CleanEnergy.Energy
{
    public sealed class EnergyBalanceResult
    {
        public float Production { get; }
        public float Demand { get; }
        public float Stored { get; }
        public float SurplusSold { get; }
        public float Shortage { get; }
        public float EnergyCharged { get; }
        /// <summary>Production after hub link-capacity cap.</summary>
        public float DeliveredProduction { get; }
        /// <summary>Component (or network) hub capacity; 0 means unlimited.</summary>
        public float LinkCapacity { get; }
        public bool IsCongested { get; }
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
            float shortage,
            float energyCharged = 0f,
            float deliveredProduction = -1f,
            float linkCapacity = 0f,
            bool isCongested = false)
        {
            Production = production;
            Demand = demand;
            Stored = stored;
            SurplusSold = surplusSold;
            Shortage = shortage;
            EnergyCharged = energyCharged;
            DeliveredProduction = deliveredProduction < 0f ? production : deliveredProduction;
            LinkCapacity = linkCapacity;
            IsCongested = isCongested;
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
            var hasLoad = NetworkLoadFactor.ComponentHasLoad(component);
            for (var i = 0; i < producers.Count; i++)
            {
                var amount = producers[i].GetAvailableProduction(context);
                if (!hasLoad)
                {
                    amount = 0f;
                    if (producers[i] is ResourceProducerAdapter adapter)
                    {
                        adapter.ClearReportedProduction();
                    }
                }
                else
                {
                    var coord = ResolveProducerCoordinate(component, producers[i].NodeId);
                    var delivery = TransmissionLoss.ResolveDeliveryFactor(coord, component);
                    amount *= delivery;
                    if (producers[i] is ResourceProducerAdapter adapter)
                    {
                        adapter.SetReportedProduction(amount);
                    }
                }

                production += amount;
            }

            var demand = 0f;
            for (var i = 0; i < consumers.Count; i++)
            {
                demand += consumers[i].GetDemand(context);
            }

            var capacity = ResolveComponentCapacity(component);
            var unlimited = capacity < 0f;
            var delivered = unlimited ? production : Mathf.Min(production, capacity);
            var congested = !unlimited && production > capacity + 0.0001f;
            var reportedCapacity = unlimited ? 0f : capacity;

            var available = delivered;
            var shortage = 0f;
            var surplusSold = 0f;
            var energyCharged = 0f;

            if (available >= demand)
            {
                available -= demand;
                energyCharged = _storageDispatch.ChargeAll(storages, available);
                available -= energyCharged;
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
            return new EnergyBalanceResult(
                production,
                demand,
                stored,
                surplusSold,
                shortage,
                energyCharged,
                delivered,
                reportedCapacity,
                congested);
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
            var charged = 0f;
            var delivered = 0f;
            var capacitySum = 0f;
            var hasFiniteCapacity = false;
            var congested = false;

            foreach (var component in graph.Components)
            {
                var result = Calculate(component, context);
                production += result.Production;
                demand += result.Demand;
                stored += result.Stored;
                sold += result.SurplusSold;
                shortage += result.Shortage;
                charged += result.EnergyCharged;
                delivered += result.DeliveredProduction;
                congested |= result.IsCongested;
                if (result.LinkCapacity > 0f)
                {
                    hasFiniteCapacity = true;
                    capacitySum += result.LinkCapacity;
                }
            }

            return new EnergyBalanceResult(
                production,
                demand,
                stored,
                sold,
                shortage,
                charged,
                delivered,
                hasFiniteCapacity ? capacitySum : 0f,
                congested);
        }

        /// <summary>
        /// Returns component hub throughput. Negative means unlimited.
        /// </summary>
        public static float ResolveComponentCapacity(EnergyNetworkComponent component)
        {
            if (component?.Nodes == null || component.Nodes.Count == 0)
            {
                return -1f;
            }

            var sum = 0f;
            var hubCount = 0;
            for (var i = 0; i < component.Nodes.Count; i++)
            {
                var node = component.Nodes[i];
                if (!node.IsHub)
                {
                    continue;
                }

                hubCount++;
                if (node.LinkCapacity <= 0f)
                {
                    return -1f;
                }

                sum += node.LinkCapacity;
            }

            return hubCount == 0 ? -1f : sum;
        }

        private static GridCoordinate ResolveProducerCoordinate(
            EnergyNetworkComponent component,
            string nodeId)
        {
            for (var i = 0; i < component.Nodes.Count; i++)
            {
                if (component.Nodes[i].Id == nodeId)
                {
                    return component.Nodes[i].Coordinate;
                }
            }

            return default;
        }
    }
}
