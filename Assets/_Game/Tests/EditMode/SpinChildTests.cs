using CleanEnergy.Buildings;
using CleanEnergy.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class SpinChildTests
    {
        [Test]
        public void ResolveSpinTarget_UsesSpinChildWhenPresent()
        {
            var root = new GameObject("Root");
            var spin = new GameObject(RotatingVisual.SpinChildName);
            spin.transform.SetParent(root.transform, false);
            try
            {
                Assert.AreSame(spin.transform, RotatingVisual.ResolveSpinTarget(root.transform));
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void ResolveSpinTarget_FallsBackToRootWhenMissing()
        {
            var root = new GameObject("RootNoSpin");
            try
            {
                Assert.AreSame(root.transform, RotatingVisual.ResolveSpinTarget(root.transform));
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void Tick_RotatesSpinChild_NotParent()
        {
            var clockGo = new GameObject("ClockSpin");
            var root = new GameObject("WindRoot");
            var spin = new GameObject(RotatingVisual.SpinChildName);
            spin.transform.SetParent(root.transform, false);
            try
            {
                var clock = clockGo.AddComponent<SimulationClock>();
                clock.SetSpeed(SimulationSpeed.One);
                var rotating = root.AddComponent<RotatingVisual>();
                rotating.Configure(60f, clock);

                var rootBefore = root.transform.eulerAngles.y;
                var spinBefore = spin.transform.eulerAngles.y;

                rotating.Tick(0.5f);

                Assert.AreEqual(rootBefore, root.transform.eulerAngles.y, 0.01f);
                var spinDelta = Mathf.DeltaAngle(spinBefore, spin.transform.eulerAngles.y);
                Assert.AreEqual(180f, spinDelta, 0.5f);
            }
            finally
            {
                Object.DestroyImmediate(root);
                Object.DestroyImmediate(clockGo);
            }
        }

        [Test]
        public void Tick_WithoutSpin_RotatesRoot()
        {
            var clockGo = new GameObject("ClockRoot");
            var root = new GameObject("HydroRoot");
            try
            {
                var clock = clockGo.AddComponent<SimulationClock>();
                clock.SetSpeed(SimulationSpeed.One);
                var rotating = root.AddComponent<RotatingVisual>();
                rotating.Configure(60f, clock);
                var before = root.transform.eulerAngles.y;

                rotating.Tick(0.5f);

                var delta = Mathf.DeltaAngle(before, root.transform.eulerAngles.y);
                Assert.AreEqual(180f, delta, 0.5f);
            }
            finally
            {
                Object.DestroyImmediate(root);
                Object.DestroyImmediate(clockGo);
            }
        }
    }
}
