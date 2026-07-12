using CleanEnergy.Core;
using CleanEnergy.Save;
using CleanEnergy.Scenario;
using UnityEngine;

namespace CleanEnergy.UI
{
    /// <summary>
    /// IMGUI main menu: New Game / Continue (slots 1–3) / Delete / Quit.
    /// </summary>
    public sealed class MainMenuUI : MonoBehaviour
    {
        [SerializeField] private string[] scenarioIds = { "green_valley" };
        [SerializeField] private string[] scenarioLabels = { "Yeşil Vadi" };
        private int _selectedScenarioIndex;
        private int _selectedSlot = 1;
        private SaveGameService _saveService;

        public void ConfigureScenarios(string[] ids, string[] labels)
        {
            scenarioIds = ids ?? new[] { "green_valley" };
            scenarioLabels = labels ?? new[] { "Yeşil Vadi" };
            _selectedScenarioIndex = 0;
        }

        private void Awake()
        {
            _saveService = new SaveGameService();
        }

        private void OnGUI()
        {
            const float width = 360f;
            const float height = 400f;
            var x = (Screen.width - width) * 0.5f;
            var y = (Screen.height - height) * 0.5f;
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
            if (GUILayout.Button("Continue", GUILayout.Height(32f)) && canContinue)
            {
                ScenarioSession.ContinueSlot = _selectedSlot;
                ScenarioSession.LoadSaveOnPlay = true;
                SceneFlow.LoadPlayScene();
            }

            GUI.enabled = canContinue;
            if (GUILayout.Button("Delete Slot", GUILayout.Height(28f)) && canContinue)
            {
                _saveService.DeleteSlot(_selectedSlot);
            }

            GUI.enabled = true;

            if (GUILayout.Button("New Game", GUILayout.Height(36f)))
            {
                var id = scenarioIds != null && scenarioIds.Length > 0
                    ? scenarioIds[Mathf.Clamp(_selectedScenarioIndex, 0, scenarioIds.Length - 1)]
                    : "green_valley";
                ScenarioSession.SelectedId = id;
                ScenarioSession.LoadSaveOnPlay = false;
                SceneFlow.LoadPlayScene();
            }

            if (GUILayout.Button("Quit", GUILayout.Height(28f)))
            {
                SceneFlow.QuitGame();
            }

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
