using CleanEnergy.CameraSystem;
using NUnit.Framework;
using UnityEngine;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class CameraFocusTests
    {
        [Test]
        public void SmoothStep_EndsAtOne()
        {
            Assert.AreEqual(0f, CameraFocusMath.SmoothStep01(0f), 0.0001f);
            Assert.AreEqual(1f, CameraFocusMath.SmoothStep01(1f), 0.0001f);
            Assert.Greater(CameraFocusMath.SmoothStep01(0.5f), 0.4f);
            Assert.Less(CameraFocusMath.SmoothStep01(0.5f), 0.6f);
        }

        [Test]
        public void LerpFocus_MidpointBetween()
        {
            var from = new Vector3(0f, 0f, 0f);
            var to = new Vector3(10f, 0f, 0f);
            var mid = CameraFocusMath.LerpFocus(from, to, 0.5f);
            Assert.AreEqual(5f, mid.x, 0.001f);
        }
    }
}
