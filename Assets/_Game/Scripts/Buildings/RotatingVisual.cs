using CleanEnergy.Simulation;
using UnityEngine;

namespace CleanEnergy.Buildings
{
    /// <summary>
    /// Spins the Spin child (or root) around world Y at a fixed RPM. Pauses when simulation is paused.
    /// </summary>
    public sealed class RotatingVisual : MonoBehaviour
    {
        public const float WindRpm = 40f;
        public const float HydroRpm = 25f;
        public const string SpinChildName = "Spin";

        [SerializeField] private float rpm = 25f;
        [SerializeField] private SimulationClock simulationClock;

        private Transform _spinTarget;

        /// <summary>Revolutions per minute around the Y axis.</summary>
        public float Rpm => rpm;

        /// <summary>Cached transform that receives rotation (Spin child or root).</summary>
        public Transform SpinTarget => _spinTarget != null ? _spinTarget : transform;

        public void Configure(float revolutionsPerMinute, SimulationClock clock = null)
        {
            rpm = Mathf.Max(0f, revolutionsPerMinute);
            if (clock != null)
            {
                simulationClock = clock;
            }

            CacheSpinTarget();
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

        /// <summary>
        /// Returns the recursive child named Spin when present; otherwise <paramref name="root"/>.
        /// </summary>
        public static Transform ResolveSpinTarget(Transform root)
        {
            if (root == null)
            {
                return null;
            }

            var spin = FindChildNamed(root, SpinChildName);
            return spin != null ? spin : root;
        }

        private void Awake()
        {
            CacheSpinTarget();
        }

        private void OnEnable()
        {
            CacheSpinTarget();
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

            if (_spinTarget == null)
            {
                CacheSpinTarget();
            }

            // 360 deg / 60 sec * rpm = 6 * rpm degrees per second
            SpinTarget.Rotate(0f, rpm * 6f * deltaTime, 0f, Space.World);
        }

        private void CacheSpinTarget()
        {
            _spinTarget = ResolveSpinTarget(transform);
        }

        private static Transform FindChildNamed(Transform root, string childName)
        {
            for (var i = 0; i < root.childCount; i++)
            {
                var child = root.GetChild(i);
                if (child.name == childName)
                {
                    return child;
                }

                var nested = FindChildNamed(child, childName);
                if (nested != null)
                {
                    return nested;
                }
            }

            return null;
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
