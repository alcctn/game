using CleanEnergy.Buildings;
using CleanEnergy.Grid;
using CleanEnergy.Map;
using CleanEnergy.Placement;
using CleanEnergy.Simulation;
using UnityEngine;

namespace CleanEnergy.Energy
{
    /// <summary>
    /// Producer that scales installed power by local resource potential.
    /// </summary>
    public sealed class ResourceProducerAdapter : IEnergyProducer
    {
        private readonly BuildingInstance _instance;
        private readonly GridService _grid;
        private readonly MapGenerationSettings _settings;
        private readonly GridOccupancyService _occupancy;
        private readonly System.Func<string, float> _efficiencyBonusProvider;

        public string NodeId => _instance.InstanceId;

        public ResourceProducerAdapter(
            BuildingInstance instance,
            GridService grid,
            MapGenerationSettings settings,
            System.Func<string, float> efficiencyBonusProvider = null,
            GridOccupancyService occupancy = null)
        {
            _instance = instance;
            _grid = grid;
            _settings = settings;
            _efficiencyBonusProvider = efficiencyBonusProvider;
            _occupancy = occupancy;
        }

        public float GetAvailableProduction(SimulationContext context)
        {
            var def = _instance.Definition;
            if (def == null || def.InstalledPower <= 0f)
            {
                return 0f;
            }

            var bonus = _efficiencyBonusProvider != null ? _efficiencyBonusProvider(def.Id) : 0f;
            var production = ProductionEstimate.Estimate(
                def,
                _instance.Coordinate,
                _grid,
                _settings,
                context,
                _occupancy,
                bonus,
                _instance.MaintenanceLevel,
                _instance.InstanceId);
            _instance.CurrentProduction = production;
            return _instance.CurrentProduction;
        }

        public void ClearReportedProduction()
        {
            _instance.CurrentProduction = 0f;
        }

        public void SetReportedProduction(float amount)
        {
            _instance.CurrentProduction = Mathf.Max(0f, amount);
        }
    }

    public sealed class VillageConsumerAdapter : IEnergyConsumer
    {
        private readonly BuildingInstance _instance;
        private readonly System.Func<float> _demandScaleProvider;

        public string NodeId => _instance.InstanceId;

        public VillageConsumerAdapter(
            BuildingInstance instance,
            System.Func<float> demandScaleProvider = null)
        {
            _instance = instance;
            _demandScaleProvider = demandScaleProvider;
        }

        public float GetDemand(SimulationContext context)
        {
            var baseDemand = Mathf.Max(0f, _instance.Definition.BaseDemand);
            var scale = _demandScaleProvider != null ? Mathf.Max(0f, _demandScaleProvider()) : 1f;
            return baseDemand * scale * Mathf.Max(0f, context.DemandMultiplier);
        }
    }

    public sealed class BatteryStorageAdapter : IEnergyStorage
    {
        private readonly BuildingInstance _instance;
        private readonly System.Func<string, float> _capacityBonusProvider;

        public string NodeId => _instance.InstanceId;
        public float StoredEnergy => _instance.StoredEnergy;
        public float Capacity
        {
            get
            {
                var baseCap = Mathf.Max(0f, _instance.Definition.StorageCapacity);
                var bonus = _capacityBonusProvider != null
                    ? Mathf.Max(0f, _capacityBonusProvider(_instance.Definition.Id))
                    : 0f;
                return baseCap * (1f + bonus);
            }
        }
        public float MaxChargeRate => Mathf.Max(0f, _instance.Definition.ChargeRate);
        public float MaxDischargeRate => Mathf.Max(0f, _instance.Definition.DischargeRate);

        public BatteryStorageAdapter(
            BuildingInstance instance,
            System.Func<string, float> capacityBonusProvider = null)
        {
            _instance = instance;
            _capacityBonusProvider = capacityBonusProvider;
            _instance.StoredEnergy = Mathf.Clamp(_instance.StoredEnergy, 0f, Capacity);
        }

        public float Charge(float amount)
        {
            if (amount <= 0f || Capacity <= 0f)
            {
                return 0f;
            }

            var room = Capacity - _instance.StoredEnergy;
            var accepted = Mathf.Min(amount, room, MaxChargeRate);
            _instance.StoredEnergy += accepted;
            return accepted;
        }

        public float Discharge(float amount)
        {
            if (amount <= 0f)
            {
                return 0f;
            }

            var provided = Mathf.Min(amount, _instance.StoredEnergy, MaxDischargeRate);
            _instance.StoredEnergy -= provided;
            return provided;
        }
    }

    public static class BuildingEnergyFactory
    {
        public static IEnergyProducer TryCreateProducer(
            BuildingInstance instance,
            GridService grid,
            MapGenerationSettings settings,
            System.Func<string, float> efficiencyBonusProvider = null,
            GridOccupancyService occupancy = null)
        {
            if (instance?.Definition == null || !instance.Definition.IsProducer)
            {
                return null;
            }

            return new ResourceProducerAdapter(
                instance, grid, settings, efficiencyBonusProvider, occupancy);
        }

        public static IEnergyConsumer TryCreateConsumer(
            BuildingInstance instance,
            System.Func<float> demandScaleProvider = null)
        {
            if (instance?.Definition == null || !instance.Definition.IsConsumer)
            {
                return null;
            }

            return new VillageConsumerAdapter(instance, demandScaleProvider);
        }

        public static IEnergyStorage TryCreateStorage(
            BuildingInstance instance,
            System.Func<string, float> capacityBonusProvider = null)
        {
            if (instance?.Definition == null || !instance.Definition.IsStorage)
            {
                return null;
            }

            return new BatteryStorageAdapter(instance, capacityBonusProvider);
        }
    }
}
