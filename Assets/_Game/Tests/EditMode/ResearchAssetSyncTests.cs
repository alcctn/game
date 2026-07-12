using CleanEnergy.Research;
using NUnit.Framework;
using UnityEngine;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class ResearchAssetSyncTests
    {
        [Test]
        public void CreateRuntimeDefaultTree_IncludesStorageAndTier3()
        {
            var tree = ResearchService.CreateRuntimeDefaultTree();
            Assert.IsTrue(HasNode(tree, "storage_basic"));
            Assert.IsTrue(HasNode(tree, "battery_cap"));
            Assert.IsTrue(HasNode(tree, "hydro_tune"));
            Assert.IsTrue(HasNode(tree, "solar_inverter"));
            Assert.IsTrue(HasNode(tree, "wind_blade"));
        }

        [Test]
        public void Battery_UnlockRequiresStorageBasic()
        {
            var tree = ResearchService.CreateRuntimeDefaultTree();
            var storage = Find(tree, "storage_basic");
            Assert.IsNotNull(storage);
            CollectionAssert.Contains(storage.UnlockBuildingIds, "battery");

            var service = new ResearchService(tree);
            Assert.IsFalse(service.IsBuildingUnlocked("battery"));
            service.Wallet.Add(20f);
            Assert.IsTrue(service.TryUnlock("storage_basic"));
            Assert.IsTrue(service.IsBuildingUnlocked("battery"));
        }

        [Test]
        public void ConfigureGreenValleyPrototype_IsIdempotent()
        {
            var tree = ScriptableObject.CreateInstance<ResearchTreeDefinition>();
            tree.ConfigureGreenValleyPrototype();
            var count = tree.Nodes.Length;
            tree.ConfigureGreenValleyPrototype();
            Assert.AreEqual(count, tree.Nodes.Length);
            Assert.IsTrue(HasNode(tree, "storage_basic"));
            Assert.IsTrue(HasNode(tree, "wind_blade"));
        }

        private static bool HasNode(ResearchTreeDefinition tree, string id) => Find(tree, id) != null;

        private static ResearchNodeDefinition Find(ResearchTreeDefinition tree, string id)
        {
            for (var i = 0; i < tree.Nodes.Length; i++)
            {
                if (tree.Nodes[i] != null && tree.Nodes[i].Id == id)
                {
                    return tree.Nodes[i];
                }
            }

            return null;
        }
    }
}
