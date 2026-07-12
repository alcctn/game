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
    /// Wires gameplay events into the ordered Level 1 tutorial progress service.
    /// </summary>
    public sealed class TutorialController : MonoBehaviour
    {
        [SerializeField] private IsometricCameraController cameraController;
        [SerializeField] private MapDebugOverlay debugOverlay;
        [SerializeField] private PlacementController placementController;
        [SerializeField] private ResearchController researchController;
        [SerializeField] private ScenarioController scenarioController;
        [SerializeField] private LevelController levelController;
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
            MapGenerator generator,
            LevelController level = null)
        {
            Unsubscribe();
            cameraController = camera;
            debugOverlay = overlay;
            placementController = placement;
            researchController = research;
            scenarioController = scenario;
            levelController = level;
            mapGenerator = generator;
            EnsureProgress();
            RefreshEnabled();
            Subscribe();
            StepChanged?.Invoke(_progress.CurrentStep);
        }

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

            if (placementController != null)
            {
                placementController.BuildingPlaced += OnBuildingPlaced;
            }

            if (levelController != null)
            {
                levelController.StateChanged += OnLevelStateChanged;
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

            if (placementController != null)
            {
                placementController.BuildingPlaced -= OnBuildingPlaced;
            }

            if (levelController != null)
            {
                levelController.StateChanged -= OnLevelStateChanged;
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

        private void OnBuildingPlaced(BuildingPlacedEvent evt)
        {
            if (_suppressEvents || !_enabled || _progress == null || evt?.Instance?.Definition == null)
            {
                return;
            }

            switch (evt.Instance.Definition.Id)
            {
                case "water_wheel":
                case "small_hydro":
                    _progress.TryComplete(TutorialStepId.PlaceWaterWheel);
                    break;
                case "small_wind":
                    _progress.TryComplete(TutorialStepId.PlaceWind);
                    break;
            }
        }

        private void OnLevelStateChanged(LevelObjectiveState state)
        {
            if (_suppressEvents || !_enabled || _progress == null || state == null)
            {
                return;
            }

            if (state.EngineerComplete)
            {
                _progress.TryComplete(TutorialStepId.HireEngineer);
            }

            if (state.TechnicianComplete)
            {
                _progress.TryComplete(TutorialStepId.HireTechnician);
            }

            if (state.CoverageComplete || state.HasCompletedLevel
                || state.CoverageStreakTicks >= TutorialProgressService.MeetDemandStreakTicks)
            {
                _progress.TryComplete(TutorialStepId.MeetDemand);
            }
        }

        private void OnStepChanged(TutorialStepId step)
        {
            StepChanged?.Invoke(step);
        }
    }
}
