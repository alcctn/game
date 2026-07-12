using CleanEnergy.Map;
using CleanEnergy.Scenario;
using NUnit.Framework;
using UnityEngine;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class ThirdScenarioTests
    {
        [Test]
        public void WindCoast_HasHighWindLowSolar()
        {
            var def = ScenarioProgressService.CreateRuntimeWindCoast();
            Assert.AreEqual("wind_coast", def.ScenarioId);
            Assert.AreEqual("wind_coast_77", def.MapSeed);
            Assert.AreEqual(0.35f, def.BaseClimateSolarOverride, 0.001f);
            Assert.AreEqual(0.85f, def.BaseClimateWindOverride, 0.001f);
            Assert.AreEqual("wind_basic", def.RequiredResearchNodeIds[0]);
        }

        [Test]
        public void ApplyToMapSettings_WritesWind()
        {
            var settings = ScriptableObject.CreateInstance<MapGenerationSettings>();
            settings.SetBaseWind(0.2f);
            var def = ScenarioProgressService.CreateRuntimeWindCoast();
            def.ApplyToMapSettings(settings);
            Assert.AreEqual(0.85f, settings.BaseWind, 0.001f);
            Assert.AreEqual(0.35f, settings.BaseClimateSolar, 0.001f);
        }
    }
}
