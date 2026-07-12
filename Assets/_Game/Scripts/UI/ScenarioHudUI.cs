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
        private bool _hideChecklist;
        private string _failReason = ScenarioFailedEvent.DefaultReason;

        public void Configure(ScenarioController controller, bool hideChecklist = false)
        {
            if (scenarioController != null)
            {
                scenarioController.StateChanged -= OnStateChanged;
                scenarioController.Won -= OnWon;
                scenarioController.Failed -= OnFailed;
            }

            scenarioController = controller;
            _hideChecklist = hideChecklist;
            _won = false;
            _lost = false;
            _failReason = ScenarioFailedEvent.DefaultReason;
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

        private void OnFailed(ScenarioFailedEvent evt)
        {
            _lost = true;
            if (evt != null && !string.IsNullOrEmpty(evt.Reason))
            {
                _failReason = evt.Reason;
            }
        }

        private void OnGUI()
        {
            if (!_hideChecklist)
            {
                DrawChecklist();
            }

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

            var area = HudLayout.ScenarioChecklist();
            ImguiHitTest.Register(area, "Scenario");
            GUILayout.BeginArea(area, GUI.skin.box);
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
            var height = 180f;
            var x = (Screen.width - width) * 0.5f;
            var y = (Screen.height - height) * 0.5f;
            GUILayout.BeginArea(new Rect(x, y, width, height), GUI.skin.box);
            GUILayout.Label("Scenario complete");
            GUILayout.Label("Village demand sustained with a diversified network.");
            GUILayout.Label("Press Generate to play again.");
            if (GUILayout.Button("Restart"))
            {
                RestartScenario();
            }

            if (GUILayout.Button("Return to Menu"))
            {
                ReturnToMainMenu();
            }

            GUILayout.EndArea();
        }

        private void DrawLoseOverlay()
        {
            var width = 360f;
            var height = 180f;
            var x = (Screen.width - width) * 0.5f;
            var y = (Screen.height - height) * 0.5f;
            GUILayout.BeginArea(new Rect(x, y, width, height), GUI.skin.box);
            GUILayout.Label("Scenario failed");
            GUILayout.Label(_failReason);
            GUILayout.Label("Press Generate to try again.");
            if (GUILayout.Button("Restart"))
            {
                RestartScenario();
            }

            if (GUILayout.Button("Return to Menu"))
            {
                ReturnToMainMenu();
            }

            GUILayout.EndArea();
        }

        public void RestartScenario()
        {
            CleanEnergy.Core.SceneFlow.LoadPlayScene();
        }

        public void ReturnToMainMenu()
        {
            CleanEnergy.Core.SceneFlow.LoadMainMenu();
        }

        private static string Mark(bool done) => done ? "[x]" : "[ ]";
    }
}
