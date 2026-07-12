using CleanEnergy.Research;
using NUnit.Framework;
using UnityEngine;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class ResearchTier3Tests
    {
        [Test]
        public void GreenValleyTree_IncludesTier3Nodes()
        {
            var tree = ScriptableObject.CreateInstance<ResearchTreeDefinition>();
            tree.ConfigureGreenValleyPrototype();
            Assert.IsTrue(HasNode(tree, "hydro_tune"));
            Assert.IsTrue(HasNode(tree, "solar_inverter"));
            Assert.IsTrue(HasNode(tree, "wind_blade"));
        }

        [Test]
        public void Tier3_RequiresTier2()
        {
            var tree = ScriptableObject.CreateInstance<ResearchTreeDefinition>();
            tree.ConfigureGreenValleyPrototype();
            var solar = Find(tree, "solar_inverter");
            Assert.AreEqual("solar_eff", solar.PrerequisiteNodeId);
            Assert.AreEqual(0, solar.UnlockBuildingIds.Length);
            Assert.Greater(solar.EfficiencyBonus, 0f);
        }

        private static bool HasNode(ResearchTreeDefinition tree, string id)
        {
            return Find(tree, id) != null;
        }

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
