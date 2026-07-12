using CleanEnergy.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class DayCycleLightingTests
    {
        [Test]
        public void Resolve_DawnNoonEveningNight_FixedTable()
        {
            DayCycleLighting.Resolve(DayPhase.Morning, out var dawnI, out var dawnC, out var dawnA);
            DayCycleLighting.Resolve(DayPhase.Noon, out var noonI, out var noonC, out var noonA);
            DayCycleLighting.Resolve(DayPhase.Evening, out var eveI, out var eveC, out var eveA);
            DayCycleLighting.Resolve(DayPhase.Night, out var nightI, out var nightC, out var nightA);

            Assert.AreEqual(0.55f, dawnI, 0.001f);
            Assert.AreEqual(1.15f, noonI, 0.001f);
            Assert.AreEqual(0.65f, eveI, 0.001f);
            Assert.AreEqual(0.12f, nightI, 0.001f);

            Assert.Greater(noonI, dawnI);
            Assert.Greater(dawnI, nightI);
            Assert.Greater(noonC.r, nightC.r);
            Assert.Greater(noonA.r, nightA.r);
            Assert.Greater(eveC.r, eveC.b);
            Assert.Greater(nightC.b, nightC.r);
        }

        [Test]
        public void ApplyPhase_UpdatesLightAndAmbient()
        {
            var lightGo = new GameObject("DirLight");
            var lightingGo = new GameObject("DayLighting");
            try
            {
                var light = lightGo.AddComponent<Light>();
                light.type = LightType.Directional;
                light.intensity = 0f;
                var lighting = lightingGo.AddComponent<DayCycleLighting>();
                lighting.Configure(null, light);

                lighting.ApplyPhase(DayPhase.Noon, force: true);
                Assert.AreEqual(1.15f, light.intensity, 0.001f);
                Assert.AreEqual(new Color(0.55f, 0.55f, 0.58f), RenderSettings.ambientLight);

                lighting.ApplyPhase(DayPhase.Night, force: true);
                Assert.AreEqual(0.12f, light.intensity, 0.001f);
                Assert.AreEqual(new Color(0.08f, 0.1f, 0.18f), RenderSettings.ambientLight);
            }
            finally
            {
                Object.DestroyImmediate(lightingGo);
                Object.DestroyImmediate(lightGo);
            }
        }

        [Test]
        public void Update_FreezesWhilePaused()
        {
            var clockGo = new GameObject("ClockLight");
            var lightGo = new GameObject("DirLightPause");
            var lightingGo = new GameObject("DayLightingPause");
            try
            {
                var clock = clockGo.AddComponent<SimulationClock>();
                clock.SetSpeed(SimulationSpeed.One);
                clock.RestoreTick(0);
                var light = lightGo.AddComponent<Light>();
                light.type = LightType.Directional;
                var lighting = lightingGo.AddComponent<DayCycleLighting>();
                lighting.Configure(clock, light);
                lighting.ApplyPhase(DayPhase.Noon, force: true);
                var noonIntensity = light.intensity;

                clock.SetSpeed(SimulationSpeed.Paused);
                clock.RestoreTick(DayCycleService.DefaultTicksPerDay / 2);
                lighting.Evaluate();

                Assert.AreEqual(noonIntensity, light.intensity, 0.001f);
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
