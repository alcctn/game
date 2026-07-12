using CleanEnergy.Settlements;
using NUnit.Framework;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class VillagePopulationTests
    {
        [Test]
        public void DemandScale_TracksPopulationRatio()
        {
            var state = new SettlementState();
            state.Reset(100f);
            Assert.AreEqual(1f, state.DemandScale, 0.001f);
            state.Restore(120f, 100f);
            Assert.AreEqual(1.2f, state.DemandScale, 0.001f);
        }

        [Test]
        public void Tick_GrowsWhenCoverageHigh()
        {
            var state = new SettlementState();
            state.Reset(100f);
            state.Tick(0.95f);
            Assert.AreEqual(100.02f, state.Population, 0.0001f);
        }

        [Test]
        public void Tick_ShrinksWhenCoverageLow()
        {
            var state = new SettlementState();
            state.Reset(100f);
            state.Tick(0.4f);
            Assert.AreEqual(99.99f, state.Population, 0.0001f);
        }

        [Test]
        public void Tick_RespectsCapAndFloor()
        {
            var state = new SettlementState();
            state.Restore(150f, 100f);
            state.Tick(1f);
            Assert.AreEqual(150f, state.Population, 0.001f);

            state.Restore(80f, 100f);
            state.Tick(0f);
            Assert.AreEqual(80f, state.Population, 0.001f);
        }
    }
}
