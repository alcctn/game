using CleanEnergy.Map;
using CleanEnergy.Scenario;
using CleanEnergy.UI;
using NUnit.Framework;
using UnityEngine;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class FifthScenarioTests
    {
        [Test]
        public void AridPlateau_HasHighSolarLowWindLowHydro()
        {
            var def = ScenarioProgressService.CreateRuntimeAridPlateau();
            Assert.AreEqual("arid_plateau", def.ScenarioId);
            Assert.AreEqual("arid_plateau_91", def.MapSeed);
            Assert.AreEqual(0.98f, def.BaseClimateSolarOverride, 0.001f);
            Assert.AreEqual(0.15f, def.BaseClimateWindOverride, 0.001f);
            Assert.AreEqual(22f, def.StreamAccumulationOverride, 0.001f);
            Assert.AreEqual("solar_panel", def.RequiredResearchNodeIds[0]);
            Assert.Greater(def.StreamAccumulationOverride, 12f);
        }

        [Test]
        public void ApplyToMapSettings_WritesAridOverrides()
        {
            var settings = ScriptableObject.CreateInstance<MapGenerationSettings>();
            settings.SetBaseClimateSolar(0.75f);
            settings.SetBaseWind(0.35f);
            settings.SetWaterThresholds(12f, 40f);

            var def = ScenarioProgressService.CreateRuntimeAridPlateau();
            def.ApplyToMapSettings(settings);

            Assert.AreEqual("arid_plateau_91", settings.Seed);
            Assert.AreEqual(0.98f, settings.BaseClimateSolar, 0.001f);
            Assert.AreEqual(0.15f, settings.BaseWind, 0.001f);
            Assert.AreEqual(22f, settings.StreamAccumulationThreshold, 0.001f);
        }

        [Test]
        public void CanonicalCatalog_HasFiveIncludingAridPlateau()
        {
            Assert.AreEqual(5, MainMenuUI.CanonicalScenarioIds.Length);
            Assert.AreEqual(
                new[] { "green_valley", "sun_ridge", "wind_coast", "pine_basin", "arid_plateau" },
                MainMenuUI.CanonicalScenarioIds);
            Assert.AreEqual("Kurak Plato", MainMenuUI.CanonicalScenarioLabels[4]);
        }
    }
}
