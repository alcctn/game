using CleanEnergy.Buildings;
using NUnit.Framework;
using UnityEngine;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class PrefabPersistTests
    {
        [Test]
        public void BuildingPrefabIds_ListsAllNine()
        {
            Assert.AreEqual(9, BuildingPrefabIds.All.Length);
            CollectionAssert.AreEqual(
                new[]
                {
                    "village",
                    "distribution_hub",
                    "battery",
                    "small_solar",
                    "small_wind",
                    "water_wheel",
                    "small_hydro",
                    "power_line",
                    "maintenance_depot"
                },
                BuildingPrefabIds.All);
        }

        [Test]
        public void NeedsSpinChild_OnlyWindAndHydro()
        {
            Assert.IsTrue(BuildingPrefabIds.NeedsSpinChild("small_wind"));
            Assert.IsTrue(BuildingPrefabIds.NeedsSpinChild("water_wheel"));
            Assert.IsTrue(BuildingPrefabIds.NeedsSpinChild("small_hydro"));
            Assert.IsFalse(BuildingPrefabIds.NeedsSpinChild("village"));
            Assert.IsFalse(BuildingPrefabIds.NeedsSpinChild("small_solar"));
        }

        [Test]
        public void SetPrefab_AssignsPrefabReference()
        {
            var prefab = new GameObject("placeholder");
            var def = ScriptableObject.CreateInstance<BuildingDefinition>();
            def.Configure(
                "village", "Village", "", BuildingCategory.Settlement,
                200f, 0f, 20f, 0f, 0f, 0f, false, true, Color.white, demand: 10f);

            def.SetPrefab(prefab);

            Assert.AreSame(prefab, def.Prefab);
            Object.DestroyImmediate(prefab);
        }
    }
}
