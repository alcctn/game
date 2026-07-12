using System.IO;
using CleanEnergy.Save;
using CleanEnergy.Scenario;
using NUnit.Framework;
using UnityEngine;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class MenuSlotDeleteTests
    {
        [Test]
        public void DeleteSlot_RemovesOnlyTarget()
        {
            var dir = Path.Combine(Application.temporaryCachePath, "ce_menu_slot_" + System.Guid.NewGuid().ToString("N"));
            var service = new SaveGameService(dir);
            service.Write(1, new GameSaveData { seed = "a", money = 1f });
            service.Write(2, new GameSaveData { seed = "b", money = 2f });

            Assert.IsTrue(service.DeleteSlot(1));
            Assert.IsFalse(service.SlotExists(1));
            Assert.IsTrue(service.SlotExists(2));
            Directory.Delete(dir, true);
        }

        [Test]
        public void ContinueSlot_IsClamped()
        {
            ScenarioSession.ContinueSlot = 9;
            Assert.AreEqual(3, ScenarioSession.ResolveContinueSlot());
            ScenarioSession.ContinueSlot = 0;
            Assert.AreEqual(1, ScenarioSession.ResolveContinueSlot());
            ScenarioSession.ContinueSlot = 2;
            Assert.AreEqual(2, ScenarioSession.ResolveContinueSlot());
            ScenarioSession.ContinueSlot = 1;
        }
    }
}
