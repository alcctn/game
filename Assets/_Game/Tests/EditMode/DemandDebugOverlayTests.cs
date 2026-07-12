using CleanEnergy.DebugTools;
using CleanEnergy.Energy;
using NUnit.Framework;
using UnityEngine;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class DemandDebugOverlayTests
    {
        [Test]
        public void ComputeRatio_UsesBaseDemand()
        {
            Assert.AreEqual(0f, DemandUtilization.ComputeRatio(5f, 0f), 0.001f);
            Assert.AreEqual(1f, DemandUtilization.ComputeRatio(10f, 10f), 0.001f);
            Assert.AreEqual(1f, DemandUtilization.ComputeRatio(20f, 10f), 0.001f);
            Assert.AreEqual(0.5f, DemandUtilization.ComputeRatio(5f, 10f), 0.001f);
        }

        [Test]
        public void ColorForRatio_HighIsMoreOrange()
        {
            var low = DemandUtilization.ColorForRatio(0.1f);
            var high = DemandUtilization.ColorForRatio(0.9f);
            Assert.Greater(high.r, low.r);
        }

        [Test]
        public void F9_MapsToDemand()
        {
            Assert.IsTrue(DebugViewHotkeys.TryMapKey(KeyCode.F9, out var mode));
            Assert.AreEqual(DebugViewMode.Demand, mode);
        }
    }
}
