using System.IO;
using CleanEnergy.Save;
using NUnit.Framework;
using UnityEngine;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class SlotMetadataTests
    {
        [Test]
        public void TryReadSummary_MissingFile_ReturnsFalse()
        {
            var dir = Path.Combine(Application.temporaryCachePath, "ce_slot_meta_missing_" + Path.GetRandomFileName());
            Directory.CreateDirectory(dir);
            try
            {
                var service = new SaveGameService(dir);
                Assert.IsFalse(service.TryReadSummary(1, out var summary));
                Assert.IsNull(summary);
            }
            finally
            {
                if (Directory.Exists(dir))
                {
                    Directory.Delete(dir, true);
                }
            }
        }

        [Test]
        public void TryReadSummary_ReadsScenarioMoneyTick()
        {
            var dir = Path.Combine(Application.temporaryCachePath, "ce_slot_meta_" + Path.GetRandomFileName());
            Directory.CreateDirectory(dir);
            try
            {
                var service = new SaveGameService(dir);
                service.Write(2, new GameSaveData
                {
                    scenarioId = "sun_ridge",
                    money = 850f,
                    tickIndex = 42,
                    seed = "x"
                });

                Assert.IsTrue(service.TryReadSummary(2, out var summary));
                Assert.AreEqual("sun_ridge", summary.ScenarioId);
                Assert.AreEqual(850f, summary.Money, 0.001f);
                Assert.AreEqual(42, summary.TickIndex);
            }
            finally
            {
                if (Directory.Exists(dir))
                {
                    Directory.Delete(dir, true);
                }
            }
        }
    }
}
