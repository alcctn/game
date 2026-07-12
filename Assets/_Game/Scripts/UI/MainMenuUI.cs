using CleanEnergy.Core;
using CleanEnergy.Save;
using CleanEnergy.Scenario;
using UnityEngine;

namespace CleanEnergy.UI
{
    /// <summary>
    /// IMGUI main menu: New Game / Continue / Quit.
    /// </summary>
    public sealed class MainMenuUI : MonoBehaviour
    {
        [SerializeField] private string[] scenarioIds = { "green_valley" };
        [SerializeField] private string[] scenarioLabels = { "Yeşil Vadi" };
        private int _selectedScenarioIndex;
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
            const float width = 320f;
            const float height = 260f;
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

            var canContinue = _saveService != null && _saveService.SlotExists();
            GUI.enabled = canContinue;
            if (GUILayout.Button("Continue", GUILayout.Height(32f)) && canContinue)
            {
                ScenarioSession.LoadSaveOnPlay = true;
                SceneFlow.LoadPlayScene();
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
    }
}
