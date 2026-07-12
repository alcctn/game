using CleanEnergy.Core;
using CleanEnergy.Scenario;
using UnityEngine;

namespace CleanEnergy.UI
{
    /// <summary>
    /// IMGUI main menu: New Game / Quit (scenario picker wired in Sprint 35).
    /// </summary>
    public sealed class MainMenuUI : MonoBehaviour
    {
        [SerializeField] private string[] scenarioIds = { "green_valley" };
        [SerializeField] private string[] scenarioLabels = { "Yeşil Vadi" };
        private int _selectedScenarioIndex;

        public void ConfigureScenarios(string[] ids, string[] labels)
        {
            scenarioIds = ids ?? new[] { "green_valley" };
            scenarioLabels = labels ?? new[] { "Yeşil Vadi" };
            _selectedScenarioIndex = 0;
        }

        private void OnGUI()
        {
            const float width = 320f;
            const float height = 220f;
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

            if (GUILayout.Button("New Game", GUILayout.Height(36f)))
            {
                var id = scenarioIds != null && scenarioIds.Length > 0
                    ? scenarioIds[Mathf.Clamp(_selectedScenarioIndex, 0, scenarioIds.Length - 1)]
                    : "green_valley";
                ScenarioSession.SelectedId = id;
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
