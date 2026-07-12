using CleanEnergy.Buildings;
using CleanEnergy.Grid;
using CleanEnergy.Map;
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
        private readonly System.Func<string, float> _efficiencyBonusProvider;

        public string NodeId => _instance.InstanceId;

        public ResourceProducerAdapter(
            BuildingInstance instance,
            GridService grid,
            MapGenerationSettings settings,
            System.Func<string, float> efficiencyBonusProvider = null)
        {
            _instance = instance;
            _grid = grid;
            _settings = settings;
            _efficiencyBonusProvider = efficiencyBonusProvider;
        }

        public float GetAvailableProduction(SimulationContext context)
        {
            var def = _instance.Definition;
            if (def == null || def.InstalledPower <= 0f)
            {
                return 0f;
            }

            var potential = SamplePotential(def.Id, _instance.Coordinate, context);
            var bonus = _efficiencyBonusProvider != null ? _efficiencyBonusProvider(def.Id) : 0f;
            var efficiency = Mathf.Max(0f, def.Efficiency + bonus);
            var maintenance = Mathf.Clamp01(_instance.MaintenanceLevel);
            var production = def.InstalledPower * potential * efficiency * maintenance;
            _instance.CurrentProduction = Mathf.Max(0f, production);
            return _instance.CurrentProduction;
        }

        private float SamplePotential(string buildingId, GridCoordinate coordinate, SimulationContext context)
        {
            if (!_grid.TryGetCell(coordinate, out var cell))
            {
                return 0f;
            }

            switch (buildingId)
            {
                case "water_wheel":
                    return SampleHydroPotential(coordinate, cell);
                case "small_solar":
                    return Mathf.Clamp01(cell.SolarPotential) * Mathf.Clamp01(context.DaylightFactor);
                case "small_wind":
                    return Mathf.Clamp01(cell.WindPotential);
                default:
                    return 1f;
            }
        }

        private float SampleHydroPotential(GridCoordinate coordinate, GridCellData cell)
        {
            var bestFlow = cell.WaterFlow;
            for (var dx = -1; dx <= 1; dx++)
            {
                for (var dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0)
                    {
                        continue;
                    }

                    var n = new GridCoordinate(coordinate.X + dx, coordinate.Y + dy);
                    if (_grid.TryGetCell(n, out var neighbor) && neighbor.WaterFlow > bestFlow)
                    {
                        bestFlow = neighbor.WaterFlow;
                    }
                }
            }

            var threshold = _settings != null ? Mathf.Max(1f, _settings.StreamAccumulationThreshold) : 12f;
            return Mathf.Clamp01(bestFlow / threshold);
        }
    }

    public sealed class VillageConsumerAdapter : IEnergyConsumer
    {
        private readonly BuildingInstance _instance;

        public string NodeId => _instance.InstanceId;

        public VillageConsumerAdapter(BuildingInstance instance)
        {
            _instance = instance;
        }

        public float GetDemand(SimulationContext context)
        {
            var baseDemand = Mathf.Max(0f, _instance.Definition.BaseDemand);
            return baseDemand * Mathf.Max(0f, context.DemandMultiplier);
        }
    }

    public sealed class BatteryStorageAdapter : IEnergyStorage
    {
        private readonly BuildingInstance _instance;

        public string NodeId => _instance.InstanceId;
        public float StoredEnergy => _instance.StoredEnergy;
        public float Capacity => Mathf.Max(0f, _instance.Definition.StorageCapacity);
        public float MaxChargeRate => Mathf.Max(0f, _instance.Definition.ChargeRate);
        public float MaxDischargeRate => Mathf.Max(0f, _instance.Definition.DischargeRate);

        public BatteryStorageAdapter(BuildingInstance instance)
        {
            _instance = instance;
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
            System.Func<string, float> efficiencyBonusProvider = null)
        {
            if (instance?.Definition == null || !instance.Definition.IsProducer)
            {
                return null;
            }

            return new ResourceProducerAdapter(instance, grid, settings, efficiencyBonusProvider);
        }

        public static IEnergyConsumer TryCreateConsumer(BuildingInstance instance)
        {
            if (instance?.Definition == null || !instance.Definition.IsConsumer)
            {
                return null;
            }

            return new VillageConsumerAdapter(instance);
        }

        public static IEnergyStorage TryCreateStorage(BuildingInstance instance)
        {
            if (instance?.Definition == null || !instance.Definition.IsStorage)
            {
                return null;
            }

            return new BatteryStorageAdapter(instance);
        }
    }
}
