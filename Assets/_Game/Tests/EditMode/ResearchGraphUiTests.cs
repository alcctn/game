using CleanEnergy.Research;
using CleanEnergy.UI;
using NUnit.Framework;
using UnityEngine;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class ResearchGraphUiTests
    {
        [Test]
        public void PrerequisiteDepth_CountsChain()
        {
            var root = new ResearchNodeDefinition();
            root.Configure("hydro_basic", "Basic Hydro", "", "", 0f, true, null, "", 0f);
            var mid = new ResearchNodeDefinition();
            mid.Configure("hydro_eff", "Hydro Eff", "", "hydro_basic", 20f, false, null, "", 0f);
            var leaf = new ResearchNodeDefinition();
            leaf.Configure("hydro_tune", "Hydro Tune", "", "hydro_eff", 35f, false, null, "", 0f);

            var byId = ResearchGraphLayout.IndexById(new[] { root, mid, leaf });
            Assert.AreEqual(0, ResearchGraphLayout.PrerequisiteDepth(root, byId));
            Assert.AreEqual(1, ResearchGraphLayout.PrerequisiteDepth(mid, byId));
            Assert.AreEqual(2, ResearchGraphLayout.PrerequisiteDepth(leaf, byId));
        }

        [Test]
        public void OrderForGraph_GroupsByBranchThenDepth()
        {
            var tree = ScriptableObject.CreateInstance<ResearchTreeDefinition>();
            tree.ConfigureGreenValleyPrototype();
            var ordered = ResearchGraphLayout.OrderForGraph(tree.Nodes);
            var byId = ResearchGraphLayout.IndexById(tree.Nodes);

            Assert.Greater(ordered.Count, 4);
            for (var i = 1; i < ordered.Count; i++)
            {
                var prevBranch = ResearchGraphLayout.ResolveBranchRootId(ordered[i - 1], byId);
                var branch = ResearchGraphLayout.ResolveBranchRootId(ordered[i], byId);
                var cmp = string.CompareOrdinal(prevBranch, branch);
                Assert.LessOrEqual(cmp, 0);
                if (cmp == 0)
                {
                    Assert.LessOrEqual(
                        ResearchGraphLayout.PrerequisiteDepth(ordered[i - 1], byId),
                        ResearchGraphLayout.PrerequisiteDepth(ordered[i], byId));
                }
            }
        }

        [Test]
        public void IndentPrefix_ScalesWithDepth()
        {
            Assert.AreEqual(string.Empty, ResearchGraphLayout.IndentPrefix(0));
            Assert.AreEqual("  ", ResearchGraphLayout.IndentPrefix(1));
            Assert.AreEqual("    ", ResearchGraphLayout.IndentPrefix(2));
        }
    }
}
