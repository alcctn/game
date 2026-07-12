using CleanEnergy.Placement;
using CleanEnergy.Simulation;
using CleanEnergy.UI;
using UnityEngine;

namespace CleanEnergy.Core
{
    /// <summary>
    /// Space/1/2/3 speed and Ctrl+Z demolish undo hotkeys.
    /// </summary>
    public sealed class PlayHotkeys : MonoBehaviour
    {
        [SerializeField] private SimulationClock clock;
        [SerializeField] private PlacementController placementController;
        [SerializeField] private PauseOverlayUI pauseOverlay;

        public void Configure(
            SimulationClock simulationClock,
            PlacementController placement,
            PauseOverlayUI pause = null)
        {
            clock = simulationClock;
            placementController = placement;
            pauseOverlay = pause;
        }

        private void Update()
        {
            if (pauseOverlay != null && pauseOverlay.IsOverlayVisible)
            {
                return;
            }

            if (placementController != null && placementController.IsPlacementActive)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                TogglePause();
            }

            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
            {
                SetSpeed(SimulationSpeed.One);
            }

            if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
            {
                SetSpeed(SimulationSpeed.Two);
            }

            if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
            {
                SetSpeed(SimulationSpeed.Four);
            }

            if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                && Input.GetKeyDown(KeyCode.Z))
            {
                placementController?.TryUndoLastDemolish();
            }
        }

        private void TogglePause()
        {
            if (clock == null)
            {
                return;
            }

            if (clock.Speed == SimulationSpeed.Paused)
            {
                clock.SetSpeed(SimulationSpeed.One);
            }
            else
            {
                clock.SetSpeed(SimulationSpeed.Paused);
            }
        }

        private void SetSpeed(SimulationSpeed speed)
        {
            if (clock == null)
            {
                return;
            }

            clock.SetSpeed(speed);
        }

        /// <summary>Pure helper for tests.</summary>
        public static SimulationSpeed ResolveTogglePause(SimulationSpeed current)
        {
            return current == SimulationSpeed.Paused ? SimulationSpeed.One : SimulationSpeed.Paused;
        }
    }
}
