using CleanEnergy.Tutorial;
using CleanEnergy.UI;
using NUnit.Framework;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class TutorialPolishTests
    {
        [Test]
        public void GetInfo_LocalizedTitles()
        {
            var en = TutorialProgressService.GetInfo(TutorialStepId.PlaceWind, "en");
            var tr = TutorialProgressService.GetInfo(TutorialStepId.PlaceWind, "tr");
            Assert.IsFalse(string.IsNullOrEmpty(en.Title));
            Assert.IsFalse(string.IsNullOrEmpty(tr.Title));
            Assert.AreNotEqual(en.Title, tr.Title);
        }

        [Test]
        public void SoftHighlight_PrefixesLabel()
        {
            Assert.AreEqual("> Water", TutorialProgressService.FormatSoftHighlightLabel("Water", true));
            Assert.AreEqual("Water", TutorialProgressService.FormatSoftHighlightLabel("Water", false));
        }

        [Test]
        public void ResolveBuildTargetId_Level01Targets()
        {
            Assert.AreEqual("water_wheel", TutorialProgressService.ResolveBuildTargetId(TutorialStepId.PlaceWaterWheel));
            Assert.AreEqual("small_wind", TutorialProgressService.ResolveBuildTargetId(TutorialStepId.PlaceWind));
            Assert.IsNull(TutorialProgressService.ResolveBuildTargetId(TutorialStepId.Camera));
            Assert.IsNull(TutorialProgressService.ResolveBuildTargetId(TutorialStepId.HireEngineer));
        }

        [Test]
        public void LevelStrings_Exist()
        {
            Assert.IsFalse(string.IsNullOrEmpty(StringTable.Get(StringKeys.Level01Title, "en")));
            Assert.IsFalse(string.IsNullOrEmpty(StringTable.Get(StringKeys.HireEngineer, "tr")));
        }
    }
}
