using CleanEnergy.DebugTools;
using NUnit.Framework;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class NetworkEdgeFlowTests
    {
        [Test]
        public void ResolveEdgeUtilization_PrefersMaxHub()
        {
            Assert.AreEqual(0.8f, NetworkEdgeOverlay.ResolveEdgeUtilization(0.8f, true, 0.3f, true), 0.001f);
            Assert.AreEqual(0.4f, NetworkEdgeOverlay.ResolveEdgeUtilization(0.4f, true, 0f, false), 0.001f);
            Assert.AreEqual(0f, NetworkEdgeOverlay.ResolveEdgeUtilization(0f, false, 0f, false), 0.001f);
        }
    }
}
