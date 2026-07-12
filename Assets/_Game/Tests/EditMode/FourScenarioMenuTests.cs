using CleanEnergy.UI;
using NUnit.Framework;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class FourScenarioMenuTests
    {
        [Test]
        public void CanonicalScenarioIds_HasFiveInLockedOrder()
        {
            Assert.AreEqual(5, MainMenuUI.CanonicalScenarioIds.Length);
            CollectionAssert.AreEqual(
                new[] { "green_valley", "sun_ridge", "wind_coast", "pine_basin", "arid_plateau" },
                MainMenuUI.CanonicalScenarioIds);
        }

        [Test]
        public void CanonicalScenarioLabels_MatchIdsLength()
        {
            Assert.AreEqual(
                MainMenuUI.CanonicalScenarioIds.Length,
                MainMenuUI.CanonicalScenarioLabels.Length);
            Assert.AreEqual("Yeşil Vadi", MainMenuUI.CanonicalScenarioLabels[0]);
            Assert.AreEqual("Çam Havzası", MainMenuUI.CanonicalScenarioLabels[3]);
            Assert.AreEqual("Kurak Plato", MainMenuUI.CanonicalScenarioLabels[4]);
        }
    }
}
