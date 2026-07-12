using UnityEngine;

namespace CleanEnergy.Simulation
{
    /// <summary>
    /// Maps <see cref="DayPhase"/> to directional light intensity/color and ambient light.
    /// Cloudy weather dims ambient; pause freezes lighting.
    /// </summary>
    public sealed class DayCycleLighting : MonoBehaviour
    {
        public const float CloudyAmbientMultiplier = 0.7f;

        [SerializeField] private SimulationClock simulationClock;
        [SerializeField] private Light directionalLight;

        private DayPhase _lastApplied = (DayPhase)(-1);
        private WeatherEventKind _lastWeather = (WeatherEventKind)(-1);

        public Light DirectionalLight => directionalLight;

        public void Configure(SimulationClock clock, Light light = null)
        {
            simulationClock = clock;
            if (light != null)
            {
                directionalLight = light;
            }

            EnsureLight();
            if (simulationClock != null)
            {
                ApplyPhase(
                    simulationClock.DayCycle.Phase,
                    simulationClock.Weather.ActiveKind,
                    force: true);
            }
        }

        private void Awake()
        {
            EnsureLight();
        }

        private void Update()
        {
            Evaluate();
        }

        /// <summary>Applies current day-phase lighting unless simulation is paused.</summary>
        public void Evaluate()
        {
            if (simulationClock == null)
            {
                simulationClock = Object.FindAnyObjectByType<SimulationClock>();
                if (simulationClock == null)
                {
                    return;
                }
            }

            if (simulationClock.Speed == SimulationSpeed.Paused)
            {
                return;
            }

            ApplyPhase(simulationClock.DayCycle.Phase, simulationClock.Weather.ActiveKind);
        }

        /// <summary>
        /// Fixed lighting table. Dawn maps to <see cref="DayPhase.Morning"/>.
        /// Cloudy multiplies ambient by <see cref="CloudyAmbientMultiplier"/>; WindGust does not.
        /// </summary>
        public static void Resolve(
            DayPhase phase,
            out float intensity,
            out Color lightColor,
            out Color ambient,
            WeatherEventKind weather = WeatherEventKind.None)
        {
            switch (phase)
            {
                case DayPhase.Morning: // Dawn
                    intensity = 0.55f;
                    lightColor = new Color(1f, 0.72f, 0.45f);
                    ambient = new Color(0.35f, 0.3f, 0.28f);
                    break;
                case DayPhase.Noon:
                    intensity = 1.15f;
                    lightColor = new Color(1f, 0.98f, 0.92f);
                    ambient = new Color(0.55f, 0.55f, 0.58f);
                    break;
                case DayPhase.Evening:
                    intensity = 0.65f;
                    lightColor = new Color(1f, 0.45f, 0.25f);
                    ambient = new Color(0.4f, 0.28f, 0.22f);
                    break;
                case DayPhase.Night:
                default:
                    intensity = 0.12f;
                    lightColor = new Color(0.35f, 0.45f, 0.75f);
                    ambient = new Color(0.08f, 0.1f, 0.18f);
                    break;
            }

            if (weather == WeatherEventKind.Cloudy)
            {
                ambient *= CloudyAmbientMultiplier;
            }
        }

        public void ApplyPhase(DayPhase phase, bool force = false)
        {
            var weather = simulationClock != null
                ? simulationClock.Weather.ActiveKind
                : WeatherEventKind.None;
            ApplyPhase(phase, weather, force);
        }

        public void ApplyPhase(DayPhase phase, WeatherEventKind weather, bool force = false)
        {
            if (!force && phase == _lastApplied && weather == _lastWeather)
            {
                return;
            }

            EnsureLight();
            Resolve(phase, out var intensity, out var lightColor, out var ambient, weather);
            if (directionalLight != null)
            {
                directionalLight.intensity = intensity;
                directionalLight.color = lightColor;
            }

            RenderSettings.ambientLight = ambient;
            _lastApplied = phase;
            _lastWeather = weather;
        }

        private void EnsureLight()
        {
            if (directionalLight != null)
            {
                return;
            }

            var lights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
            for (var i = 0; i < lights.Length; i++)
            {
                if (lights[i] != null && lights[i].type == LightType.Directional)
                {
                    directionalLight = lights[i];
                    return;
                }
            }
        }
    }
}
