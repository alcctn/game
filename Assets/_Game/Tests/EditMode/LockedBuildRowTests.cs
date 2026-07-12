using System.Collections.Generic;
using CleanEnergy.Buildings;
using CleanEnergy.Placement;
using CleanEnergy.Research;
using CleanEnergy.UI;
using NUnit.Framework;
using UnityEngine;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class LockedBuildRowTests
    {
        [Test]
        public void FilterForTab_IncludesLockedBuildings()
        {
            var buildings = new List<BuildingDefinition>
            {
                CreateBuilding("small_solar", BuildingCategory.Energy),
                CreateBuilding("small_wind", BuildingCategory.Energy),
                CreateBuilding("village", BuildingCategory.Settlement)
            };
            var unlocks = new StubUnlocks("small_solar", "village");

            var energy = BuildingPlacementUI.FilterForTab(buildings, BuildingCategory.Energy, unlocks);
            Assert.AreEqual(2, energy.Count);
            Assert.AreEqual("small_solar", energy[0].Id);
            Assert.AreEqual("small_wind", energy[1].Id);
        }

        [Test]
        public void FormatLockedReason_UsesRequiresPrefix()
        {
            Assert.AreEqual("Requires: Basic Solar", BuildingPlacementUI.FormatLockedReason("Basic Solar"));
            Assert.AreEqual("Requires: solar_basic", BuildingPlacementUI.FormatLockedReason("solar_basic"));
        }

        [Test]
        public void ResolveUnlockRequirementLabel_PrefersDisplayName()
        {
            var tree = ResearchService.CreateRuntimeDefaultTree();
            Assert.AreEqual("Basic Solar", BuildingPlacementUI.ResolveUnlockRequirementLabel("small_solar", tree));
            Assert.AreEqual("Basic Storage", BuildingPlacementUI.ResolveUnlockRequirementLabel("battery", tree));
        }

        [Test]
        public void CanSelectBuilding_UnlockedTrue_LockedFalse()
        {
            var solar = CreateBuilding("small_solar", BuildingCategory.Energy);
            var wind = CreateBuilding("small_wind", BuildingCategory.Energy);
            var unlocks = new StubUnlocks("small_solar");

            Assert.IsTrue(BuildingPlacementUI.CanSelectBuilding(solar, unlocks));
            Assert.IsFalse(BuildingPlacementUI.CanSelectBuilding(wind, unlocks));
            Assert.IsTrue(BuildingPlacementUI.CanSelectBuilding(wind, null));
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
