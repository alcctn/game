using CleanEnergy.Art;
using NUnit.Framework;
using UnityEngine;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class PurePolyCatalogTests
    {
        [Test]
        public void Configure_AssignsTypedSlots()
        {
            var catalog = ScriptableObject.CreateInstance<PurePolyCatalog>();
            var meadow = new GameObject("meadow");
            var tree = new GameObject("tree");
            try
            {
                catalog.Configure(
                    meadow,
                    new[] { tree },
                    null,
                    null,
                    null,
                    null,
                    null,
                    null);
                Assert.AreSame(meadow, catalog.Meadow);
                Assert.AreEqual(1, catalog.ForestTrees.Length);
                Assert.AreEqual(2, catalog.AssignedSlotCount());
            }
            finally
            {
                Object.DestroyImmediate(meadow);
                Object.DestroyImmediate(tree);
            }
        }

        [Test]
        public void Pick_SkipsNullEntries()
        {
            var a = new GameObject("a");
            try
            {
                var picked = PurePolyCatalog.Pick(new GameObject[] { null, a }, 0);
                Assert.AreSame(a, picked);
                Assert.IsNull(PurePolyCatalog.Pick(null, 1));
                Assert.IsNull(PurePolyCatalog.Pick(System.Array.Empty<GameObject>(), 1));
            }
            finally
            {
                Object.DestroyImmediate(a);
            }
        }

        [Test]
        public void PrefabFolder_IsLockedPath()
        {
            Assert.AreEqual(
                "Assets/Pure Poly/Free Low Poly Nature Pack/Prefabs",
                PurePolyCatalog.PrefabFolder);
        }
    }
}
