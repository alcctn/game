using CleanEnergy.Energy;
using CleanEnergy.Maintenance;
using CleanEnergy.Research;
using UnityEngine;

namespace CleanEnergy.UI
{
    /// <summary>
    /// Wires gameplay events into the notification feed.
    /// </summary>
    public sealed class NotificationController : MonoBehaviour
    {
        [SerializeField] private EnergySimulationDriver energyDriver;
        [SerializeField] private ResearchController researchController;
        [SerializeField] private MaintenanceController maintenanceController;
        [SerializeField] private EnergyNetworkService networkService;

        private readonly NotificationService _service = new NotificationService();
        private int _previousLowMaintenance;
        private bool _hadShortage;

        public NotificationService Service => _service;

        public void Configure(
            EnergySimulationDriver driver,
            ResearchController research,
            MaintenanceController maintenance,
            EnergyNetworkService network)
        {
            Unsubscribe();
            energyDriver = driver;
            researchController = research;
            maintenanceController = maintenance;
            networkService = network;
            Subscribe();
        }

        private void OnEnable() => Subscribe();

        private void OnDisable() => Unsubscribe();

        private void Update()
        {
            _service.Prune(Time.unscaledTime);

            var low = maintenanceController != null ? maintenanceController.LowMaintenanceCount : 0;
            if (_previousLowMaintenance <= 0 && low > 0)
            {
                _service.Push("Buildings need maintenance", Time.unscaledTime);
            }

            _previousLowMaintenance = low;
        }

        private void Subscribe()
        {
            if (energyDriver != null)
            {
                energyDriver.ShortageOccurred += OnShortage;
                energyDriver.BalanceUpdated += OnBalance;
            }

            if (researchController?.Service != null)
            {
                researchController.Service.NodeUnlocked += OnResearchUnlocked;
            }
        }

        private void Unsubscribe()
        {
            if (energyDriver != null)
            {
                energyDriver.ShortageOccurred -= OnShortage;
                energyDriver.BalanceUpdated -= OnBalance;
            }

            if (researchController?.Service != null)
            {
                researchController.Service.NodeUnlocked -= OnResearchUnlocked;
            }
        }

        private void OnShortage(EnergyShortageEvent _)
        {
            if (_hadShortage)
            {
                return;
            }

            _hadShortage = true;
            _service.Push("Energy shortage", Time.unscaledTime);
        }

        private void OnBalance(EnergyBalanceResult result)
        {
            if (result == null)
            {
                return;
            }

            if (!result.HasShortage)
            {
                _hadShortage = false;
            }

            if (result.EnergyCharged > 0.0001f && HasFullBattery())
            {
                _service.TryPushBatteryFull(Time.unscaledTime);
            }
        }

        private void OnResearchUnlocked(ResearchUnlockedEvent evt)
        {
            if (evt == null || string.IsNullOrEmpty(evt.NodeId))
            {
                return;
            }

            _service.Push($"Unlocked: {evt.NodeId}", Time.unscaledTime);
        }

        private bool HasFullBattery()
        {
            if (networkService?.Graph == null)
            {
                return false;
            }

            foreach (var pair in networkService.Graph.Nodes)
            {
                var storage = pair.Value.Storage;
                if (storage == null || storage.Capacity <= 0f)
                {
                    continue;
                }

                if (storage.StoredEnergy >= storage.Capacity - 0.01f)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
