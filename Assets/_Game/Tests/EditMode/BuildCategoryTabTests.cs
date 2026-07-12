using System.Collections.Generic;
using CleanEnergy.Buildings;
using CleanEnergy.Placement;
using CleanEnergy.UI;
using NUnit.Framework;
using UnityEngine;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class BuildCategoryTabTests
    {
        [TearDown]
        public void TearDown()
        {
            BuildingPlacementUI.ClearTabPrefs();
        }

        [Test]
        public void PrefsKey_IsLockedName()
        {
            Assert.AreEqual("ce_build_tab", BuildingPlacementUI.BuildTabPrefsKey);
        }

        [Test]
        public void SaveLoad_PersistsActiveCategory()
        {
            BuildingPlacementUI.ClearTabPrefs();
            BuildingPlacementUI.SaveActiveCategory(BuildingCategory.Network);

            Assert.AreEqual(BuildingCategory.Network, BuildingPlacementUI.LoadActiveCategory());
            Assert.AreEqual((int)BuildingCategory.Network, PlayerPrefs.GetInt(BuildingPlacementUI.BuildTabPrefsKey));
        }

        [Test]
        public void Load_DefaultsToEnergy_WhenMissing()
        {
            BuildingPlacementUI.ClearTabPrefs();
            Assert.AreEqual(BuildingCategory.Energy, BuildingPlacementUI.LoadActiveCategory());
        }

        [Test]
        public void Load_FallsBackToEnergy_WhenCorrupt()
        {
            BuildingPlacementUI.ClearTabPrefs();
            PlayerPrefs.SetInt(BuildingPlacementUI.BuildTabPrefsKey, 99);
            PlayerPrefs.Save();
            Assert.AreEqual(BuildingCategory.Energy, BuildingPlacementUI.LoadActiveCategory());
        }

        [Test]
        public void MatchesCategory_FiltersByDefinitionCategory()
        {
            var solar = CreateBuilding("small_solar", BuildingCategory.Energy);
            var hub = CreateBuilding("distribution_hub", BuildingCategory.Network);

            Assert.IsTrue(BuildingPlacementUI.MatchesCategory(solar, BuildingCategory.Energy));
            Assert.IsFalse(BuildingPlacementUI.MatchesCategory(solar, BuildingCategory.Network));
            Assert.IsTrue(BuildingPlacementUI.MatchesCategory(hub, BuildingCategory.Network));
        }

        [Test]
        public void FilterForTab_ReturnsAllInCategory_IncludingLocked()
        {
            var buildings = new List<BuildingDefinition>
            {
                CreateBuilding("small_solar", BuildingCategory.Energy),
                CreateBuilding("small_wind", BuildingCategory.Energy),
                CreateBuilding("battery", BuildingCategory.Storage),
                CreateBuilding("village", BuildingCategory.Settlement)
            };
            var unlocks = new StubUnlocks("small_solar", "village");

            var energy = BuildingPlacementUI.FilterForTab(buildings, BuildingCategory.Energy, unlocks);
            Assert.AreEqual(2, energy.Count);
            Assert.AreEqual("small_solar", energy[0].Id);
            Assert.AreEqual("small_wind", energy[1].Id);

            var storage = BuildingPlacementUI.FilterForTab(buildings, BuildingCategory.Storage, unlocks);
            Assert.AreEqual(1, storage.Count);
            Assert.AreEqual("battery", storage[0].Id);

            var settlement = BuildingPlacementUI.FilterForTab(buildings, BuildingCategory.Settlement, unlocks);
            Assert.AreEqual(1, settlement.Count);
            Assert.AreEqual("village", settlement[0].Id);
        }

        [Test]
        public void SetActiveCategory_WritesPrefs()
        {
            var go = new GameObject("BuildTabUI");
            try
            {
                var ui = go.AddComponent<BuildingPlacementUI>();
                ui.SetActiveCategory(BuildingCategory.Service);
                Assert.AreEqual(BuildingCategory.Service, ui.ActiveCategory);
                Assert.AreEqual(BuildingCategory.Service, BuildingPlacementUI.LoadActiveCategory());
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void CanSelectBuilding_EnergyOnly_BlocksNetwork()
        {
            var line = CreateBuilding("power_line", BuildingCategory.Network);
            var wheel = CreateBuilding("water_wheel", BuildingCategory.Energy);
            var unlocks = new StubUnlocks("power_line", "water_wheel");

            Assert.IsFalse(BuildingPlacementUI.CanSelectBuilding(line, unlocks, energyOnly: true));
            Assert.IsTrue(BuildingPlacementUI.CanSelectBuilding(wheel, unlocks, energyOnly: true));
            Assert.IsTrue(BuildingPlacementUI.CanSelectBuilding(line, unlocks, energyOnly: false));
        }

        [Test]
        public void IsCategoryAllowed_EnergyOnly()
        {
            Assert.IsTrue(BuildingPlacementUI.IsCategoryAllowed(BuildingCategory.Energy, true));
            Assert.IsFalse(BuildingPlacementUI.IsCategoryAllowed(BuildingCategory.Network, true));
            Assert.IsTrue(BuildingPlacementUI.IsCategoryAllowed(BuildingCategory.Network, false));
        }

        private static BuildingDefinition CreateBuilding(string id, BuildingCategory category)
        {
            var def = ScriptableObject.CreateInstance<BuildingDefinition>();
            def.Configure(
                id, id, "", category,
                100f, 0f, 30f, 0f, 0f, 0f, false, true, Color.white);
            return def;
        }

        private sealed class StubUnlocks : IBuildingUnlockQuery
        {
            private readonly HashSet<string> _unlocked;

            public StubUnlocks(params string[] ids)
            {
                _unlocked = new HashSet<string>(ids);
            }

            public bool IsBuildingUnlocked(string buildingId) => _unlocked.Contains(buildingId);
        }
    }
}
