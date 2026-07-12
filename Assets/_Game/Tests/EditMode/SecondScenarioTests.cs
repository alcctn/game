using CleanEnergy.Map;
using CleanEnergy.Scenario;
using NUnit.Framework;
using UnityEngine;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class SecondScenarioTests
    {
        [Test]
        public void SunRidge_HasDistinctThresholdsAndOverrides()
        {
            var def = ScenarioProgressService.CreateRuntimeSunRidge();
            Assert.AreEqual("sun_ridge", def.ScenarioId);
            Assert.AreEqual("sun_ridge_42", def.MapSeed);
            Assert.AreEqual(0.95f, def.BaseClimateSolarOverride, 0.001f);
            Assert.AreEqual(18f, def.StreamAccumulationOverride, 0.001f);
            Assert.Less(def.RequiredCoverageRatio, 0.95f);
        }

        [Test]
        public void ApplyToMapSettings_WritesSeedAndSolar()
        {
            var settings = ScriptableObject.CreateInstance<MapGenerationSettings>();
            settings.SetSeed("old");
            settings.SetBaseClimateSolar(0.5f);
            settings.SetWaterThresholds(12f, 40f);

            var def = ScenarioProgressService.CreateRuntimeSunRidge();
            def.ApplyToMapSettings(settings);

            Assert.AreEqual("sun_ridge_42", settings.Seed);
            Assert.AreEqual(0.95f, settings.BaseClimateSolar, 0.001f);
            Assert.AreEqual(18f, settings.StreamAccumulationThreshold, 0.001f);
        }

        [Test]
        public void ScenarioSession_DefaultsToGreenValley()
        {
            ScenarioSession.SelectedId = null;
            Assert.AreEqual("green_valley", ScenarioSession.ResolveSelectedId());
            ScenarioSession.SelectedId = "sun_ridge";
            Assert.AreEqual("sun_ridge", ScenarioSession.ResolveSelectedId());
            ScenarioSession.SelectedId = ScenarioSession.DefaultId;
        }
    }
}
