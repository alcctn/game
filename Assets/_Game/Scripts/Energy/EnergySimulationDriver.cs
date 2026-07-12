using System.Collections.Generic;
using CleanEnergy.Economy;
using CleanEnergy.Grid;
using CleanEnergy.Maintenance;
using CleanEnergy.Map;
using CleanEnergy.Placement;
using CleanEnergy.Simulation;
using UnityEngine;

namespace CleanEnergy.Energy
{
    /// <summary>
    /// Drives energy balance each simulation tick and updates wallet from surplus sales / upkeep.
    /// </summary>
    public sealed class EnergySimulationDriver : MonoBehaviour
    {
        [SerializeField] private SimulationClock clock;
        [SerializeField] private EnergyNetworkService networkService;
        [SerializeField] private PlacementController placementController;
        [SerializeField] private MaintenanceController maintenanceController;
        [SerializeField] private MapGenerator mapGenerator;
        [SerializeField] private float surplusSellPrice = 0.5f;

        private readonly EnergyBalanceCalculator _balanceCalculator = new EnergyBalanceCalculator();
        private readonly UpkeepService _upkeepService = new UpkeepService();
        private readonly EmergencyCreditService _emergencyCredit = new EmergencyCreditService();
        private readonly Dictionary<GridCoordinate, float> _hubUtilization =
            new Dictionary<GridCoordinate, float>();
        private readonly HashSet<GridCoordinate> _energyNodeCells = new HashSet<GridCoordinate>();
        private readonly Dictionary<GridCoordinate, float> _productionRatios =
            new Dictionary<GridCoordinate, float>();
        private readonly HashSet<GridCoordinate> _occupiedNonProducerCells = new HashSet<GridCoordinate>();
        private EnergyBalanceResult _lastResult = new EnergyBalanceResult(0f, 0f, 0f, 0f, 0f);

        public EnergyBalanceResult LastResult => _lastResult;
        public UpkeepService Upkeep => _upkeepService;
        public EmergencyCreditService EmergencyCredit => _emergencyCredit;
        public float LastUpkeepTotal => _upkeepService.LastUpkeepTotal;
        public bool CouldNotAffordFullUpkeep => _upkeepService.CouldNotAffordFullUpkeep;

        public event System.Action<EnergyShortageEvent> ShortageOccurred;
        public event System.Action<EnergyBalanceResult> BalanceUpdated;
        public event System.Action EmergencyCreditGranted;

        private void OnEnable()
        {
            if (clock != null)
            {
                clock.Ticked += OnTick;
            }

            SubscribeMap();
        }

        private void OnDisable()
        {
            if (clock != null)
            {
                clock.Ticked -= OnTick;
            }

            UnsubscribeMap();
        }

        public void Configure(
            SimulationClock simulationClock,
            EnergyNetworkService network,
            PlacementController placement,
            MaintenanceController maintenance = null,
            MapGenerator generator = null)
        {
            if (clock != null)
            {
                clock.Ticked -= OnTick;
            }

            UnsubscribeMap();

            clock = simulationClock;
            networkService = network;
            placementController = placement;
            maintenanceController = maintenance;
            mapGenerator = generator;

            if (clock != null)
            {
                clock.Ticked += OnTick;
            }

            SubscribeMap();
        }

        public bool TryGetHubUtilization(GridCoordinate coordinate, out float utilization)
        {
            return _hubUtilization.TryGetValue(coordinate, out utilization);
        }

        public bool IsEnergyNetworkCell(GridCoordinate coordinate)
        {
            return _energyNodeCells.Contains(coordinate);
        }

        public bool TryGetProductionRatio(GridCoordinate coordinate, out float ratio)
        {
            return _productionRatios.TryGetValue(coordinate, out ratio);
        }

        public bool IsOccupiedNonProducer(GridCoordinate coordinate)
        {
            return _occupiedNonProducerCells.Contains(coordinate);
        }

        private void OnTick(SimulationContext context)
        {
            if (networkService == null)
            {
                return;
            }

            maintenanceController?.ProcessTick();
            networkService.RebuildIfNeeded();
            RefreshNetworkSnapshot(context);

            if (_lastResult.SurplusSold > 0f && placementController?.Wallet != null)
            {
                placementController.Wallet.Add(_lastResult.SurplusSold * surplusSellPrice);
            }

            if (placementController != null)
            {
                _upkeepService.ProcessTick(
                    placementController.Occupancy.Occupied,
                    placementController.Wallet);

                if (_emergencyCredit.TryGrant(placementController.Wallet))
                {
                    EmergencyCreditGranted?.Invoke();
                }
            }

            BalanceUpdated?.Invoke(_lastResult);
            if (_lastResult.HasShortage)
            {
                ShortageOccurred?.Invoke(new EnergyShortageEvent(_lastResult.Shortage));
            }
        }

        private void RefreshNetworkSnapshot(SimulationContext context)
        {
            _hubUtilization.Clear();
            _energyNodeCells.Clear();
            _productionRatios.Clear();
            _occupiedNonProducerCells.Clear();

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

            foreach (var component in networkService.Graph.Components)
            {
                var result = _balanceCalculator.Calculate(component, context);
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

                var util = NetworkUtilization.Compute(result.DeliveredProduction, result.LinkCapacity);
                for (var i = 0; i < component.Nodes.Count; i++)
                {
                    var node = component.Nodes[i];
                    if (node.IsHub)
                    {
                        _hubUtilization[node.Coordinate] = util;
                    }
                    else if (node.Producer != null || node.Consumer != null || node.Storage != null)
                    {
                        _energyNodeCells.Add(node.Coordinate);
                    }
                }
            }

            RefreshProductionSnapshot();

            _lastResult = new EnergyBalanceResult(
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

        private void RefreshProductionSnapshot()
        {
            if (placementController == null)
            {
                return;
            }

            foreach (var pair in placementController.Occupancy.Occupied)
            {
                var building = pair.Value;
                if (building?.Definition == null)
                {
                    continue;
                }

                if (building.Definition.IsProducer)
                {
                    _productionRatios[pair.Key] = ProductionUtilization.ComputeRatio(
                        building.CurrentProduction,
                        building.Definition.InstalledPower);
                }
                else
                {
                    _occupiedNonProducerCells.Add(pair.Key);
                }
            }
        }

        private void SubscribeMap()
        {
            if (mapGenerator != null)
            {
                mapGenerator.Events.MapGenerated += OnMapGenerated;
            }
        }

        private void UnsubscribeMap()
        {
            if (mapGenerator != null)
            {
                mapGenerator.Events.MapGenerated -= OnMapGenerated;
            }
        }

        private void OnMapGenerated(Core.MapGeneratedEvent _)
        {
            _emergencyCredit.Reset();
            _hubUtilization.Clear();
            _energyNodeCells.Clear();
            _productionRatios.Clear();
            _occupiedNonProducerCells.Clear();
        }
    }
}
