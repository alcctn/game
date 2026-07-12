using CleanEnergy.Buildings;
using CleanEnergy.Grid;
using NUnit.Framework;
using UnityEngine;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class PrefabWireupTests
    {
        [Test]
        public void Create_PrefersAssignedPrefabOverPrimitive()
        {
            var prefab = new GameObject("village_source");
            prefab.AddComponent<PrefabMarker>();
            var def = ScriptableObject.CreateInstance<BuildingDefinition>();
            def.Configure(
                "village", "Village", "", BuildingCategory.Settlement,
                200f, 0f, 20f, 0f, 0f, 0f, false, true, Color.white, demand: 10f);
            def.SetPrefab(prefab);

            var grid = CreateFlatGrid(4);
            var factory = new BuildingFactory();
            BuildingInstance instance = null;
            try
            {
                instance = factory.Create(def, new GridCoordinate(1, 1), grid, null);
                Assert.IsNotNull(instance);
                Assert.IsNotNull(instance.GameObject);
                Assert.IsNotNull(instance.GameObject.GetComponent<PrefabMarker>());
                Assert.AreEqual(new Vector2Int(1, 1), def.Size);
            }
            finally
            {
                if (instance?.GameObject != null)
                {
                    Object.DestroyImmediate(instance.GameObject);
                }

                Object.DestroyImmediate(prefab);
            }
        }

        [Test]
        public void Create_WithoutPrefab_StillBuildsFootprintCompatibleInstance()
        {
            var def = ScriptableObject.CreateInstance<BuildingDefinition>();
            def.Configure(
                "small_solar", "Solar", "", BuildingCategory.Energy,
                120f, 12f, 20f, 0f, 0.45f, 0f, false, true, Color.yellow,
                footprint: new Vector2Int(2, 1));

            var grid = CreateFlatGrid(4);
            var factory = new BuildingFactory();
            BuildingInstance instance = null;
            try
            {
                instance = factory.Create(def, new GridCoordinate(0, 0), grid, null);
                Assert.IsNotNull(instance);
                Assert.IsNotNull(instance.GameObject);
                Assert.AreEqual(new Vector2Int(2, 1), def.Size);
                Assert.IsNull(def.Prefab);
            }
            finally
            {
                if (instance?.GameObject != null)
                {
                    Object.DestroyImmediate(instance.GameObject);
                }
            }
        }

        private static GridService CreateFlatGrid(int size)
        {
            var grid = new GridService();
            grid.Create(size, size, 1f, Vector3.zero);
            for (var x = 0; x < size; x++)
            {
                for (var y = 0; y < size; y++)
                {
                    grid.SetElevation(new GridCoordinate(x, y), 1f);
                }
            }

            return grid;
        }

        private sealed class PrefabMarker : MonoBehaviour
        {
        }
    }
}
