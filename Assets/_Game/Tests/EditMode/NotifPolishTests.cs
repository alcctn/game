using CleanEnergy.Research;
using CleanEnergy.UI;
using NUnit.Framework;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class NotifPolishTests
    {
        [Test]
        public void ResolveUnlockDisplayName_PrefersDisplayName()
        {
            var node = new ResearchNodeDefinition();
            node.Configure(
                "tier2_solar",
                "Advanced Solar",
                "",
                "",
                10f,
                false,
                null,
                "",
                0f);

            Assert.AreEqual(
                "Advanced Solar",
                NotificationController.ResolveUnlockDisplayName("tier2_solar", node));
        }

        [Test]
        public void ResolveUnlockDisplayName_FallsBackToNodeId()
        {
            Assert.AreEqual(
                "missing_node",
                NotificationController.ResolveUnlockDisplayName("missing_node", null));
            Assert.AreEqual(
                string.Empty,
                NotificationController.ResolveUnlockDisplayName(null, null));
        }

        [Test]
        public void ResolveUnlockDisplayName_EmptyDisplayFallsBack()
        {
            var node = new ResearchNodeDefinition();
            node.Configure("id_only", "", "", "", 1f, false, null, "", 0f);
            Assert.AreEqual(
                "id_only",
                NotificationController.ResolveUnlockDisplayName("id_only", node));
        }
    }
}
