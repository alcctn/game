using CleanEnergy.Energy;
using CleanEnergy.Maintenance;
using CleanEnergy.Research;
using CleanEnergy.Scenario;
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
        [SerializeField] private ScenarioController scenarioController;

        private readonly NotificationService _service = new NotificationService();
        private int _previousLowMaintenance;
        private bool _hadShortage;
        private bool _hadCongestion;

        public NotificationService Service => _service;

        public void Configure(
            EnergySimulationDriver driver,
            ResearchController research,
            MaintenanceController maintenance,
            EnergyNetworkService network,
            ScenarioController scenario = null)
        {
            Unsubscribe();
            energyDriver = driver;
            researchController = research;
            maintenanceController = maintenance;
            networkService = network;
            scenarioController = scenario;
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
                energyDriver.EmergencyCreditGranted += OnEmergencyCredit;
            }

            if (researchController?.Service != null)
            {
                researchController.Service.NodeUnlocked += OnResearchUnlocked;
            }

            if (scenarioController != null)
            {
                scenarioController.Failed += OnScenarioFailed;
            }
        }

        private void Unsubscribe()
        {
            if (energyDriver != null)
            {
                energyDriver.ShortageOccurred -= OnShortage;
                energyDriver.BalanceUpdated -= OnBalance;
                energyDriver.EmergencyCreditGranted -= OnEmergencyCredit;
            }

            if (researchController?.Service != null)
            {
                researchController.Service.NodeUnlocked -= OnResearchUnlocked;
            }

            if (scenarioController != null)
            {
                scenarioController.Failed -= OnScenarioFailed;
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

            if (result.IsCongested)
            {
                if (!_hadCongestion)
                {
                    _hadCongestion = true;
                    _service.Push("Network congested", Time.unscaledTime);
                }
            }
            else
            {
                _hadCongestion = false;
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

        private void OnEmergencyCredit()
        {
            _service.Push(
                $"Emergency credit +{Economy.EmergencyCreditService.CreditAmount:F0}",
                Time.unscaledTime);
        }

        private void OnScenarioFailed(ScenarioFailedEvent _)
        {
            _service.Push("Scenario failed", Time.unscaledTime);
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
