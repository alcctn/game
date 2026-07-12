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
        public const float SavedFlashSeconds = 1.25f;

        private enum ConfirmMode
        {
            None = 0,
            MainMenu = 1,
            Delete = 2,
            Overwrite = 3
        }

        [SerializeField] private SimulationClock clock;
        [SerializeField] private PlacementController placementController;
        [SerializeField] private IsometricCameraController cameraController;
        [SerializeField] private SaveLoadController saveLoadController;

        private bool _overlayVisible;
        private bool _settingsOpen;
        private ConfirmMode _confirmMode;
        private int _confirmSlot;
        private SimulationSpeed _speedBeforePause = SimulationSpeed.One;
        private float _savedFlashTimer;
        private int _savedFlashSlot;
        private Vector2 _settingsScroll;

        public bool IsOverlayVisible => _overlayVisible;
        public bool IsSettingsOpen => _settingsOpen;
        public bool IsConfirmingMainMenu => _confirmMode == ConfirmMode.MainMenu;
        public bool IsConfirmingDelete => _confirmMode == ConfirmMode.Delete;
        public bool IsConfirmingOverwrite => _confirmMode == ConfirmMode.Overwrite;
        public int ConfirmDeleteSlot => _confirmMode == ConfirmMode.Delete ? _confirmSlot : 0;
        public int ConfirmOverwriteSlot => _confirmMode == ConfirmMode.Overwrite ? _confirmSlot : 0;
        public bool IsShowingSavedFlash => _savedFlashTimer > 0f;
        public int SavedFlashSlot => _savedFlashSlot;

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
            if (_savedFlashTimer > 0f)
            {
                _savedFlashTimer -= Time.unscaledDeltaTime;
                if (_savedFlashTimer < 0f)
                {
                    _savedFlashTimer = 0f;
                }
            }

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

            if (_confirmMode == ConfirmMode.Delete)
            {
                CancelDeleteConfirm();
                return;
            }

            if (_confirmMode == ConfirmMode.Overwrite)
            {
                CancelOverwriteConfirm();
                return;
            }

            if (_confirmMode == ConfirmMode.MainMenu)
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
            ClearConfirm();
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
            ClearConfirm();
        }

        public void RequestMainMenuConfirm()
        {
            _confirmMode = ConfirmMode.MainMenu;
            _confirmSlot = 0;
        }

        public void CancelMainMenuConfirm()
        {
            if (_confirmMode == ConfirmMode.MainMenu)
            {
                ClearConfirm();
            }
        }

        public void RequestDeleteSlotConfirm(int slot)
        {
            _confirmMode = ConfirmMode.Delete;
            _confirmSlot = SaveGameService.ClampSlot(slot);
        }

        public void CancelDeleteConfirm()
        {
            if (_confirmMode == ConfirmMode.Delete)
            {
                ClearConfirm();
            }
        }

        public void RequestOverwriteConfirm(int slot)
        {
            _confirmMode = ConfirmMode.Overwrite;
            _confirmSlot = SaveGameService.ClampSlot(slot);
        }

        public void CancelOverwriteConfirm()
        {
            if (_confirmMode == ConfirmMode.Overwrite)
            {
                ClearConfirm();
            }
        }

        /// <summary>Deletes the pending confirm slot via SaveLoadController. Returns false if empty/missing.</summary>
        public bool ConfirmDeleteSlotAction()
        {
            if (_confirmMode != ConfirmMode.Delete || _confirmSlot <= 0)
            {
                return false;
            }

            var slot = _confirmSlot;
            ClearConfirm();
            var saveLoad = ResolveSaveLoad();
            if (saveLoad == null)
            {
                return false;
            }

            return saveLoad.DeleteSlot(slot);
        }

        /// <summary>Overwrites the pending slot after confirm.</summary>
        public bool ConfirmOverwriteSlotAction()
        {
            if (_confirmMode != ConfirmMode.Overwrite || _confirmSlot <= 0)
            {
                return false;
            }

            var slot = _confirmSlot;
            ClearConfirm();
            return PerformSave(slot);
        }

        public void GoToMainMenu()
        {
            _overlayVisible = false;
            _settingsOpen = false;
            ClearConfirm();
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

        /// <summary>Formats save-slot button with optional metadata summary.</summary>
        public static string FormatSaveSlotLabel(
            int slot,
            int activeSlot,
            SlotSaveSummary summary,
            bool slotExists)
        {
            var baseLabel = FormatSaveSlotLabel(slot, activeSlot);
            if (slotExists && summary != null)
            {
                return $"{baseLabel} — {summary.ScenarioId} ${summary.Money:F0} t{summary.TickIndex}";
            }

            if (slotExists)
            {
                return baseLabel;
            }

            return $"{baseLabel} — empty";
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

            if (_confirmMode == ConfirmMode.MainMenu)
            {
                DrawMainMenuConfirm();
                return;
            }

            if (_confirmMode == ConfirmMode.Delete)
            {
                DrawDeleteConfirm();
                return;
            }

            if (_confirmMode == ConfirmMode.Overwrite)
            {
                DrawOverwriteConfirm();
                return;
            }

            const float width = 320f;
            const float height = 440f;
            var x = (Screen.width / GuiScale.Current - width) * 0.5f;
            var y = (Screen.height / GuiScale.Current - height) * 0.5f;
            GUILayout.BeginArea(new Rect(x, y, width, height), GUI.skin.box);
            GUILayout.Label(StringTable.Get(StringKeys.Pause));
            GUILayout.Label(StringTable.Get(StringKeys.EscToResume));
            if (IsShowingSavedFlash)
            {
                GUILayout.Label($"Saved slot{_savedFlashSlot}.");
            }

            if (GUILayout.Button(StringTable.Get(StringKeys.Resume)))
            {
                _speedBeforePause = SimulationSpeed.One;
                Resume();
            }

            GUILayout.Space(6f);
            var activeSlot = ResolveActiveSlot();
            var saveLoad = ResolveSaveLoad();
            for (var slot = 1; slot <= SaveGameService.MaxSlot; slot++)
            {
                SlotSaveSummary summary = null;
                var exists = saveLoad?.Service != null && saveLoad.Service.SlotExists(slot);
                if (exists)
                {
                    saveLoad.Service.TryReadSummary(slot, out summary);
                }

                var label = FormatSaveSlotLabel(slot, activeSlot, summary, exists);
                if (GUILayout.Button(label, GUILayout.Height(28f)))
                {
                    TrySaveSlot(slot);
                }
            }

            if (GUILayout.Button(StringTable.Get(StringKeys.DeleteSlot), GUILayout.Height(28f)))
            {
                RequestDeleteSlotConfirm(activeSlot);
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

        private void DrawDeleteConfirm()
        {
            const float width = 300f;
            const float height = 140f;
            var x = (Screen.width / GuiScale.Current - width) * 0.5f;
            var y = (Screen.height / GuiScale.Current - height) * 0.5f;
            GUILayout.BeginArea(new Rect(x, y, width, height), GUI.skin.box);
            GUILayout.Label($"Delete save slot {_confirmSlot}?");
            GUILayout.Label("This cannot be undone.");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Confirm", GUILayout.Height(28f)))
            {
                ConfirmDeleteSlotAction();
            }

            if (GUILayout.Button(StringTable.Get(StringKeys.Back), GUILayout.Height(28f)))
            {
                CancelDeleteConfirm();
            }

            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        private void DrawOverwriteConfirm()
        {
            const float width = 300f;
            const float height = 140f;
            var x = (Screen.width / GuiScale.Current - width) * 0.5f;
            var y = (Screen.height / GuiScale.Current - height) * 0.5f;
            GUILayout.BeginArea(new Rect(x, y, width, height), GUI.skin.box);
            GUILayout.Label($"Overwrite save slot {_confirmSlot}?");
            GUILayout.Label("Existing save will be replaced.");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Confirm", GUILayout.Height(28f)))
            {
                ConfirmOverwriteSlotAction();
            }

            if (GUILayout.Button(StringTable.Get(StringKeys.Back), GUILayout.Height(28f)))
            {
                CancelOverwriteConfirm();
            }

            GUILayout.EndHorizontal();
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
            if (saveLoad?.Service != null && saveLoad.Service.SlotExists(slot))
            {
                RequestOverwriteConfirm(slot);
                return;
            }

            PerformSave(slot);
        }

        private bool PerformSave(int slot)
        {
            var saveLoad = ResolveSaveLoad();
            if (saveLoad == null)
            {
                return false;
            }

            saveLoad.SetActiveSlot(slot);
            var ok = saveLoad.SaveSlot(slot);
            if (ok)
            {
                ScenarioSession.ContinueSlot = SaveGameService.ClampSlot(slot);
                ShowSavedFlash(slot);
            }

            return ok;
        }

        private void ShowSavedFlash(int slot)
        {
            _savedFlashSlot = SaveGameService.ClampSlot(slot);
            _savedFlashTimer = SavedFlashSeconds;
        }

        private void ClearConfirm()
        {
            _confirmMode = ConfirmMode.None;
            _confirmSlot = 0;
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
            const float width = 360f;
            const float height = 520f;
            var x = (Screen.width / GuiScale.Current - width) * 0.5f;
            var y = (Screen.height / GuiScale.Current - height) * 0.5f;
            GUILayout.BeginArea(new Rect(x, y, width, height), GUI.skin.box);
            GUILayout.Label(StringTable.Get(StringKeys.Settings));

            // Back stays above the scroll region so it never clips under keybinds.
            if (GUILayout.Button(StringTable.Get(StringKeys.Back), GUILayout.Height(28f)))
            {
                _settingsOpen = false;
                _settingsScroll = Vector2.zero;
            }

            GUILayout.Space(6f);
            _settingsScroll = GUILayout.BeginScrollView(_settingsScroll);
            SettingsPanelUI.Draw(cameraController);
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }
    }
}
