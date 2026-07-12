using CleanEnergy.Scenario;
using CleanEnergy.Tutorial;
using CleanEnergy.UI;
using NUnit.Framework;
using UnityEngine;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class TutorialPolishTests
    {
        [TearDown]
        public void TearDown()
        {
            SettingsService.ClearPrefs();
            ScenarioSession.SelectedId = ScenarioSession.DefaultId;
        }

        [Test]
        public void StringTable_HasTutorialTitlesEnAndTr()
        {
            Assert.AreEqual("Move the camera", StringTable.Get(StringKeys.TutorialCameraTitle, "en"));
            Assert.AreEqual("Kamerayı hareket ettir", StringTable.Get(StringKeys.TutorialCameraTitle, "tr"));
            Assert.AreEqual("Sustain village demand", StringTable.Get(StringKeys.TutorialMeetDemandTitle, "en"));
            Assert.AreEqual("Köy talebini sürdür", StringTable.Get(StringKeys.TutorialMeetDemandTitle, "tr"));
        }

        [Test]
        public void GetInfo_UsesLocale()
        {
            var en = TutorialProgressService.GetInfo(TutorialStepId.PlaceSolar, "en");
            var tr = TutorialProgressService.GetInfo(TutorialStepId.PlaceSolar, "tr");
            Assert.AreEqual("Build Small Solar", en.Title);
            Assert.AreEqual("Küçük güneş paneli kur", tr.Title);
        }

        [Test]
        public void IsEnabledForScenario_OnlyGreenValley()
        {
            Assert.IsTrue(TutorialProgressService.IsEnabledForScenario("green_valley"));
            Assert.IsFalse(TutorialProgressService.IsEnabledForScenario("sun_ridge"));
            Assert.IsFalse(TutorialProgressService.IsEnabledForScenario("wind_coast"));
            Assert.IsFalse(TutorialProgressService.IsEnabledForScenario(null));
        }

        [Test]
        public void ResolveBuildTargetId_MapsPlaceSteps()
        {
            Assert.AreEqual("water_wheel", TutorialProgressService.ResolveBuildTargetId(TutorialStepId.PlaceWaterWheel));
            Assert.AreEqual("power_line", TutorialProgressService.ResolveBuildTargetId(TutorialStepId.PlacePowerLine));
            Assert.AreEqual("small_solar", TutorialProgressService.ResolveBuildTargetId(TutorialStepId.PlaceSolar));
            Assert.AreEqual("battery", TutorialProgressService.ResolveBuildTargetId(TutorialStepId.PlaceBattery));
            Assert.IsNull(TutorialProgressService.ResolveBuildTargetId(TutorialStepId.Camera));
        }

        [Test]
        public void SoftHighlight_PrefixesLabel_WithoutLocking()
        {
            Assert.AreEqual("> Water Wheel (100)", TutorialProgressService.FormatSoftHighlightLabel("Water Wheel (100)", true));
            Assert.AreEqual("Water Wheel (100)", TutorialProgressService.FormatSoftHighlightLabel("Water Wheel (100)", false));
        }

        [Test]
        public void Controller_DisablesOutsideGreenValley()
        {
            var go = new GameObject("TutorialPolish");
            try
            {
                ScenarioSession.SelectedId = "sun_ridge";
                var tutorial = go.AddComponent<TutorialController>();
                tutorial.RefreshEnabled();
                Assert.IsFalse(tutorial.IsEnabled);
                Assert.IsTrue(tutorial.Progress.IsComplete);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }
}
