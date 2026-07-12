using CleanEnergy.CameraSystem;
using CleanEnergy.Core;
using CleanEnergy.Placement;
using CleanEnergy.Save;
using CleanEnergy.Scenario;
using CleanEnergy.Simulation;
using UnityEngine;

namespace CleanEnergy.UI
{
    /// <summary>
    /// Escape/Space pause overlay when placement is not active.
    /// </summary>
    public sealed class PauseOverlayUI : MonoBehaviour
    {
        [SerializeField] private SimulationClock clock;
        [SerializeField] private PlacementController placementController;
        [SerializeField] private IsometricCameraController cameraController;
        [SerializeField] private SaveLoadController saveLoadController;

        private bool _overlayVisible;
        private bool _settingsOpen;
        private bool _confirmMainMenu;
        private SimulationSpeed _speedBeforePause = SimulationSpeed.One;

        public bool IsOverlayVisible => _overlayVisible;
        public bool IsSettingsOpen => _settingsOpen;
        public bool IsConfirmingMainMenu => _confirmMainMenu;

        public void Configure(
            SimulationClock simulationClock,
            PlacementController placement,
            IsometricCameraController camera = null,
            SaveLoadController saveLoad = null)
        {
            clock = simulationClock;
            placementController = placement;
            cameraController = camera;
            if (saveLoad != null)
            {
                saveLoadController = saveLoad;
            }
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

            if (_confirmMainMenu)
            {
                CancelMainMenuConfirm();
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
            _confirmMainMenu = false;
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
            _confirmMainMenu = false;
        }

        public void RequestMainMenuConfirm()
        {
            _confirmMainMenu = true;
        }

        public void CancelMainMenuConfirm()
        {
            _confirmMainMenu = false;
        }

        public void GoToMainMenu()
        {
            _overlayVisible = false;
            _settingsOpen = false;
            _confirmMainMenu = false;
            if (clock != null)
            {
                clock.SetSpeed(SimulationSpeed.One);
            }

            SceneFlow.LoadMainMenu();
        }

        /// <summary>Formats save-slot button label; marks the active slot with *.</summary>
        public static string FormatSaveSlotLabel(int slot, int activeSlot)
        {
            var clamped = SaveGameService.ClampSlot(slot);
            var active = SaveGameService.ClampSlot(activeSlot);
            return clamped == active
                ? $"Save Slot {clamped} *"
                : $"Save Slot {clamped}";
        }

        private void OnGUI()
        {
            if (!_overlayVisible)
            {
                return;
            }

            GuiScale.Apply();

            if (_settingsOpen)
            {
                DrawSettingsPanel();
                return;
            }

            if (_confirmMainMenu)
            {
                DrawMainMenuConfirm();
                return;
            }

            const float width = 280f;
            const float height = 360f;
            var x = (Screen.width / GuiScale.Current - width) * 0.5f;
            var y = (Screen.height / GuiScale.Current - height) * 0.5f;
            GUILayout.BeginArea(new Rect(x, y, width, height), GUI.skin.box);
            GUILayout.Label(StringTable.Get(StringKeys.Pause));
            GUILayout.Label(StringTable.Get(StringKeys.EscToResume));
            if (GUILayout.Button(StringTable.Get(StringKeys.Resume)))
            {
                _speedBeforePause = SimulationSpeed.One;
                Resume();
            }

            GUILayout.Space(6f);
            var activeSlot = ResolveActiveSlot();
            for (var slot = 1; slot <= SaveGameService.MaxSlot; slot++)
            {
                var label = FormatSaveSlotLabel(slot, activeSlot);
                if (GUILayout.Button(label, GUILayout.Height(28f)))
                {
                    TrySaveSlot(slot);
                }
            }

            if (GUILayout.Button(StringTable.Get(StringKeys.Settings)))
            {
                _settingsOpen = true;
            }

            if (GUILayout.Button(StringTable.Get(StringKeys.MainMenu)))
            {
                RequestMainMenuConfirm();
            }

            GUILayout.EndArea();
        }

        private void DrawMainMenuConfirm()
        {
            const float width = 300f;
            const float height = 140f;
            var x = (Screen.width / GuiScale.Current - width) * 0.5f;
            var y = (Screen.height / GuiScale.Current - height) * 0.5f;
            GUILayout.BeginArea(new Rect(x, y, width, height), GUI.skin.box);
            GUILayout.Label("Leave to Main Menu?");
            GUILayout.Label("Unsaved progress may be lost.");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Confirm", GUILayout.Height(28f)))
            {
                GoToMainMenu();
            }

            if (GUILayout.Button(StringTable.Get(StringKeys.Back), GUILayout.Height(28f)))
            {
                CancelMainMenuConfirm();
            }

            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        private int ResolveActiveSlot()
        {
            var saveLoad = ResolveSaveLoad();
            if (saveLoad?.Service != null)
            {
                return saveLoad.Service.ActiveSlot;
            }

            return ScenarioSession.ResolveContinueSlot();
        }

        private void TrySaveSlot(int slot)
        {
            var saveLoad = ResolveSaveLoad();
            if (saveLoad == null)
            {
                return;
            }

            saveLoad.SetActiveSlot(slot);
            saveLoad.SaveSlot(slot);
            ScenarioSession.ContinueSlot = SaveGameService.ClampSlot(slot);
        }

        private SaveLoadController ResolveSaveLoad()
        {
            if (saveLoadController != null)
            {
                return saveLoadController;
            }

            saveLoadController = Object.FindObjectOfType<SaveLoadController>();
            return saveLoadController;
        }

        private void DrawSettingsPanel()
        {
            const float width = 320f;
            const float height = 400f;
            var x = (Screen.width / GuiScale.Current - width) * 0.5f;
            var y = (Screen.height / GuiScale.Current - height) * 0.5f;
            GUILayout.BeginArea(new Rect(x, y, width, height), GUI.skin.box);
            GUILayout.Label(StringTable.Get(StringKeys.Settings));
            GUILayout.Space(8f);
            SettingsPanelUI.Draw(cameraController);
            GUILayout.Space(12f);
            if (GUILayout.Button(StringTable.Get(StringKeys.Back), GUILayout.Height(28f)))
            {
                _settingsOpen = false;
            }

            GUILayout.EndArea();
        }
    }
}
