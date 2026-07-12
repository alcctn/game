using CleanEnergy.Research;
using NUnit.Framework;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class StorageResearchTests
    {
        [Test]
        public void Battery_StartsLocked()
        {
            var service = new ResearchService(ResearchService.CreateRuntimeDefaultTree());
            Assert.IsFalse(service.IsBuildingUnlocked("battery"));
            Assert.IsFalse(service.IsNodeUnlocked("storage_basic"));
        }

        [Test]
        public void StorageBasic_UnlocksBattery()
        {
            var service = new ResearchService(ResearchService.CreateRuntimeDefaultTree());
            service.Wallet.Add(20f);
            Assert.IsTrue(service.TryUnlock("storage_basic"));
            Assert.IsTrue(service.IsBuildingUnlocked("battery"));
        }

        [Test]
        public void BatteryCap_AddsCapacityBonus()
        {
            var service = new ResearchService(ResearchService.CreateRuntimeDefaultTree());
            service.Wallet.Add(50f);
            Assert.IsTrue(service.TryUnlock("storage_basic"));
            Assert.IsTrue(service.TryUnlock("battery_cap"));
            Assert.AreEqual(0.25f, service.GetStorageCapacityBonus("battery"), 0.001f);
        }
    }
}
