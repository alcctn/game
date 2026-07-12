using System.IO;
using CleanEnergy.Save;
using NUnit.Framework;
using UnityEngine;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class SaveGameTests
    {
        [Test]
        public void JsonRoundTrip_PreservesCoreFields()
        {
            var original = new GameSaveData
            {
                saveVersion = GameSaveData.CurrentVersion,
                seed = "valley-42",
                tickIndex = 17,
                money = 850.5f,
                researchPoints = 22f,
                emergencyCreditUsed = true,
                unlockedNodeIds = new[] { "hydro_basic", "solar_basic" },
                buildings = new[]
                {
                    new BuildingSaveData
                    {
                        definitionId = "village",
                        x = 3,
                        y = 4,
                        rotation = 1,
                        storedEnergy = 0f
                    },
                    new BuildingSaveData
                    {
                        definitionId = "battery",
                        x = 5,
                        y = 4,
                        rotation = 0,
                        storedEnergy = 12.5f
                    }
                },
                scenario = new ScenarioSaveData
                {
                    coverageStreakTicks = 9,
                    demandObjectiveComplete = false,
                    diversityObjectiveComplete = true,
                    batteryObjectiveComplete = true,
                    satisfaction = 88f,
                    hasWon = false
                }
            };

            var service = new SaveGameService(Path.Combine(Application.temporaryCachePath, "ce_save_tests"));
            var json = service.ToJson(original);
            var loaded = service.FromJson(json);

            Assert.IsNotNull(loaded);
            Assert.AreEqual(GameSaveData.CurrentVersion, loaded.saveVersion);
            Assert.AreEqual("valley-42", loaded.seed);
            Assert.AreEqual(17, loaded.tickIndex);
            Assert.AreEqual(850.5f, loaded.money, 0.001f);
            Assert.AreEqual(22f, loaded.researchPoints, 0.001f);
            Assert.IsTrue(loaded.emergencyCreditUsed);
            Assert.AreEqual(2, loaded.unlockedNodeIds.Length);
            Assert.AreEqual("solar_basic", loaded.unlockedNodeIds[1]);
            Assert.AreEqual(2, loaded.buildings.Length);
            Assert.AreEqual("battery", loaded.buildings[1].definitionId);
            Assert.AreEqual(12.5f, loaded.buildings[1].storedEnergy, 0.001f);
            Assert.AreEqual(9, loaded.scenario.coverageStreakTicks);
            Assert.IsTrue(loaded.scenario.diversityObjectiveComplete);
        }

        [Test]
        public void WriteAndRead_SlotFile()
        {
            var dir = Path.Combine(Application.temporaryCachePath, "ce_save_slot_" + System.Guid.NewGuid().ToString("N"));
            var service = new SaveGameService(dir);
            var data = new GameSaveData
            {
                seed = "slot-test",
                money = 100f,
                buildings = new[]
                {
                    new BuildingSaveData { definitionId = "water_wheel", x = 1, y = 2 }
                }
            };

            service.Write(data);
            Assert.IsTrue(service.SlotExists());
            var loaded = service.Read();
            Assert.IsNotNull(loaded);
            Assert.AreEqual("slot-test", loaded.seed);
            Assert.AreEqual(1, loaded.buildings.Length);
            Assert.AreEqual("water_wheel", loaded.buildings[0].definitionId);

            if (Directory.Exists(dir))
            {
                Directory.Delete(dir, true);
            }
        }

        [Test]
        public void ToJson_SetsCurrentSaveVersion()
        {
            var data = new GameSaveData { saveVersion = 0, seed = "x" };
            var service = new SaveGameService();
            var loaded = service.FromJson(service.ToJson(data));
            Assert.AreEqual(GameSaveData.CurrentVersion, loaded.saveVersion);
        }
    }
}
