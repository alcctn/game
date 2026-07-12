using CleanEnergy.Map;
using CleanEnergy.Scenario;
using NUnit.Framework;
using UnityEngine;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class FourthScenarioTests
    {
        [Test]
        public void PineBasin_HasHighHydroMidSolarLowWind()
        {
            var def = ScenarioProgressService.CreateRuntimePineBasin();
            Assert.AreEqual("pine_basin", def.ScenarioId);
            Assert.AreEqual("pine_basin_55", def.MapSeed);
            Assert.AreEqual(0.55f, def.BaseClimateSolarOverride, 0.001f);
            Assert.AreEqual(0.2f, def.BaseClimateWindOverride, 0.001f);
            Assert.AreEqual(6f, def.StreamAccumulationOverride, 0.001f);
            Assert.AreEqual("hydro_turbine", def.RequiredResearchNodeIds[0]);
            Assert.Less(def.StreamAccumulationOverride, 12f);
        }

        [Test]
        public void ApplyToMapSettings_WritesHydroFriendlyThresholds()
        {
            var settings = ScriptableObject.CreateInstance<MapGenerationSettings>();
            settings.SetBaseClimateSolar(0.75f);
            settings.SetBaseWind(0.35f);
            settings.SetWaterThresholds(12f, 40f);

            var def = ScenarioProgressService.CreateRuntimePineBasin();
            def.ApplyToMapSettings(settings);

            Assert.AreEqual("pine_basin_55", settings.Seed);
            Assert.AreEqual(0.55f, settings.BaseClimateSolar, 0.001f);
            Assert.AreEqual(0.2f, settings.BaseWind, 0.001f);
            Assert.AreEqual(6f, settings.StreamAccumulationThreshold, 0.001f);
        }
    }
}
