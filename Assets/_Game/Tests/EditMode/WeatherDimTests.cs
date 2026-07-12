using CleanEnergy.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class WeatherDimTests
    {
        [Test]
        public void Cloudy_DimsAmbientByPointSeven()
        {
            DayCycleLighting.Resolve(
                DayPhase.Noon, out _, out _, out var clearAmbient, WeatherEventKind.None);
            DayCycleLighting.Resolve(
                DayPhase.Noon, out _, out _, out var cloudyAmbient, WeatherEventKind.Cloudy);

            Assert.AreEqual(
                clearAmbient.r * DayCycleLighting.CloudyAmbientMultiplier,
                cloudyAmbient.r,
                0.001f);
            Assert.AreEqual(
                clearAmbient.g * DayCycleLighting.CloudyAmbientMultiplier,
                cloudyAmbient.g,
                0.001f);
            Assert.AreEqual(
                clearAmbient.b * DayCycleLighting.CloudyAmbientMultiplier,
                cloudyAmbient.b,
                0.001f);
        }

        [Test]
        public void WindGust_DoesNotChangeAmbient()
        {
            DayCycleLighting.Resolve(
                DayPhase.Noon, out _, out _, out var clearAmbient, WeatherEventKind.None);
            DayCycleLighting.Resolve(
                DayPhase.Noon, out _, out _, out var gustAmbient, WeatherEventKind.WindGust);

            Assert.AreEqual(clearAmbient, gustAmbient);
        }

        [Test]
        public void ApplyPhase_Cloudy_UpdatesRenderSettingsAmbient()
        {
            var lightGo = new GameObject("DirLightDim");
            var lightingGo = new GameObject("DayLightingDim");
            var clockGo = new GameObject("ClockDim");
            try
            {
                var clock = clockGo.AddComponent<SimulationClock>();
                clock.SetSpeed(SimulationSpeed.One);
                var light = lightGo.AddComponent<Light>();
                light.type = LightType.Directional;
                var lighting = lightingGo.AddComponent<DayCycleLighting>();
                lighting.Configure(clock, light);

                lighting.ApplyPhase(DayPhase.Noon, WeatherEventKind.None, force: true);
                var clear = RenderSettings.ambientLight;

                lighting.ApplyPhase(DayPhase.Noon, WeatherEventKind.Cloudy, force: true);
                Assert.AreEqual(clear * DayCycleLighting.CloudyAmbientMultiplier, RenderSettings.ambientLight);
            }
            finally
            {
                Object.DestroyImmediate(lightingGo);
                Object.DestroyImmediate(lightGo);
                Object.DestroyImmediate(clockGo);
            }
        }

        [Test]
        public void Evaluate_FreezesWhilePaused_EvenIfWeatherChanges()
        {
            var clockGo = new GameObject("ClockDimPause");
            var lightGo = new GameObject("DirLightDimPause");
            var lightingGo = new GameObject("DayLightingDimPause");
            try
            {
                var clock = clockGo.AddComponent<SimulationClock>();
                clock.SetSpeed(SimulationSpeed.One);
                var light = lightGo.AddComponent<Light>();
                light.type = LightType.Directional;
                var lighting = lightingGo.AddComponent<DayCycleLighting>();
                lighting.Configure(clock, light);
                lighting.ApplyPhase(DayPhase.Noon, WeatherEventKind.None, force: true);
                var ambientBefore = RenderSettings.ambientLight;

                clock.SetSpeed(SimulationSpeed.Paused);
                clock.Weather.Start(WeatherEventKind.Cloudy);
                lighting.Evaluate();

                Assert.AreEqual(ambientBefore, RenderSettings.ambientLight);
            }
            finally
            {
                Object.DestroyImmediate(lightingGo);
                Object.DestroyImmediate(lightGo);
                Object.DestroyImmediate(clockGo);
            }
        }
    }
}
