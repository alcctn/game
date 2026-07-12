using CleanEnergy.Scenario;
using NUnit.Framework;
using UnityEngine;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class ScenarioProgressTests
    {
        [Test]
        public void CoverageStreak_CompletesDemandObjective()
        {
            var service = CreateService(coverageTicks: 3);
            for (var i = 0; i < 3; i++)
            {
                service.Evaluate(GoodCoverage(types: 1, battery: false));
            }

            Assert.IsTrue(service.State.DemandObjectiveComplete);
            Assert.AreEqual(3, service.State.CoverageStreakTicks);
            Assert.IsFalse(service.State.HasWon);
        }

        [Test]
        public void CoverageBreak_ResetsStreak()
        {
            var service = CreateService(coverageTicks: 5);
            service.Evaluate(GoodCoverage(types: 1, battery: false));
            service.Evaluate(GoodCoverage(types: 1, battery: false));
            Assert.AreEqual(2, service.State.CoverageStreakTicks);

            service.Evaluate(new ScenarioTickInput(0.5f, demand: 10f, hasShortage: true, 1, false));
            Assert.AreEqual(0, service.State.CoverageStreakTicks);
            Assert.IsFalse(service.State.DemandObjectiveComplete);
        }

        [Test]
        public void SingleProducerType_DiversityIncomplete()
        {
            var service = CreateService(coverageTicks: 1, producerTypes: 2);
            service.Evaluate(GoodCoverage(types: 1, battery: true));

            Assert.IsFalse(service.State.DiversityObjectiveComplete);
            Assert.IsTrue(service.State.BatteryObjectiveComplete);
            Assert.IsFalse(service.State.HasWon);
        }

        [Test]
        public void TwoTypes_Battery_AndStreak_Wins()
        {
            var service = CreateService(coverageTicks: 2, producerTypes: 2);
            var won = false;
            service.Won += _ => won = true;

            service.Evaluate(GoodCoverage(types: 2, battery: true));
            Assert.IsFalse(service.State.HasWon);

            service.Evaluate(GoodCoverage(types: 2, battery: true));
            Assert.IsTrue(service.State.HasWon);
            Assert.IsTrue(won);
            Assert.IsTrue(service.State.AllObjectivesComplete);
        }

        [Test]
        public void ShortageStreak_LowersSatisfaction()
        {
            var service = CreateService(coverageTicks: 10, shortagePenalty: 5f);
            var start = service.State.Satisfaction;

            service.Evaluate(new ScenarioTickInput(0.4f, demand: 10f, hasShortage: true, 1, false));
            service.Evaluate(new ScenarioTickInput(0.4f, demand: 10f, hasShortage: true, 1, false));

            Assert.AreEqual(2, service.State.ShortageStreakTicks);
            Assert.AreEqual(start - 10f, service.State.Satisfaction, 0.001f);
        }

        [Test]
        public void LowSatisfaction_SetsRiskFlag()
        {
            var service = CreateService(
                coverageTicks: 10,
                shortagePenalty: 40f,
                riskThreshold: 30f,
                initialSatisfaction: 100f);

            service.Evaluate(new ScenarioTickInput(0.2f, demand: 10f, hasShortage: true, 1, false));
            service.Evaluate(new ScenarioTickInput(0.2f, demand: 10f, hasShortage: true, 1, false));

            Assert.IsTrue(service.State.IsAtRisk);
            Assert.LessOrEqual(service.State.Satisfaction, 30f);
        }

        [Test]
        public void CoverageRatio_FromBalanceResult()
        {
            var full = new CleanEnergy.Energy.EnergyBalanceResult(10f, 10f, 0f, 0f, 0f);
            Assert.AreEqual(1f, full.CoverageRatio, 0.001f);

            var partial = new CleanEnergy.Energy.EnergyBalanceResult(4f, 10f, 0f, 0f, 6f);
            Assert.AreEqual(0.4f, partial.CoverageRatio, 0.001f);
        }

        private static ScenarioProgressService CreateService(
            int coverageTicks = 60,
            int producerTypes = 2,
            float shortagePenalty = 2f,
            float riskThreshold = 30f,
            float initialSatisfaction = 100f)
        {
            var def = ScriptableObject.CreateInstance<ScenarioDefinition>();
            def.Configure(
                "test",
                "Test",
                0.95f,
                coverageTicks,
                producerTypes,
                initialSatisfaction,
                shortagePenalty,
                0.25f,
                riskThreshold);
            return new ScenarioProgressService(def);
        }

        private static ScenarioTickInput GoodCoverage(int types, bool battery)
        {
            return new ScenarioTickInput(0.96f, demand: 10f, hasShortage: false, types, battery);
        }
    }
}
