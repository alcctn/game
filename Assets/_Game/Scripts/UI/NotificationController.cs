using System;
using CleanEnergy.Energy;
using CleanEnergy.Maintenance;
using CleanEnergy.Research;
using CleanEnergy.Scenario;
using CleanEnergy.Simulation;
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
        [SerializeField] private SimulationClock simulationClock;

        private readonly NotificationService _service = new NotificationService();
        private int _previousLowMaintenance;
        private bool _hadShortage;
        private bool _hadCongestion;

        public NotificationService Service => _service;

        /// <summary>Raised when a shortage warning is pushed to the feed.</summary>
        public event Action ShortageWarned;

        /// <summary>Raised when a research unlock is pushed to the feed.</summary>
        public event Action ResearchUnlockNotified;

        /// <summary>Raised when a battery-full notice is pushed to the feed.</summary>
        public event Action BatteryFullNotified;

        /// <summary>Raised when diversity RP bonus is toasted.</summary>
        public event Action DiversityBonusNotified;

        public void Configure(
            EnergySimulationDriver driver,
            ResearchController research,
            MaintenanceController maintenance,
            EnergyNetworkService network,
            ScenarioController scenario = null,
            SimulationClock clock = null)
        {
            Unsubscribe();
            energyDriver = driver;
            researchController = research;
            maintenanceController = maintenance;
            networkService = network;
            scenarioController = scenario;
            simulationClock = clock;
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

            if (researchController != null)
            {
                researchController.DiversityBonusGranted += OnDiversityBonusGranted;
            }

            if (scenarioController != null)
            {
                scenarioController.Failed += OnScenarioFailed;
                scenarioController.Won += OnScenarioWon;
            }

            if (simulationClock != null)
            {
                simulationClock.Weather.EventStarted += OnWeatherStarted;
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

            if (researchController != null)
            {
                researchController.DiversityBonusGranted -= OnDiversityBonusGranted;
            }

            if (scenarioController != null)
            {
                scenarioController.Failed -= OnScenarioFailed;
                scenarioController.Won -= OnScenarioWon;
            }

            if (simulationClock != null)
            {
                simulationClock.Weather.EventStarted -= OnWeatherStarted;
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
            ShortageWarned?.Invoke();
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
                if (_service.TryPushBatteryFull(Time.unscaledTime))
                {
                    BatteryFullNotified?.Invoke();
                }
            }
        }

        private void OnResearchUnlocked(ResearchUnlockedEvent evt)
        {
            if (evt == null || string.IsNullOrEmpty(evt.NodeId))
            {
                return;
            }

            var node = researchController?.Service?.GetNode(evt.NodeId);
            var displayName = ResolveUnlockDisplayName(evt.NodeId, node);
            _service.Push($"Unlocked: {displayName}", Time.unscaledTime);
            ResearchUnlockNotified?.Invoke();
        }

        /// <summary>Prefers research node display name; falls back to id.</summary>
        public static string ResolveUnlockDisplayName(string nodeId, ResearchNodeDefinition node)
        {
            if (node != null && !string.IsNullOrEmpty(node.DisplayName))
            {
                return node.DisplayName;
            }

            return string.IsNullOrEmpty(nodeId) ? string.Empty : nodeId;
        }

        /// <summary>Toast text for diversity RP bonus.</summary>
        public static string FormatDiversityBonusToast(float amount = ResearchProgressTracker.DiversityBonusRp)
        {
            return $"Diversity +{amount:F0} RP";
        }

        /// <summary>Toast text for scenario failure (parity with win toast).</summary>
        public static string FormatFailToast(string reason = null)
        {
            if (string.IsNullOrEmpty(reason))
            {
                return "Scenario failed";
            }

            return $"Scenario failed — {reason}";
        }

        private void OnDiversityBonusGranted()
        {
            _service.Push(FormatDiversityBonusToast(), Time.unscaledTime);
            DiversityBonusNotified?.Invoke();
        }

        private void OnScenarioWon(ScenarioWonEvent _)
        {
            _service.Push("Scenario complete", Time.unscaledTime);
        }

        private void OnEmergencyCredit()
        {
            _service.Push(
                $"Emergency credit +{Economy.EmergencyCreditService.CreditAmount:F0}",
                Time.unscaledTime);
        }

        private void OnScenarioFailed(ScenarioFailedEvent evt)
        {
            _service.Push(FormatFailToast(evt?.Reason), Time.unscaledTime);
        }

        private void OnWeatherStarted(WeatherEventKind kind)
        {
            if (kind == WeatherEventKind.None)
            {
                return;
            }

            _service.Push($"Weather: {kind}", Time.unscaledTime);
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
