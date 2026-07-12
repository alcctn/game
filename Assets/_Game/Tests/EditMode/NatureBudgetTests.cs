using CleanEnergy.Art;
using NUnit.Framework;
using UnityEngine;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class NatureBudgetTests
    {
        [Test]
        public void AllocateBudget_SharesSumToMax()
        {
            NatureSpawnMath.AllocateBudget(100, out var p, out var f, out var h, out var l, out var b);
            Assert.AreEqual(100, p + f + h + l + b);
            Assert.GreaterOrEqual(f, p);
        }

        [Test]
        public void ClearNatureRoot_RemovesChildren()
        {
            var host = new GameObject("BudgetHost");
            var root = new GameObject(NatureVisualSpawner.NatureRootName);
            root.transform.SetParent(host.transform, false);
            var child = new GameObject("prop");
            child.transform.SetParent(root.transform, false);
            try
            {
                var spawner = host.AddComponent<NatureVisualSpawner>();
                spawner.Configure(null, null, root.transform);
                Assert.AreEqual(1, root.transform.childCount);
                spawner.ClearNatureRoot();
                Assert.AreEqual(0, root.transform.childCount);
                Assert.AreEqual(0, spawner.SpawnedCount);
            }
            finally
            {
                Object.DestroyImmediate(host);
            }
        }

        [Test]
        public void Rebuild_WithoutCatalog_StaysAtZero()
        {
            var host = new GameObject("BudgetEmpty");
            try
            {
                var spawner = host.AddComponent<NatureVisualSpawner>();
                spawner.Configure(null, null, host.transform);
                spawner.Rebuild("seed");
                Assert.AreEqual(0, spawner.SpawnedCount);
            }
            finally
            {
                Object.DestroyImmediate(host);
            }
        }
    }
}
