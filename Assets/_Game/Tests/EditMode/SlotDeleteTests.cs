using CleanEnergy.Save;
using CleanEnergy.Simulation;
using CleanEnergy.UI;
using NUnit.Framework;
using UnityEngine;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class SlotDeleteTests
    {
        [Test]
        public void RequestDeleteSlotConfirm_ClampsAndSetsState()
        {
            var go = new GameObject("SlotDeleteConfirm");
            try
            {
                var clock = go.AddComponent<SimulationClock>();
                var overlay = go.AddComponent<PauseOverlayUI>();
                overlay.Configure(clock, null);
                overlay.Pause();

                overlay.RequestDeleteSlotConfirm(99);
                Assert.IsTrue(overlay.IsConfirmingDelete);
                Assert.AreEqual(SaveGameService.MaxSlot, overlay.ConfirmDeleteSlot);

                overlay.CancelDeleteConfirm();
                Assert.IsFalse(overlay.IsConfirmingDelete);
                Assert.AreEqual(0, overlay.ConfirmDeleteSlot);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void RequestDelete_ClearsMainMenuConfirm_AndViceVersa()
        {
            var go = new GameObject("SlotDeleteMutex");
            try
            {
                var overlay = go.AddComponent<PauseOverlayUI>();
                overlay.Configure(go.AddComponent<SimulationClock>(), null);
                overlay.Pause();

                overlay.RequestMainMenuConfirm();
                overlay.RequestDeleteSlotConfirm(2);
                Assert.IsFalse(overlay.IsConfirmingMainMenu);
                Assert.AreEqual(2, overlay.ConfirmDeleteSlot);

                overlay.RequestMainMenuConfirm();
                Assert.IsTrue(overlay.IsConfirmingMainMenu);
                Assert.IsFalse(overlay.IsConfirmingDelete);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void Resume_ClearsDeleteConfirm()
        {
            var go = new GameObject("SlotDeleteResume");
            try
            {
                var overlay = go.AddComponent<PauseOverlayUI>();
                overlay.Configure(go.AddComponent<SimulationClock>(), null);
                overlay.Pause();
                overlay.RequestDeleteSlotConfirm(1);
                overlay.Resume();
                Assert.IsFalse(overlay.IsConfirmingDelete);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void ConfirmDeleteSlotAction_WithoutController_ReturnsFalse()
        {
            var go = new GameObject("SlotDeleteNoSave");
            try
            {
                var overlay = go.AddComponent<PauseOverlayUI>();
                overlay.Configure(go.AddComponent<SimulationClock>(), null);
                overlay.RequestDeleteSlotConfirm(1);
                Assert.IsFalse(overlay.ConfirmDeleteSlotAction());
                Assert.IsFalse(overlay.IsConfirmingDelete);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }
}
