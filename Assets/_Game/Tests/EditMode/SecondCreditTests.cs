using CleanEnergy.Economy;
using CleanEnergy.Save;
using NUnit.Framework;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class SecondCreditTests
    {
        [Test]
        public void AfterRepay_AllowsSecondGrantOf150()
        {
            var service = new EmergencyCreditService();
            var wallet = new Wallet(0f);

            Assert.IsTrue(service.TryGrant(wallet));
            Assert.AreEqual(EmergencyCreditService.CreditAmount, wallet.Money, 0.001f);
            Assert.AreEqual(1, service.CreditUses);
            Assert.AreEqual(EmergencyCreditService.CreditAmount, service.RemainingDebt, 0.001f);

            Assert.IsTrue(service.TryRepay(wallet));
            Assert.AreEqual(0f, service.RemainingDebt, 0.001f);
            Assert.AreEqual(1, service.CreditUses);

            wallet.SetMoney(0f);
            Assert.IsTrue(service.TryGrant(wallet));
            Assert.AreEqual(EmergencyCreditService.SecondCreditAmount, wallet.Money, 0.001f);
            Assert.AreEqual(2, service.CreditUses);
            Assert.AreEqual(EmergencyCreditService.SecondCreditAmount, service.RemainingDebt, 0.001f);
        }

        [Test]
        public void SecondGrant_InterestUsesSameRuleOnNewPrincipal()
        {
            var service = new EmergencyCreditService();
            var wallet = new Wallet(0f);
            service.TryGrant(wallet);
            service.TryRepay(wallet);
            wallet.SetMoney(0f);
            service.TryGrant(wallet);

            Assert.AreEqual(2f, EmergencyCreditService.CalculateInterest(150f), 0.001f);
            wallet.Add(10f);
            var interest = service.ProcessInterestTick(wallet);
            Assert.AreEqual(2f, interest, 0.001f);
            Assert.AreEqual(EmergencyCreditService.SecondCreditAmount, service.RemainingDebt, 0.001f);
        }

        [Test]
        public void NoThirdLoan_AfterTwoUses()
        {
            var service = new EmergencyCreditService();
            var wallet = new Wallet(0f);
            Assert.IsTrue(service.TryGrant(wallet));
            Assert.IsTrue(service.TryRepay(wallet));
            wallet.SetMoney(0f);
            Assert.IsTrue(service.TryGrant(wallet));
            Assert.IsTrue(service.TryRepay(wallet));
            wallet.SetMoney(0f);
            Assert.IsFalse(service.TryGrant(wallet));
            Assert.AreEqual(2, service.CreditUses);
            Assert.AreEqual(0f, wallet.Money, 0.001f);
        }

        [Test]
        public void CannotGrantWhileDebtRemains()
        {
            var service = new EmergencyCreditService();
            var wallet = new Wallet(0f);
            Assert.IsTrue(service.TryGrant(wallet));
            wallet.SetMoney(0f);
            Assert.IsFalse(service.TryGrant(wallet));
            Assert.AreEqual(1, service.CreditUses);
        }

        [Test]
        public void SaveData_RoundTripsCreditUses()
        {
            var original = new GameSaveData
            {
                emergencyCreditUsed = true,
                creditUses = 2,
                creditDebt = 150f,
                money = 80f
            };
            var save = new SaveGameService();
            var loaded = save.FromJson(save.ToJson(original));
            Assert.AreEqual(2, loaded.creditUses);
            Assert.AreEqual(150f, loaded.creditDebt, 0.001f);
            Assert.IsTrue(loaded.emergencyCreditUsed);
        }

        [Test]
        public void Restore_LegacyUsedFlag_MapsToOneUse()
        {
            var service = new EmergencyCreditService();
            service.Restore(true, 200f);
            Assert.AreEqual(1, service.CreditUses);
            Assert.AreEqual(200f, service.RemainingDebt, 0.001f);
        }
    }
}
