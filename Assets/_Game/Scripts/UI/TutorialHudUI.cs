using CleanEnergy.Tutorial;
using UnityEngine;

namespace CleanEnergy.UI
{
    /// <summary>
    /// Checklist HUD for the ordered Green Valley tutorial.
    /// </summary>
    public sealed class TutorialHudUI : MonoBehaviour
    {
        [SerializeField] private TutorialController tutorialController;

        private TutorialStepId _current = TutorialStepId.Camera;

        public void Configure(TutorialController controller)
        {
            if (tutorialController != null)
            {
                tutorialController.StepChanged -= OnStepChanged;
            }

            tutorialController = controller;
            if (tutorialController != null)
            {
                tutorialController.StepChanged += OnStepChanged;
                if (tutorialController.Progress != null)
                {
                    _current = tutorialController.Progress.CurrentStep;
                }
            }
        }

        private void OnDestroy()
        {
            if (tutorialController != null)
            {
                tutorialController.StepChanged -= OnStepChanged;
            }
        }

        private void OnStepChanged(TutorialStepId step)
        {
            _current = step;
        }

        private void OnGUI()
        {
            if (tutorialController == null || !tutorialController.IsEnabled)
            {
                return;
            }

            var progress = tutorialController.Progress;
            if (progress == null)
            {
                return;
            }

            var info = progress.GetCurrentInfo();
            var area = HudLayout.Tutorial();
            ImguiHitTest.Register(area, "Tutorial");
            GUILayout.BeginArea(area, GUI.skin.box);
            GUILayout.Label(StringTable.Get(StringKeys.Tutorial));
            if (progress.IsComplete)
            {
                GUILayout.Label($"[x] {StringTable.Get(StringKeys.TutorialCompleteTitle)}");
            }
            else
            {
                var index = (int)_current + 1;
                GUILayout.Label($"Step {index}/{TutorialProgressService.StepCount}: {info.Title}");
                GUILayout.Label(info.Hint);
            }

            GUILayout.Space(4f);
            for (var i = 0; i < TutorialProgressService.StepCount; i++)
            {
                var step = (TutorialStepId)i;
                var done = progress.IsComplete || step < _current;
                var active = !progress.IsComplete && step == _current;
                var label = TutorialProgressService.GetInfo(step).Title;
                if (done)
                {
                    GUILayout.Label($"[x] {label}");
                }
                else if (active)
                {
                    GUILayout.Label($"> {label}");
                }
                else
                {
                    GUILayout.Label($"[ ] {label}");
                }
            }

            GUILayout.EndArea();
        }
    }
}
