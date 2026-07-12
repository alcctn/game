using CleanEnergy.Save;
using CleanEnergy.Simulation;
using CleanEnergy.UI;
using NUnit.Framework;
using UnityEngine;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class PauseSaveUxTests
    {
        [Test]
        public void FormatSaveSlotLabel_MarksActiveSlot()
        {
            Assert.AreEqual("Save Slot 2 *", PauseOverlayUI.FormatSaveSlotLabel(2, 2));
            Assert.AreEqual("Save Slot 1", PauseOverlayUI.FormatSaveSlotLabel(1, 2));
            Assert.AreEqual("Save Slot 3 *", PauseOverlayUI.FormatSaveSlotLabel(99, 3));
        }

        [Test]
        public void RequestMainMenuConfirm_SetsConfirmState()
        {
            var go = new GameObject("PauseConfirm");
            try
            {
                var clock = go.AddComponent<SimulationClock>();
                clock.SetSpeed(SimulationSpeed.One);
                var overlay = go.AddComponent<PauseOverlayUI>();
                overlay.Configure(clock, null);

                overlay.Pause();
                Assert.IsFalse(overlay.IsConfirmingMainMenu);

                overlay.RequestMainMenuConfirm();
                Assert.IsTrue(overlay.IsConfirmingMainMenu);

                overlay.CancelMainMenuConfirm();
                Assert.IsFalse(overlay.IsConfirmingMainMenu);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void Resume_ClearsConfirmState()
        {
            var go = new GameObject("PauseResumeConfirm");
            try
            {
                var clock = go.AddComponent<SimulationClock>();
                var overlay = go.AddComponent<PauseOverlayUI>();
                overlay.Configure(clock, null);
                overlay.Pause();
                overlay.RequestMainMenuConfirm();
                overlay.Resume();
                Assert.IsFalse(overlay.IsOverlayVisible);
                Assert.IsFalse(overlay.IsConfirmingMainMenu);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void ClampSlot_MatchesSaveServiceBounds()
        {
            Assert.AreEqual(1, SaveGameService.ClampSlot(0));
            Assert.AreEqual(3, SaveGameService.ClampSlot(9));
        }
    }
}
