using CleanEnergy.Energy;
using CleanEnergy.Research;
using CleanEnergy.UI;
using NUnit.Framework;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class RpUxTests
    {
        [Test]
        public void CoverageTick_SurfacesLastCoverageRpGranted()
        {
            var service = CreateService();
            var tracker = new ResearchProgressTracker(service);
            var balance = new EnergyBalanceResult(10f, 10f, 0f, 0f, 0f);

            tracker.OnBalanceTick(balance, activeProducerTypeCount: 1);

            Assert.AreEqual(ResearchProgressTracker.CoverageRpPerTick, tracker.LastCoverageRpGranted, 0.001f);
            Assert.AreEqual(
                $"RP 1 (+{ResearchProgressTracker.CoverageRpPerTick:F0}/tick)",
                EnergyHudUI.FormatRpLabel(service.Wallet.Points, tracker.LastCoverageRpGranted));
        }

        [Test]
        public void BelowCoverage_NoRpIncomeLabel()
        {
            var service = CreateService();
            var tracker = new ResearchProgressTracker(service);
            var balance = new EnergyBalanceResult(5f, 10f, 0f, 0f, 5f);

            tracker.OnBalanceTick(balance, 1);

            Assert.AreEqual(0f, tracker.LastCoverageRpGranted, 0.001f);
            Assert.AreEqual("RP 0", EnergyHudUI.FormatRpLabel(0f, tracker.LastCoverageRpGranted));
        }

        [Test]
        public void DiversityBonus_RaisesEventOnce_AndFormatsToast()
        {
            var service = CreateService();
            var tracker = new ResearchProgressTracker(service);
            var raised = 0;
            tracker.DiversityBonusGranted += () => raised++;
            var balance = new EnergyBalanceResult(10f, 10f, 0f, 0f, 0f);

            tracker.OnBalanceTick(balance, 2);
            Assert.AreEqual(1, raised);
            Assert.IsTrue(tracker.LastDiversityBonusGranted);

            tracker.OnBalanceTick(balance, 2);
            Assert.AreEqual(1, raised);
            Assert.IsFalse(tracker.LastDiversityBonusGranted);
            Assert.AreEqual(
                $"Diversity +{ResearchProgressTracker.DiversityBonusRp:F0} RP",
                NotificationController.FormatDiversityBonusToast());
        }

        [Test]
        public void DiversityBonus_FormulasUnchanged()
        {
            var service = CreateService();
            var tracker = new ResearchProgressTracker(service);
            var balance = new EnergyBalanceResult(10f, 10f, 0f, 0f, 0f);

            tracker.OnBalanceTick(balance, 2);
            // coverage 1 + diversity 10
            Assert.AreEqual(11f, service.Wallet.Points, 0.001f);
            Assert.IsTrue(tracker.LastDiversityBonusGranted);
        }

        private static ResearchService CreateService()
        {
            return new ResearchService(ResearchService.CreateRuntimeDefaultTree());
        }
    }
}
