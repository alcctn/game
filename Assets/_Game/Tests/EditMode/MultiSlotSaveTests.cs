using System.IO;
using CleanEnergy.Save;
using NUnit.Framework;
using UnityEngine;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class MultiSlotSaveTests
    {
        [Test]
        public void SlotFileNames_CoverOneToThree()
        {
            Assert.AreEqual("slot1.json", SaveGameService.SlotFileName(1));
            Assert.AreEqual("slot2.json", SaveGameService.SlotFileName(2));
            Assert.AreEqual("slot3.json", SaveGameService.SlotFileName(3));
            Assert.AreEqual("slot1.json", SaveGameService.SlotFileName(0));
            Assert.AreEqual("slot3.json", SaveGameService.SlotFileName(9));
        }

        [Test]
        public void WriteRead_UsesSelectedSlot()
        {
            var dir = Path.Combine(Application.temporaryCachePath, "ce_multi_slot_test");
            if (Directory.Exists(dir))
            {
                Directory.Delete(dir, true);
            }

            var service = new SaveGameService(dir);
            var data = new GameSaveData { seed = "abc", money = 42f };
            service.Write(2, data);
            Assert.IsFalse(service.SlotExists(1));
            Assert.IsTrue(service.SlotExists(2));
            var loaded = service.Read(2);
            Assert.AreEqual("abc", loaded.seed);
            Assert.AreEqual(42f, loaded.money, 0.001f);
            Directory.Delete(dir, true);
        }
    }
}
