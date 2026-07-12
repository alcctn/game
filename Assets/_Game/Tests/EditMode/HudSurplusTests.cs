using CleanEnergy.Energy;
using CleanEnergy.Scenario;
using CleanEnergy.UI;
using NUnit.Framework;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class HudSurplusTests
    {
        [Test]
        public void ReadSurplusSold_UsesBalanceResult()
        {
            var result = new EnergyBalanceResult(10f, 5f, 0f, 4.5f, 0f);
            Assert.AreEqual(4.5f, EnergyHudUI.ReadSurplusSold(result), 0.001f);
            Assert.AreEqual(0f, EnergyHudUI.ReadSurplusSold(null), 0.001f);
        }

        [Test]
        public void ReadSatisfaction_UsesScenarioState()
        {
            var state = new ScenarioObjectiveState();
            state.Reset(72f);
            Assert.AreEqual(72f, EnergyHudUI.ReadSatisfaction(state), 0.001f);
            Assert.AreEqual(100f, EnergyHudUI.ReadSatisfaction(null), 0.001f);
        }
    }
}
