using System.Collections.Generic;
using CleanEnergy.CameraSystem;
using CleanEnergy.DebugTools;
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

        [Test]
        public void BoundsAroundCells_EncapsulatesAndPads()
        {
            var positions = new List<Vector3>
            {
                new Vector3(0f, 0f, 0f),
                new Vector3(10f, 0f, 0f)
            };
            var b = CameraFitMath.BoundsAroundCells(positions, 1f, 4);
            Assert.AreEqual(5f, b.center.x, 0.001f);
            Assert.AreEqual(18f, b.size.x, 0.001f);
            Assert.AreEqual(8f, b.size.z, 0.001f);
        }

        [Test]
        public void SelectionOrthoSize_ClampsMinMax()
        {
            var tiny = CameraFitMath.BoundsAroundCell(Vector3.zero, 1f, 0);
            Assert.AreEqual(8f, SelectionCameraFocus.ResolveSelectionOrthoSize(tiny, 1f, 8f, 80f), 0.001f);

            var wide = new Bounds(Vector3.zero, new Vector3(500f, 0f, 500f));
            Assert.AreEqual(80f, SelectionCameraFocus.ResolveSelectionOrthoSize(wide, 1f, 8f, 80f), 0.001f);
        }

        [Test]
        public void FitToBounds_SetsOrthoWithinClamp()
        {
            var go = new GameObject("ZoomFitCam");
            try
            {
                var cam = go.AddComponent<Camera>();
                cam.orthographic = true;
                cam.orthographicSize = 40f;
                var controller = go.AddComponent<IsometricCameraController>();
                var fit = CameraFitMath.BoundsAroundCell(new Vector3(10f, 0f, 10f), 1f, 4);
                controller.FitToBounds(fit, 0f);
                Assert.GreaterOrEqual(controller.OrthographicSize, controller.MinOrthographicSize);
                Assert.LessOrEqual(controller.OrthographicSize, controller.MaxOrthographicSize);
                Assert.AreEqual(
                    CameraFitMath.OrthographicSizeForBounds(
                        fit, cam.aspect, controller.MinOrthographicSize, controller.MaxOrthographicSize),
                    controller.OrthographicSize,
                    0.001f);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }
}
