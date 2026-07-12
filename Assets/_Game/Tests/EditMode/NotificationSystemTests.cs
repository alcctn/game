using CleanEnergy.UI;
using NUnit.Framework;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class NotificationSystemTests
    {
        [Test]
        public void Push_CapsAtMaxCount()
        {
            var service = new NotificationService();
            for (var i = 0; i < 7; i++)
            {
                service.Push($"msg-{i}", now: 0f);
            }

            Assert.AreEqual(6, service.Active.Count);
            Assert.AreEqual("msg-1", service.Active[0].Message);
            Assert.AreEqual("msg-6", service.Active[5].Message);
        }

        [Test]
        public void Prune_ExpiresOldMessages()
        {
            var service = new NotificationService();
            service.Push("old", now: 0f, lifetimeSeconds: 5f);
            service.Push("fresh", now: 4f, lifetimeSeconds: 5f);

            service.Prune(now: 5.1f);

            Assert.AreEqual(1, service.Active.Count);
            Assert.AreEqual("fresh", service.Active[0].Message);
        }

        [Test]
        public void BatteryFull_CooldownPreventsSpam()
        {
            var service = new NotificationService();
            Assert.IsTrue(service.TryPushBatteryFull(now: 10f));
            Assert.IsFalse(service.TryPushBatteryFull(now: 11f));
            Assert.IsFalse(service.TryPushBatteryFull(now: 12.9f));
            Assert.IsTrue(service.TryPushBatteryFull(now: 13.1f));
            Assert.AreEqual(2, service.Active.Count);
        }

        [Test]
        public void EnergyBalanceResult_TracksEnergyCharged()
        {
            var result = new CleanEnergy.Energy.EnergyBalanceResult(10f, 2f, 5f, 3f, 0f, energyCharged: 5f);
            Assert.AreEqual(5f, result.EnergyCharged, 0.001f);
        }
    }
}
