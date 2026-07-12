using CleanEnergy.Placement;
using CleanEnergy.Simulation;
using UnityEngine;

namespace CleanEnergy.UI
{
    /// <summary>
    /// Escape pause overlay when placement is not active.
    /// </summary>
    public sealed class PauseOverlayUI : MonoBehaviour
    {
        [SerializeField] private SimulationClock clock;
        [SerializeField] private PlacementController placementController;

        private bool _overlayVisible;
        private SimulationSpeed _speedBeforePause = SimulationSpeed.One;

        public bool IsOverlayVisible => _overlayVisible;

        public void Configure(SimulationClock simulationClock, PlacementController placement)
        {
            clock = simulationClock;
            placementController = placement;
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
        }

        private void OnGUI()
        {
            if (!_overlayVisible)
            {
                return;
            }

            const float width = 280f;
            const float height = 120f;
            var x = (Screen.width - width) * 0.5f;
            var y = (Screen.height - height) * 0.5f;
            GUILayout.BeginArea(new Rect(x, y, width, height), GUI.skin.box);
            GUILayout.Label("Paused");
            GUILayout.Label("Esc to resume");
            if (GUILayout.Button("Resume (1x)"))
            {
                _speedBeforePause = SimulationSpeed.One;
                Resume();
            }

            GUILayout.EndArea();
        }
    }
}
