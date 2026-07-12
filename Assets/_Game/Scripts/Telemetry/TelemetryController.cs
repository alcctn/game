using CleanEnergy.DebugTools;
using CleanEnergy.Energy;
using CleanEnergy.Map;
using CleanEnergy.Placement;
using CleanEnergy.Scenario;
using UnityEngine;

namespace CleanEnergy.Telemetry
{
    /// <summary>
    /// Wires gameplay events into session telemetry counters.
    /// </summary>
    public sealed class TelemetryController : MonoBehaviour
    {
        [SerializeField] private PlacementController placementController;
        [SerializeField] private EnergySimulationDriver energyDriver;
        [SerializeField] private MapDebugOverlay debugOverlay;
        [SerializeField] private ScenarioController scenarioController;
        [SerializeField] private MapGenerator mapGenerator;

        private readonly SessionTelemetryService _service = new SessionTelemetryService();

        public SessionTelemetryService Service => _service;

        public void Configure(
            PlacementController placement,
            EnergySimulationDriver driver,
            MapDebugOverlay overlay,
            ScenarioController scenario,
            MapGenerator generator)
        {
            Unsubscribe();
            placementController = placement;
            energyDriver = driver;
            debugOverlay = overlay;
            scenarioController = scenario;
            mapGenerator = generator;
            _service.Reset(Time.unscaledTime);
            Subscribe();
        }

        private void OnEnable() => Subscribe();

        private void OnDisable() => Unsubscribe();

        private void Subscribe()
        {
            if (placementController != null)
            {
                placementController.BuildingPlaced += OnBuildingPlaced;
                placementController.PlacementRejected += OnPlacementRejected;
            }

            if (energyDriver != null)
            {
                energyDriver.BalanceUpdated += OnBalance;
            }

            if (debugOverlay != null)
            {
                debugOverlay.ModeChanged += OnModeChanged;
            }

            if (scenarioController != null)
            {
                scenarioController.Won += OnWon;
                scenarioController.Failed += OnFailed;
            }

            if (mapGenerator != null)
            {
                mapGenerator.Events.MapGenerated += OnMapGenerated;
            }
        }

        private void Unsubscribe()
        {
            if (placementController != null)
            {
                placementController.BuildingPlaced -= OnBuildingPlaced;
                placementController.PlacementRejected -= OnPlacementRejected;
            }

            if (energyDriver != null)
            {
                energyDriver.BalanceUpdated -= OnBalance;
            }

            if (debugOverlay != null)
            {
                debugOverlay.ModeChanged -= OnModeChanged;
            }

            if (scenarioController != null)
            {
                scenarioController.Won -= OnWon;
                scenarioController.Failed -= OnFailed;
            }

            if (mapGenerator != null)
            {
                mapGenerator.Events.MapGenerated -= OnMapGenerated;
            }
        }

        private void OnMapGenerated(Core.MapGeneratedEvent _)
        {
            _service.Reset(Time.unscaledTime);
        }

        private void OnBuildingPlaced(BuildingPlacedEvent _)
        {
            _service.RecordBuildingPlaced(Time.unscaledTime);
        }

        private void OnPlacementRejected()
        {
            _service.RecordInvalidPlacement();
        }

        private void OnBalance(EnergyBalanceResult result)
        {
            if (result == null)
            {
                return;
            }

            _service.RecordBalanceTick(result.Production, result.HasShortage, Time.unscaledTime);
        }

        private void OnModeChanged(DebugViewMode mode)
        {
            _service.RecordDebugMode(mode);
        }

        private void OnWon(ScenarioWonEvent _)
        {
            _service.RecordScenarioEnded(lost: false, Time.unscaledTime);
        }

        private void OnFailed(ScenarioFailedEvent _)
        {
            _service.RecordScenarioEnded(lost: true, Time.unscaledTime);
        }
    }
}
