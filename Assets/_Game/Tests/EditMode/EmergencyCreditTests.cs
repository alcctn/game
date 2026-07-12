using CleanEnergy.Economy;
using CleanEnergy.Save;
using NUnit.Framework;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class EmergencyCreditTests
    {
        [Test]
        public void TryGrant_WhenBroke_AddsCreditOnce()
        {
            var service = new EmergencyCreditService();
            var wallet = new Wallet(0f);

            Assert.IsTrue(service.TryGrant(wallet));
            Assert.AreEqual(EmergencyCreditService.CreditAmount, wallet.Money, 0.001f);
            Assert.IsTrue(service.HasBeenUsed);

            Assert.IsFalse(service.TryGrant(wallet));
            Assert.AreEqual(EmergencyCreditService.CreditAmount, wallet.Money, 0.001f);
        }

        [Test]
        public void TryGrant_WhenHasMoney_DoesNothing()
        {
            var service = new EmergencyCreditService();
            var wallet = new Wallet(50f);

            Assert.IsFalse(service.TryGrant(wallet));
            Assert.AreEqual(50f, wallet.Money, 0.001f);
            Assert.IsFalse(service.HasBeenUsed);
        }

        [Test]
        public void Reset_AllowsGrantAgain()
        {
            var service = new EmergencyCreditService();
            var wallet = new Wallet(0f);
            Assert.IsTrue(service.TryGrant(wallet));
            service.Reset();
            wallet.SetMoney(0f);
            Assert.IsTrue(service.TryGrant(wallet));
            Assert.AreEqual(EmergencyCreditService.CreditAmount, wallet.Money, 0.001f);
        }

        [Test]
        public void SaveData_RoundTripsEmergencyCreditUsed()
        {
            var original = new GameSaveData
            {
                emergencyCreditUsed = true,
                money = 200f
            };
            var service = new SaveGameService();
            var loaded = service.FromJson(service.ToJson(original));
            Assert.IsTrue(loaded.emergencyCreditUsed);
            Assert.AreEqual(200f, loaded.money, 0.001f);
        }
    }
}
