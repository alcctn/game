using CleanEnergy.Save;
using CleanEnergy.Scenario;
using NUnit.Framework;
using UnityEngine;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class ScenarioFailTests
    {
        [Test]
        public void SatisfactionToZero_SetsHasLost()
        {
            var service = CreateService(initialSatisfaction: 10f, shortagePenalty: 10f);
            var failed = false;
            service.Failed += _ => failed = true;

            service.Evaluate(ShortageTick());

            Assert.IsTrue(service.State.HasLost);
            Assert.AreEqual(0f, service.State.Satisfaction, 0.001f);
            Assert.IsTrue(failed);
        }

        [Test]
        public void AfterLost_EvaluateIsNoOp()
        {
            var service = CreateService(initialSatisfaction: 5f, shortagePenalty: 5f);
            service.Evaluate(ShortageTick());
            Assert.IsTrue(service.State.HasLost);

            var sat = service.State.Satisfaction;
            service.Evaluate(GoodCoverage());
            Assert.AreEqual(sat, service.State.Satisfaction, 0.001f);
            Assert.IsFalse(service.State.HasWon);
        }

        [Test]
        public void AfterWon_DoesNotFail()
        {
            var service = CreateService(coverageTicks: 1, producerTypes: 2, initialSatisfaction: 5f, shortagePenalty: 5f);
            service.Evaluate(GoodCoverage(types: 2, battery: true));
            Assert.IsTrue(service.State.HasWon);

            var failed = false;
            service.Failed += _ => failed = true;
            service.Evaluate(ShortageTick());
            Assert.IsFalse(service.State.HasLost);
            Assert.IsFalse(failed);
        }

        [Test]
        public void SaveData_RoundTripsHasLost()
        {
            var original = new GameSaveData
            {
                scenario = new ScenarioSaveData
                {
                    satisfaction = 0f,
                    hasLost = true,
                    hasWon = false
                }
            };
            var service = new SaveGameService();
            var loaded = service.FromJson(service.ToJson(original));
            Assert.IsTrue(loaded.scenario.hasLost);
            Assert.IsFalse(loaded.scenario.hasWon);
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

        private static ScenarioTickInput ShortageTick()
        {
            return new ScenarioTickInput(0.2f, demand: 10f, hasShortage: true, 1, false);
        }

        private static ScenarioTickInput GoodCoverage(int types = 2, bool battery = true)
        {
            return new ScenarioTickInput(0.96f, demand: 10f, hasShortage: false, types, battery);
        }
    }
}
