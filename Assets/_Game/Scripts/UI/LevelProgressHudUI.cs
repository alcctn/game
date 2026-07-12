using CleanEnergy.Scenario;
using CleanEnergy.Workers;
using UnityEngine;

namespace CleanEnergy.UI
{
    /// <summary>Level 1 progress bar, checklist, and hire buttons.</summary>
    public sealed class LevelProgressHudUI : MonoBehaviour
    {
        [SerializeField] private LevelController levelController;

        private LevelObjectiveState _state;

        public void Configure(LevelController controller)
        {
            if (levelController != null)
            {
                levelController.StateChanged -= OnStateChanged;
            }

            levelController = controller;
            _state = levelController != null ? levelController.State : null;
            if (levelController != null)
            {
                levelController.StateChanged += OnStateChanged;
            }
        }

        private void OnDestroy()
        {
            if (levelController != null)
            {
                levelController.StateChanged -= OnStateChanged;
            }
        }

        private void OnStateChanged(LevelObjectiveState state)
        {
            _state = state;
        }

        private void OnGUI()
        {
            if (levelController == null || _state == null)
            {
                return;
            }

            var area = HudLayout.ScenarioChecklist();
            area.height = 300f;
            GUILayout.BeginArea(area, GUI.skin.box);
            ImguiHitTest.Register(area, "LevelProgress");
            GUILayout.Label(StringTable.Get(StringKeys.Level01Title));
            GUILayout.Label($"{StringTable.Get(StringKeys.LevelProgress)}: {_state.ProgressPercent:F0}%");
            GUILayout.HorizontalSlider(_state.ProgressPercent, 0f, 100f);

            DrawCheck(_state.EngineerComplete, StringTable.Get(StringKeys.LevelObjEngineer));
            DrawCheck(_state.WaterComplete, StringTable.Get(StringKeys.LevelObjWater));
            DrawCheck(_state.TechnicianComplete, StringTable.Get(StringKeys.LevelObjTechnician));
            DrawCheck(_state.WindComplete, StringTable.Get(StringKeys.LevelObjWind));
            DrawCheck(
                _state.CoverageComplete,
                $"{StringTable.Get(StringKeys.LevelObjCoverage)} ({_state.CoverageStreakTicks}/{levelController.Definition.RequiredCoverageTicks})");

            GUILayout.Space(6f);
            var workers = levelController.Workers;
            if (workers != null)
            {
                GUILayout.Label(
                    $"Eng {workers.Pool.EngineerCount} | Tech {workers.Pool.TechnicianCount}");
                if (GUILayout.Button(
                        $"{StringTable.Get(StringKeys.HireEngineer)} ({workers.EngineerHireCost:F0})"))
                {
                    workers.TryHireEngineer();
                }

                if (GUILayout.Button(
                        $"{StringTable.Get(StringKeys.HireTechnician)} ({workers.TechnicianHireCost:F0})"))
                {
                    workers.TryHireTechnician();
                }
            }

            GUILayout.EndArea();
        }

        private static void DrawCheck(bool done, string label)
        {
            GUILayout.Label($"{(done ? "[x]" : "[ ]")} {label}");
        }
    }
}
