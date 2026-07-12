using CleanEnergy.Save;
using CleanEnergy.Simulation;
using CleanEnergy.UI;
using NUnit.Framework;
using UnityEngine;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class SavePolishTests
    {
        [Test]
        public void FormatSaveSlotLabel_IncludesSummaryMetadata()
        {
            var summary = new SlotSaveSummary
            {
                ScenarioId = "green_valley",
                Money = 420f,
                TickIndex = 12
            };

            Assert.AreEqual(
                "Save Slot 2 * — green_valley $420 t12",
                PauseOverlayUI.FormatSaveSlotLabel(2, 2, summary, true));
            Assert.AreEqual(
                "Save Slot 1 — empty",
                PauseOverlayUI.FormatSaveSlotLabel(1, 2, null, false));
        }

        [Test]
        public void RequestOverwriteConfirm_ExclusiveWithDeleteAndMainMenu()
        {
            var go = new GameObject("SavePolishOverwrite");
            try
            {
                var overlay = go.AddComponent<PauseOverlayUI>();
                overlay.Configure(go.AddComponent<SimulationClock>(), null);
                overlay.Pause();

                overlay.RequestDeleteSlotConfirm(1);
                Assert.IsTrue(overlay.IsConfirmingDelete);

                overlay.RequestOverwriteConfirm(2);
                Assert.IsTrue(overlay.IsConfirmingOverwrite);
                Assert.AreEqual(2, overlay.ConfirmOverwriteSlot);
                Assert.IsFalse(overlay.IsConfirmingDelete);
                Assert.IsFalse(overlay.IsConfirmingMainMenu);

                overlay.RequestMainMenuConfirm();
                Assert.IsTrue(overlay.IsConfirmingMainMenu);
                Assert.IsFalse(overlay.IsConfirmingOverwrite);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void CancelOverwriteConfirm_ClearsState()
        {
            var go = new GameObject("SavePolishCancel");
            try
            {
                var overlay = go.AddComponent<PauseOverlayUI>();
                overlay.Configure(go.AddComponent<SimulationClock>(), null);
                overlay.Pause();
                overlay.RequestOverwriteConfirm(3);
                overlay.CancelOverwriteConfirm();
                Assert.IsFalse(overlay.IsConfirmingOverwrite);
                Assert.AreEqual(0, overlay.ConfirmOverwriteSlot);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void Resume_ClearsOverwriteConfirm()
        {
            var go = new GameObject("SavePolishResume");
            try
            {
                var overlay = go.AddComponent<PauseOverlayUI>();
                overlay.Configure(go.AddComponent<SimulationClock>(), null);
                overlay.Pause();
                overlay.RequestOverwriteConfirm(1);
                overlay.Resume();
                Assert.IsFalse(overlay.IsConfirmingOverwrite);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }
}
