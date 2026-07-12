using CleanEnergy.Save;
using CleanEnergy.Scenario;
using NUnit.Framework;
using UnityEngine;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class ScenarioResearchWinTests
    {
        [Test]
        public void WithoutResearch_CannotWin()
        {
            var service = CreateService(coverageTicks: 1, producerTypes: 2);
            service.Evaluate(Tick(types: 2, battery: true, researchMet: false));
            Assert.IsTrue(service.State.DemandObjectiveComplete);
            Assert.IsTrue(service.State.DiversityObjectiveComplete);
            Assert.IsTrue(service.State.BatteryObjectiveComplete);
            Assert.IsFalse(service.State.ResearchObjectiveComplete);
            Assert.IsFalse(service.State.HasWon);
        }

        [Test]
        public void UnlockRequiredNode_SetsResearchObjective()
        {
            var service = CreateService(coverageTicks: 10, producerTypes: 2);
            service.Evaluate(Tick(types: 1, battery: false, researchMet: true));
            Assert.IsTrue(service.State.ResearchObjectiveComplete);
            Assert.IsFalse(service.State.HasWon);
        }

        [Test]
        public void FullPath_WithResearch_Wins()
        {
            var service = CreateService(coverageTicks: 1, producerTypes: 2);
            var won = false;
            service.Won += _ => won = true;

            service.Evaluate(Tick(types: 2, battery: true, researchMet: true));
            Assert.IsTrue(service.State.HasWon);
            Assert.IsTrue(won);
            Assert.IsTrue(service.State.ResearchObjectiveComplete);
        }

        [Test]
        public void EmptyRequiredList_ResearchAutoComplete()
        {
            var def = ScriptableObject.CreateInstance<ScenarioDefinition>();
            def.Configure(
                "test", "Test", 0.95f, 1, 2, 100f, 2f, 0.25f, 30f,
                researchNodeIds: System.Array.Empty<string>());
            var service = new ScenarioProgressService(def);
            service.Evaluate(Tick(types: 2, battery: true, researchMet: true));
            Assert.IsTrue(service.State.ResearchObjectiveComplete);
            Assert.IsTrue(service.State.HasWon);
        }

        [Test]
        public void SaveData_RoundTripsResearchObjective()
        {
            var original = new GameSaveData
            {
                scenario = new ScenarioSaveData
                {
                    researchObjectiveComplete = true,
                    hasWon = false
                }
            };
            var service = new SaveGameService();
            var loaded = service.FromJson(service.ToJson(original));
            Assert.IsTrue(loaded.scenario.researchObjectiveComplete);
        }

        private static ScenarioProgressService CreateService(
            int coverageTicks = 60,
            int producerTypes = 2)
        {
            var def = ScriptableObject.CreateInstance<ScenarioDefinition>();
            def.Configure(
                "test",
                "Test",
                0.95f,
                coverageTicks,
                producerTypes,
                100f,
                2f,
                0.25f,
                30f,
                researchNodeIds: new[] { "solar_basic" });
            return new ScenarioProgressService(def);
        }

        private static ScenarioTickInput Tick(int types, bool battery, bool researchMet)
        {
            return new ScenarioTickInput(0.96f, demand: 10f, hasShortage: false, types, battery, researchMet);
        }
    }
}
