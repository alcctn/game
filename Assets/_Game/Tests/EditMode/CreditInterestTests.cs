using CleanEnergy.Economy;
using CleanEnergy.Save;
using NUnit.Framework;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class CreditInterestTests
    {
        [Test]
        public void TryGrant_OpensDebtOfCreditAmount()
        {
            var service = new EmergencyCreditService();
            var wallet = new Wallet(0f);
            Assert.IsTrue(service.TryGrant(wallet));
            Assert.AreEqual(EmergencyCreditService.CreditAmount, service.RemainingDebt, 0.001f);
        }

        [Test]
        public void Interest_CeilOnePercent_MinOne_DoesNotChangePrincipal()
        {
            Assert.AreEqual(2f, EmergencyCreditService.CalculateInterest(200f), 0.001f);
            Assert.AreEqual(1f, EmergencyCreditService.CalculateInterest(50f), 0.001f);

            var service = new EmergencyCreditService();
            var wallet = new Wallet(0f);
            service.TryGrant(wallet);
            wallet.Add(100f);
            var before = service.RemainingDebt;
            var interest = service.ProcessInterestTick(wallet);
            Assert.AreEqual(2f, interest, 0.001f);
            Assert.AreEqual(before, service.RemainingDebt, 0.001f);
            Assert.AreEqual(EmergencyCreditService.CreditAmount + 100f - 2f, wallet.Money, 0.001f);
        }

        [Test]
        public void TryRepay_ClearsDebt()
        {
            var service = new EmergencyCreditService();
            var wallet = new Wallet(0f);
            service.TryGrant(wallet);
            Assert.IsTrue(service.TryRepay(wallet));
            Assert.AreEqual(0f, service.RemainingDebt, 0.001f);
            Assert.AreEqual(0f, wallet.Money, 0.001f);
        }

        [Test]
        public void SaveData_RoundTripsCreditDebt()
        {
            var original = new GameSaveData
            {
                emergencyCreditUsed = true,
                creditDebt = 200f,
                money = 150f
            };
            var service = new SaveGameService();
            var loaded = service.FromJson(service.ToJson(original));
            Assert.IsTrue(loaded.emergencyCreditUsed);
            Assert.AreEqual(200f, loaded.creditDebt, 0.001f);
        }
    }
}
