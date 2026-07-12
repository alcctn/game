using CleanEnergy.Energy;
using NUnit.Framework;
using UnityEngine;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class NetworkDebugOverlayTests
    {
        [Test]
        public void Utilization_ZeroCapacity_IsZero()
        {
            Assert.AreEqual(0f, NetworkUtilization.Compute(40f, 0f), 0.001f);
        }

        [Test]
        public void Utilization_HalfCapacity()
        {
            Assert.AreEqual(0.5f, NetworkUtilization.Compute(20f, 40f), 0.001f);
        }

        [Test]
        public void Utilization_ClampsAboveOne()
        {
            Assert.AreEqual(1f, NetworkUtilization.Compute(100f, 40f), 0.001f);
        }

        [Test]
        public void Color_HighUtil_IsRedder()
        {
            var low = NetworkUtilization.ColorForUtilization(0.1f);
            var high = NetworkUtilization.ColorForUtilization(0.9f);
            Assert.Greater(high.r, low.r);
            Assert.Less(high.g, low.g);
        }
    }
}
