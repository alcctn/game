using CleanEnergy.Simulation;
using CleanEnergy.UI;
using NUnit.Framework;
using UnityEngine;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class PauseOverlayTests
    {
        [Test]
        public void Pause_SetsClockPaused()
        {
            var go = new GameObject("Pause");
            try
            {
                var clock = go.AddComponent<SimulationClock>();
                clock.SetSpeed(SimulationSpeed.Two);
                var overlay = go.AddComponent<PauseOverlayUI>();
                overlay.Configure(clock, null);

                overlay.Pause();
                Assert.IsTrue(overlay.IsOverlayVisible);
                Assert.AreEqual(SimulationSpeed.Paused, clock.Speed);

                overlay.Resume();
                Assert.IsFalse(overlay.IsOverlayVisible);
                Assert.AreEqual(SimulationSpeed.Two, clock.Speed);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }
}
