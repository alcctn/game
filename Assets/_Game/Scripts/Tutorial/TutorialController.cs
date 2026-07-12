using CleanEnergy.CameraSystem;
using CleanEnergy.DebugTools;
using CleanEnergy.Map;
using CleanEnergy.Placement;
using CleanEnergy.Research;
using CleanEnergy.Scenario;
using UnityEngine;

namespace CleanEnergy.Tutorial
{
    /// <summary>
    /// Wires gameplay events into the ordered tutorial progress service.
    /// </summary>
    public sealed class TutorialController : MonoBehaviour
    {
        [SerializeField] private IsometricCameraController cameraController;
        [SerializeField] private MapDebugOverlay debugOverlay;
        [SerializeField] private PlacementController placementController;
        [SerializeField] private ResearchController researchController;
        [SerializeField] private ScenarioController scenarioController;
        [SerializeField] private MapGenerator mapGenerator;

        private TutorialProgressService _progress;
        private bool _suppressEvents;
        private bool _enabled = true;

        public TutorialProgressService Progress => _progress;
        public bool IsEnabled => _enabled;
        public bool SuppressEvents
        {
            get => _suppressEvents;
            set => _suppressEvents = value;
        }

        public event System.Action<TutorialStepId> StepChanged;

        private void OnEnable()
        {
            EnsureProgress();
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        public void Configure(
            IsometricCameraController camera,
            MapDebugOverlay overlay,
            PlacementController placement,
            ResearchController research,
            ScenarioController scenario,
            MapGenerator generator)
        {
            Unsubscribe();
            cameraController = camera;
            debugOverlay = overlay;
            placementController = placement;
            researchController = research;
            scenarioController = scenario;
            mapGenerator = generator;
            EnsureProgress();
            RefreshEnabled();
            Subscribe();
            StepChanged?.Invoke(_progress.CurrentStep);
        }

        /// <summary>Enables tutorial only for Green Valley.</summary>
        public void RefreshEnabled()
        {
            var id = scenarioController?.Progress?.Definition?.ScenarioId
                ?? ScenarioSession.ResolveSelectedId();
            _enabled = TutorialProgressService.IsEnabledForScenario(id);
            if (!_enabled)
            {
                EnsureProgress();
                _progress.Restore(TutorialStepId.Completed);
            }
        }

        public void ResetTutorial()
        {
            EnsureProgress();
            RefreshEnabled();
            if (!_enabled)
            {
                return;
            }

            _progress.Reset();
        }

        public void RestoreTutorial(TutorialStepId step)
        {
            EnsureProgress();
            RefreshEnabled();
            if (!_enabled)
            {
                return;
            }

            _progress.Restore(step);
        }

        private void EnsureProgress()
        {
            if (_progress != null)
            {
                return;
            }

            _progress = new TutorialProgressService();
            _progress.StepChanged += OnStepChanged;
        }

        private void Subscribe()
        {
            if (_progress != null)
            {
                _progress.StepChanged -= OnStepChanged;
                _progress.StepChanged += OnStepChanged;
            }

            if (cameraController != null)
            {
                cameraController.CameraInputUsed += OnCameraInput;
            }

            if (debugOverlay != null)
            {
                debugOverlay.ModeChanged += OnDebugModeChanged;
            }

            if (placementController != null)
            {
                placementController.BuildingPlaced += OnBuildingPlaced;
            }

            if (researchController?.Service != null)
            {
                researchController.Service.NodeUnlocked += OnResearchUnlocked;
            }

            if (scenarioController != null)
            {
                scenarioController.StateChanged += OnScenarioStateChanged;
            }

            if (mapGenerator != null)
            {
                mapGenerator.Events.MapGenerated += OnMapGenerated;
            }
        }

        private void Unsubscribe()
        {
            if (_progress != null)
            {
                _progress.StepChanged -= OnStepChanged;
            }

            if (cameraController != null)
            {
                cameraController.CameraInputUsed -= OnCameraInput;
            }

            if (debugOverlay != null)
            {
                debugOverlay.ModeChanged -= OnDebugModeChanged;
            }

            if (placementController != null)
            {
                placementController.BuildingPlaced -= OnBuildingPlaced;
            }

            if (researchController?.Service != null)
            {
                researchController.Service.NodeUnlocked -= OnResearchUnlocked;
            }

            if (scenarioController != null)
            {
                scenarioController.StateChanged -= OnScenarioStateChanged;
            }

            if (mapGenerator != null)
            {
                mapGenerator.Events.MapGenerated -= OnMapGenerated;
            }
        }

        private void OnMapGenerated(Core.MapGeneratedEvent _)
        {
            if (_suppressEvents)
            {
                return;
            }

            ResetTutorial();
        }

        private void OnCameraInput()
        {
            if (_suppressEvents || !_enabled)
            {
                return;
            }

            _progress?.TryComplete(TutorialStepId.Camera);
        }

        private void OnDebugModeChanged(DebugViewMode mode)
        {
            if (_suppressEvents || !_enabled || _progress == null)
            {
                return;
            }

            if (mode == DebugViewMode.Water)
            {
                _progress.TryComplete(TutorialStepId.OpenWaterLayer);
            }
            else if (mode == DebugViewMode.Solar)
            {
                _progress.TryComplete(TutorialStepId.OpenSolarLayer);
            }
        }

        private void OnBuildingPlaced(BuildingPlacedEvent evt)
        {
            if (_suppressEvents || !_enabled || _progress == null || evt?.Instance?.Definition == null)
            {
                return;
            }

            switch (evt.Instance.Definition.Id)
            {
                case "water_wheel":
                    _progress.TryComplete(TutorialStepId.PlaceWaterWheel);
                    break;
                case "power_line":
                    _progress.TryComplete(TutorialStepId.PlacePowerLine);
                    break;
                case "small_solar":
                    _progress.TryComplete(TutorialStepId.PlaceSolar);
                    break;
                case "battery":
                    // Always-unlocked building; no prior research step.
                    _progress.TryComplete(TutorialStepId.PlaceBattery);
                    break;
            }
        }

        private void OnResearchUnlocked(ResearchUnlockedEvent evt)
        {
            if (_suppressEvents || !_enabled || _progress == null || evt == null)
            {
                return;
            }

            if (evt.NodeId == "solar_basic")
            {
                _progress.TryComplete(TutorialStepId.UnlockSolar);
            }
        }

        private void OnScenarioStateChanged(ScenarioObjectiveState state)
        {
            if (_suppressEvents || !_enabled || _progress == null || state == null)
            {
                return;
            }

            if (state.DemandObjectiveComplete
                || state.CoverageStreakTicks >= TutorialProgressService.MeetDemandStreakTicks)
            {
                _progress.TryComplete(TutorialStepId.MeetDemand);
            }
        }

        private void OnStepChanged(TutorialStepId step)
        {
            StepChanged?.Invoke(step);
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }
    }
}
