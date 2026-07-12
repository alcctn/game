using CleanEnergy.CameraSystem;
using NUnit.Framework;
using UnityEngine;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class ZoomFitTests
    {
        [Test]
        public void OrthographicSize_UsesLargerAxis()
        {
            var bounds = new Bounds(Vector3.zero, new Vector3(40f, 0f, 20f));
            var size = CameraFitMath.OrthographicSizeForBounds(bounds, 1f, 8f, 80f);
            Assert.AreEqual(20f, size, 0.001f);
        }

        [Test]
        public void OrthographicSize_ClampsToLimits()
        {
            var tiny = new Bounds(Vector3.zero, Vector3.one);
            Assert.AreEqual(8f, CameraFitMath.OrthographicSizeForBounds(tiny, 1f, 8f, 80f), 0.001f);

            var huge = new Bounds(Vector3.zero, new Vector3(400f, 0f, 400f));
            Assert.AreEqual(80f, CameraFitMath.OrthographicSizeForBounds(huge, 1f, 8f, 80f), 0.001f);
        }

        [Test]
        public void BoundsAroundCell_AppliesPadding()
        {
            var b = CameraFitMath.BoundsAroundCell(new Vector3(5f, 0f, 5f), 1f, 4);
            Assert.AreEqual(8f, b.size.x, 0.001f);
            Assert.AreEqual(8f, b.size.z, 0.001f);
        }
    }
}
