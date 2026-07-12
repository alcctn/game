using CleanEnergy.Simulation;
using UnityEngine;

namespace CleanEnergy.Buildings
{
    /// <summary>
    /// Spins this transform around world Y at a fixed RPM. Pauses when simulation is paused.
    /// </summary>
    public sealed class RotatingVisual : MonoBehaviour
    {
        public const float WindRpm = 40f;
        public const float HydroRpm = 25f;

        [SerializeField] private float rpm = 25f;
        [SerializeField] private SimulationClock simulationClock;

        /// <summary>Revolutions per minute around the Y axis.</summary>
        public float Rpm => rpm;

        public void Configure(float revolutionsPerMinute, SimulationClock clock = null)
        {
            rpm = Mathf.Max(0f, revolutionsPerMinute);
            if (clock != null)
            {
                simulationClock = clock;
            }
        }

        /// <summary>Resolves default RPM for known rotating building ids; otherwise 0.</summary>
        public static float ResolveRpmForBuildingId(string buildingId)
        {
            switch (buildingId)
            {
                case "small_wind":
                    return WindRpm;
                case "water_wheel":
                case "small_hydro":
                    return HydroRpm;
                default:
                    return 0f;
            }
        }

        private void Update()
        {
            Tick(Time.deltaTime);
        }

        /// <summary>Advances rotation by <paramref name="deltaTime"/> seconds unless paused.</summary>
        public void Tick(float deltaTime)
        {
            if (rpm <= 0f || deltaTime <= 0f)
            {
                return;
            }

            if (IsSimulationPaused())
            {
                return;
            }

            // 360 deg / 60 sec * rpm = 6 * rpm degrees per second
            transform.Rotate(0f, rpm * 6f * deltaTime, 0f, Space.World);
        }

        private bool IsSimulationPaused()
        {
            if (simulationClock == null)
            {
                simulationClock = Object.FindAnyObjectByType<SimulationClock>();
            }

            return simulationClock != null && simulationClock.Speed == SimulationSpeed.Paused;
        }
    }
}
