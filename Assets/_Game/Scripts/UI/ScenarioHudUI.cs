using CleanEnergy.Scenario;
using UnityEngine;

namespace CleanEnergy.UI
{
    /// <summary>
    /// Checklist HUD, risk banner and win/lose overlay for the active scenario.
    /// </summary>
    public sealed class ScenarioHudUI : MonoBehaviour
    {
        [SerializeField] private ScenarioController scenarioController;

        private ScenarioObjectiveState _state;
        private bool _won;
        private bool _lost;

        public void Configure(ScenarioController controller)
        {
            if (scenarioController != null)
            {
                scenarioController.StateChanged -= OnStateChanged;
                scenarioController.Won -= OnWon;
                scenarioController.Failed -= OnFailed;
            }

            scenarioController = controller;
            _won = false;
            _lost = false;
            _state = scenarioController != null ? scenarioController.State : null;

            if (scenarioController != null)
            {
                scenarioController.StateChanged += OnStateChanged;
                scenarioController.Won += OnWon;
                scenarioController.Failed += OnFailed;
            }
        }

        private void OnDestroy()
        {
            if (scenarioController == null)
            {
                return;
            }

            scenarioController.StateChanged -= OnStateChanged;
            scenarioController.Won -= OnWon;
            scenarioController.Failed -= OnFailed;
        }

        private void OnStateChanged(ScenarioObjectiveState state)
        {
            _state = state;
            if (state == null)
            {
                return;
            }

            if (state.HasWon)
            {
                _won = true;
            }

            if (state.HasLost)
            {
                _lost = true;
            }
        }

        private void OnWon(ScenarioWonEvent _)
        {
            _won = true;
        }

        private void OnFailed(ScenarioFailedEvent _)
        {
            _lost = true;
        }

        private void OnGUI()
        {
            DrawChecklist();
            if (_state != null && _state.IsAtRisk && !_won && !_lost)
            {
                DrawRiskBanner();
            }

            if (_won)
            {
                DrawWinOverlay();
            }
            else if (_lost)
            {
                DrawLoseOverlay();
            }
        }

        private void DrawChecklist()
        {
            var def = scenarioController != null ? scenarioController.Progress?.Definition : null;
            var requiredTicks = def != null ? def.RequiredCoverageTicks : 60;
            var requiredTypes = def != null ? def.RequiredProducerTypes : 2;
            var streak = _state?.CoverageStreakTicks ?? 0;
            var types = _state?.ActiveProducerTypeCount ?? 0;
            var demandDone = _state != null && _state.DemandObjectiveComplete;
            var diversityDone = _state != null && _state.DiversityObjectiveComplete;
            var batteryDone = _state != null && _state.BatteryObjectiveComplete;
            var satisfaction = _state?.Satisfaction ?? 100f;

            GUILayout.BeginArea(new Rect(12f, 90f, 280f, 140f), GUI.skin.box);
            GUILayout.Label(def != null ? def.DisplayName : "Scenario");
            GUILayout.Label($"{Mark(demandDone)} Demand {streak}/{requiredTicks} ({(_state?.CoverageRatio ?? 0f) * 100f:F0}%)");
            GUILayout.Label($"{Mark(diversityDone)} Sources {types}/{requiredTypes}");
            GUILayout.Label($"{Mark(batteryDone)} Battery connected");
            GUILayout.Label($"{Mark(_state != null && _state.ResearchObjectiveComplete)} Research unlock");
            GUILayout.Label($"Satisfaction {satisfaction:F0}");
            GUILayout.EndArea();
        }

        private void DrawRiskBanner()
        {
            var width = 420f;
            var x = (Screen.width - width) * 0.5f;
            GUILayout.BeginArea(new Rect(x, 84f, width, 28f), GUI.skin.box);
            GUILayout.Label("Scenario risk: satisfaction is critically low");
            GUILayout.EndArea();
        }

        private void DrawWinOverlay()
        {
            var width = 360f;
            var height = 110f;
            var x = (Screen.width - width) * 0.5f;
            var y = (Screen.height - height) * 0.5f;
            GUILayout.BeginArea(new Rect(x, y, width, height), GUI.skin.box);
            GUILayout.Label("Scenario complete");
            GUILayout.Label("Village demand sustained with a diversified network.");
            GUILayout.Label("Press Generate to play again.");
            GUILayout.EndArea();
        }

        private void DrawLoseOverlay()
        {
            var width = 360f;
            var height = 110f;
            var x = (Screen.width - width) * 0.5f;
            var y = (Screen.height - height) * 0.5f;
            GUILayout.BeginArea(new Rect(x, y, width, height), GUI.skin.box);
            GUILayout.Label("Scenario failed");
            GUILayout.Label("Village satisfaction collapsed from prolonged shortages.");
            GUILayout.Label("Press Generate to try again.");
            GUILayout.EndArea();
        }

        private static string Mark(bool done) => done ? "[x]" : "[ ]";
    }
}
