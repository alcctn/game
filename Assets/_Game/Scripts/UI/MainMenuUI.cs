using CleanEnergy.Core;
using CleanEnergy.Save;
using CleanEnergy.Scenario;
using UnityEngine;

namespace CleanEnergy.UI
{
    /// <summary>
    /// IMGUI main menu: New Game / Continue (slots 1–3) / Settings / Delete / Quit.
    /// </summary>
    public sealed class MainMenuUI : MonoBehaviour
    {
        /// <summary>Prototype scenario catalog order (locked).</summary>
        public static readonly string[] CanonicalScenarioIds =
        {
            "green_valley",
            "sun_ridge",
            "wind_coast",
            "pine_basin",
            "arid_plateau"
        };

        /// <summary>Display labels matching <see cref="CanonicalScenarioIds"/>.</summary>
        public static readonly string[] CanonicalScenarioLabels =
        {
            "Yeşil Vadi",
            "Güneş Sırtı",
            "Rüzgâr Sahili",
            "Çam Havzası",
            "Kurak Plato"
        };

        [SerializeField] private string[] scenarioIds =
        {
            "green_valley",
            "sun_ridge",
            "wind_coast",
            "pine_basin",
            "arid_plateau"
        };

        [SerializeField] private string[] scenarioLabels =
        {
            "Yeşil Vadi",
            "Güneş Sırtı",
            "Rüzgâr Sahili",
            "Çam Havzası",
            "Kurak Plato"
        };

        private int _selectedScenarioIndex;
        private int _selectedSlot = 1;
        private SaveGameService _saveService;
        private bool _settingsOpen;

        private Vector2 _settingsScroll;

        public void ConfigureScenarios(string[] ids, string[] labels)
        {
            scenarioIds = ids ?? CanonicalScenarioIds;
            scenarioLabels = labels ?? CanonicalScenarioLabels;
            _selectedScenarioIndex = 0;
        }

        private void Awake()
        {
            _saveService = new SaveGameService();
            SettingsService.ApplyAll();
        }

        private void OnGUI()
        {
            GuiScale.Apply();

            if (_settingsOpen)
            {
                DrawSettingsPanel();
                return;
            }

            const float width = 360f;
            const float height = 480f;
            var x = (Screen.width / GuiScale.Current - width) * 0.5f;
            var y = (Screen.height / GuiScale.Current - height) * 0.5f;
            GUILayout.BeginArea(new Rect(x, y, width, height), GUI.skin.box);
            GUILayout.Label("Clean Energy");
            GUILayout.Space(8f);

            if (scenarioLabels != null && scenarioLabels.Length > 1)
            {
                GUILayout.Label("Scenario");
                _selectedScenarioIndex = GUILayout.SelectionGrid(
                    _selectedScenarioIndex, scenarioLabels, 1);
            }

            GUILayout.Label("Save slot");
            for (var slot = 1; slot <= SaveGameService.MaxSlot; slot++)
            {
                var label = FormatSlotLabel(slot);
                var was = _selectedSlot == slot;
                var now = GUILayout.Toggle(was, label, GUI.skin.button, GUILayout.Height(28f));
                if (now && !was)
                {
                    _selectedSlot = slot;
                }
            }

            var canContinue = _saveService != null && _saveService.SlotExists(_selectedSlot);
            GUI.enabled = canContinue;
            if (GUILayout.Button(StringTable.Get(StringKeys.Continue), GUILayout.Height(32f)) && canContinue)
            {
                ScenarioSession.ContinueSlot = _selectedSlot;
                ScenarioSession.LoadSaveOnPlay = true;
                SceneFlow.LoadPlayScene();
            }

            GUI.enabled = canContinue;
            if (GUILayout.Button(StringTable.Get(StringKeys.DeleteSlot), GUILayout.Height(28f)) && canContinue)
            {
                _saveService.DeleteSlot(_selectedSlot);
            }

            GUI.enabled = true;

            if (GUILayout.Button(StringTable.Get(StringKeys.NewGame), GUILayout.Height(36f)))
            {
                var id = scenarioIds != null && scenarioIds.Length > 0
                    ? scenarioIds[Mathf.Clamp(_selectedScenarioIndex, 0, scenarioIds.Length - 1)]
                    : CanonicalScenarioIds[0];
                ScenarioSession.SelectedId = id;
                ScenarioSession.LoadSaveOnPlay = false;
                SceneFlow.LoadPlayScene();
            }

            if (GUILayout.Button(StringTable.Get(StringKeys.Settings), GUILayout.Height(28f)))
            {
                _settingsOpen = true;
            }

            if (GUILayout.Button(StringTable.Get(StringKeys.Quit), GUILayout.Height(28f)))
            {
                SceneFlow.QuitGame();
            }

            GUILayout.EndArea();
        }

        private void DrawSettingsPanel()
        {
            const float width = 360f;
            const float height = 520f;
            var x = (Screen.width / GuiScale.Current - width) * 0.5f;
            var y = (Screen.height / GuiScale.Current - height) * 0.5f;
            GUILayout.BeginArea(new Rect(x, y, width, height), GUI.skin.box);
            GUILayout.Label(StringTable.Get(StringKeys.Settings));
            if (GUILayout.Button(StringTable.Get(StringKeys.Back), GUILayout.Height(28f)))
            {
                _settingsOpen = false;
                _settingsScroll = Vector2.zero;
            }

            GUILayout.Space(6f);
            _settingsScroll = GUILayout.BeginScrollView(_settingsScroll);
            SettingsPanelUI.Draw();
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        private string FormatSlotLabel(int slot)
        {
            if (_saveService != null && _saveService.TryReadSummary(slot, out var summary))
            {
                return $"Slot {slot} — {summary.ScenarioId} ${summary.Money:F0} t{summary.TickIndex}";
            }

            return $"Slot {slot} — empty";
        }
    }
}
