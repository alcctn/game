using CleanEnergy.DebugTools;
using CleanEnergy.Energy;
using NUnit.Framework;
using UnityEngine;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class ProductionDebugOverlayTests
    {
        [Test]
        public void ComputeRatio_ClampsToInstalledPower()
        {
            Assert.AreEqual(0f, ProductionUtilization.ComputeRatio(10f, 0f), 0.001f);
            Assert.AreEqual(0.5f, ProductionUtilization.ComputeRatio(25f, 50f), 0.001f);
            Assert.AreEqual(1f, ProductionUtilization.ComputeRatio(80f, 50f), 0.001f);
        }

        [Test]
        public void ColorForRatio_HighIsGreenerThanLow()
        {
            var low = ProductionUtilization.ColorForRatio(0.1f);
            var high = ProductionUtilization.ColorForRatio(0.9f);
            Assert.Greater(high.g, low.g);
        }

        [Test]
        public void F8_MapsToProduction()
        {
            Assert.IsTrue(DebugViewHotkeys.TryMapKey(KeyCode.F8, out var mode));
            Assert.AreEqual(DebugViewMode.Production, mode);
        }
    }
}
