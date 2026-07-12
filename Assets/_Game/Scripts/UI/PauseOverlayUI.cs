using CleanEnergy.CameraSystem;
using CleanEnergy.Core;
using CleanEnergy.Placement;
using CleanEnergy.Simulation;
using UnityEngine;

namespace CleanEnergy.UI
{
    /// <summary>
    /// Escape pause overlay when placement is not active.
    /// </summary>
    public sealed class PauseOverlayUI : MonoBehaviour
    {
        [SerializeField] private SimulationClock clock;
        [SerializeField] private PlacementController placementController;
        [SerializeField] private IsometricCameraController cameraController;

        private bool _overlayVisible;
        private bool _settingsOpen;
        private SimulationSpeed _speedBeforePause = SimulationSpeed.One;

        public bool IsOverlayVisible => _overlayVisible;
        public bool IsSettingsOpen => _settingsOpen;

        public void Configure(
            SimulationClock simulationClock,
            PlacementController placement,
            IsometricCameraController camera = null)
        {
            clock = simulationClock;
            placementController = placement;
            cameraController = camera;
        }

        private void Update()
        {
            if (!Input.GetKeyDown(KeyCode.Escape))
            {
                return;
            }

            if (placementController != null && placementController.IsPlacementActive)
            {
                return;
            }

            if (_settingsOpen)
            {
                _settingsOpen = false;
                return;
            }

            if (_overlayVisible)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }

        public void Pause()
        {
            if (clock != null)
            {
                if (clock.Speed != SimulationSpeed.Paused)
                {
                    _speedBeforePause = clock.Speed;
                }

                clock.SetSpeed(SimulationSpeed.Paused);
            }

            _overlayVisible = true;
            _settingsOpen = false;
        }

        public void Resume()
        {
            if (clock != null)
            {
                var restore = _speedBeforePause == SimulationSpeed.Paused
                    ? SimulationSpeed.One
                    : _speedBeforePause;
                clock.SetSpeed(restore);
            }

            _overlayVisible = false;
            _settingsOpen = false;
        }

        public void GoToMainMenu()
        {
            _overlayVisible = false;
            _settingsOpen = false;
            if (clock != null)
            {
                clock.SetSpeed(SimulationSpeed.One);
            }

            SceneFlow.LoadMainMenu();
        }

        private void OnGUI()
        {
            if (!_overlayVisible)
            {
                return;
            }

            if (_settingsOpen)
            {
                DrawSettingsPanel();
                return;
            }

            const float width = 280f;
            const float height = 240f;
            var x = (Screen.width - width) * 0.5f;
            var y = (Screen.height - height) * 0.5f;
            GUILayout.BeginArea(new Rect(x, y, width, height), GUI.skin.box);
            GUILayout.Label("Paused");
            GUILayout.Label("Esc to resume");
            if (GUILayout.Button("Resume (1x)"))
            {
                _speedBeforePause = SimulationSpeed.One;
                Resume();
            }

            if (GUILayout.Button("Save", GUILayout.Height(28f)))
            {
                TrySaveActiveSlot();
            }

            if (GUILayout.Button("Settings"))
            {
                _settingsOpen = true;
            }

            if (GUILayout.Button("Main Menu"))
            {
                GoToMainMenu();
            }

            GUILayout.EndArea();
        }

        private void TrySaveActiveSlot()
        {
            var saveLoad = FindObjectOfType<Save.SaveLoadController>();
            if (saveLoad != null)
            {
                saveLoad.SaveSlot();
            }
        }

        private void DrawSettingsPanel()
        {
            const float width = 320f;
            const float height = 280f;
            var x = (Screen.width - width) * 0.5f;
            var y = (Screen.height - height) * 0.5f;
            GUILayout.BeginArea(new Rect(x, y, width, height), GUI.skin.box);
            GUILayout.Label("Settings");
            GUILayout.Space(8f);
            SettingsPanelUI.Draw(cameraController);
            GUILayout.Space(12f);
            if (GUILayout.Button("Back", GUILayout.Height(28f)))
            {
                _settingsOpen = false;
            }

            GUILayout.EndArea();
        }
    }
}
