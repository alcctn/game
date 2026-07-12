using CleanEnergy.Economy;
using CleanEnergy.Maintenance;
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
        [SerializeField] private float surplusSellPrice = 0.5f;

        private readonly EnergyBalanceCalculator _balanceCalculator = new EnergyBalanceCalculator();
        private readonly UpkeepService _upkeepService = new UpkeepService();
        private EnergyBalanceResult _lastResult = new EnergyBalanceResult(0f, 0f, 0f, 0f, 0f);

        public EnergyBalanceResult LastResult => _lastResult;
        public UpkeepService Upkeep => _upkeepService;
        public float LastUpkeepTotal => _upkeepService.LastUpkeepTotal;
        public bool CouldNotAffordFullUpkeep => _upkeepService.CouldNotAffordFullUpkeep;

        public event System.Action<EnergyShortageEvent> ShortageOccurred;
        public event System.Action<EnergyBalanceResult> BalanceUpdated;

        private void OnEnable()
        {
            if (clock != null)
            {
                clock.Ticked += OnTick;
            }
        }

        private void OnDisable()
        {
            if (clock != null)
            {
                clock.Ticked -= OnTick;
            }
        }

        public void Configure(
            SimulationClock simulationClock,
            EnergyNetworkService network,
            PlacementController placement,
            MaintenanceController maintenance = null)
        {
            if (clock != null)
            {
                clock.Ticked -= OnTick;
            }

            clock = simulationClock;
            networkService = network;
            placementController = placement;
            maintenanceController = maintenance;
            if (clock != null)
            {
                clock.Ticked += OnTick;
            }
        }

        private void OnTick(SimulationContext context)
        {
            if (networkService == null)
            {
                return;
            }

            maintenanceController?.ProcessTick();
            networkService.RebuildIfNeeded();
            _lastResult = _balanceCalculator.CalculateNetwork(networkService.Graph, context);

            if (_lastResult.SurplusSold > 0f && placementController?.Wallet != null)
            {
                placementController.Wallet.Add(_lastResult.SurplusSold * surplusSellPrice);
            }

            if (placementController != null)
            {
                _upkeepService.ProcessTick(
                    placementController.Occupancy.Occupied,
                    placementController.Wallet);
            }

            BalanceUpdated?.Invoke(_lastResult);
            if (_lastResult.HasShortage)
            {
                ShortageOccurred?.Invoke(new EnergyShortageEvent(_lastResult.Shortage));
            }
        }
    }
}
