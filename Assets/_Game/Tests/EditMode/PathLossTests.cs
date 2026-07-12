using CleanEnergy.Energy;
using CleanEnergy.Grid;
using NUnit.Framework;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class PathLossTests
    {
        [Test]
        public void BfsHops_CountsEdgesNotManhattan()
        {
            var graph = new EnergyNetworkGraph();
            var producer = new EnergyNetworkNode(
                "p1", new GridCoordinate(0, 0), null, null, null, false, 20, "small_solar");
            var hub = new EnergyNetworkNode(
                "h1", new GridCoordinate(0, 8), null, null, null, true, 20, "distribution_hub", 40f);
            var village = new EnergyNetworkNode(
                "v1", new GridCoordinate(0, 16), null, new FakeConsumer(), null, false, 20, "village");
            graph.AddOrReplaceNode(producer);
            graph.AddOrReplaceNode(hub);
            graph.AddOrReplaceNode(village);
            graph.Connect("p1", "h1");
            graph.Connect("h1", "v1");

            // Manhattan producer→village = 16, path hops = 2
            Assert.AreEqual(2, TransmissionLoss.BfsHopsToNearestLoad("p1", graph));
            Assert.AreEqual(
                1f - TransmissionLoss.LossPerHop * 2f,
                TransmissionLoss.ResolveDeliveryFactor("p1", graph),
                0.001f);
            Assert.AreNotEqual(
                TransmissionLoss.FactorFromHops(16),
                TransmissionLoss.ResolveDeliveryFactor("p1", graph));
        }

        [Test]
        public void BfsHops_Unreachable_ReturnsMinusOne_FactorOne()
        {
            var graph = new EnergyNetworkGraph();
            graph.AddOrReplaceNode(new EnergyNetworkNode(
                "p1", new GridCoordinate(0, 0), null, null, null, false, 4, "small_solar"));
            graph.AddOrReplaceNode(new EnergyNetworkNode(
                "v1", new GridCoordinate(10, 0), null, new FakeConsumer(), null, false, 4, "village"));

            Assert.AreEqual(-1, TransmissionLoss.BfsHopsToNearestLoad("p1", graph));
            Assert.AreEqual(1f, TransmissionLoss.ResolveDeliveryFactor("p1", graph), 0.001f);
        }

        [Test]
        public void LossConstants_Unchanged()
        {
            Assert.AreEqual(0.05f, TransmissionLoss.LossPerHop, 0.0001f);
            Assert.AreEqual(0.25f, TransmissionLoss.MinDeliveryFactor, 0.0001f);
        }

        private sealed class FakeConsumer : IEnergyConsumer
        {
            public string NodeId => "fake";
            public float GetDemand(Simulation.SimulationContext context) => 1f;
        }
    }
}
