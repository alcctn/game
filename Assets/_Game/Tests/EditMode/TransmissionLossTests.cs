using CleanEnergy.Energy;
using CleanEnergy.Grid;
using NUnit.Framework;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class TransmissionLossTests
    {
        [Test]
        public void DeliveryFactor_ScalesWithPathHops()
        {
            var graph = new EnergyNetworkGraph();
            var producer = new EnergyNetworkNode(
                "p1", new GridCoordinate(0, 0),
                producer: null, consumer: null, storage: null,
                isHub: false, connectionRange: 4, buildingTypeId: "small_solar");
            var hub = new EnergyNetworkNode(
                "h1", new GridCoordinate(1, 0),
                null, null, null, true, 8, "distribution_hub", linkCapacity: 40f);
            var village = new EnergyNetworkNode(
                "v1", new GridCoordinate(4, 0),
                producer: null, consumer: new FakeConsumer(), storage: null,
                isHub: false, connectionRange: 4, buildingTypeId: "village");
            graph.AddOrReplaceNode(producer);
            graph.AddOrReplaceNode(hub);
            graph.AddOrReplaceNode(village);
            graph.Connect("p1", "h1");
            graph.Connect("h1", "v1");

            var factor = TransmissionLoss.ResolveDeliveryFactor("p1", graph);
            Assert.AreEqual(1f - TransmissionLoss.LossPerHop * 2f, factor, 0.001f);
        }

        [Test]
        public void DeliveryFactor_ClampsAtMinimum()
        {
            var graph = new EnergyNetworkGraph();
            var producer = new EnergyNetworkNode(
                "p1", new GridCoordinate(0, 0),
                null, null, null, false, 4, "small_solar");
            EnergyNetworkNode prev = producer;
            graph.AddOrReplaceNode(producer);
            for (var i = 0; i < 20; i++)
            {
                var hub = new EnergyNetworkNode(
                    "h" + i, new GridCoordinate(i + 1, 0),
                    null, null, null, true, 4, "distribution_hub", linkCapacity: 40f);
                graph.AddOrReplaceNode(hub);
                graph.Connect(prev.Id, hub.Id);
                prev = hub;
            }

            var village = new EnergyNetworkNode(
                "v1", new GridCoordinate(40, 0),
                null, new FakeConsumer(), null, false, 4, "village");
            graph.AddOrReplaceNode(village);
            graph.Connect(prev.Id, "v1");

            Assert.AreEqual(
                TransmissionLoss.MinDeliveryFactor,
                TransmissionLoss.ResolveDeliveryFactor("p1", graph),
                0.001f);
        }

        [Test]
        public void DeliveryFactor_NoLoad_IsOne()
        {
            var graph = new EnergyNetworkGraph();
            var producer = new EnergyNetworkNode(
                "p1", new GridCoordinate(0, 0),
                null, null, null, false, 4, "small_solar");
            var hub = new EnergyNetworkNode(
                "h1", new GridCoordinate(1, 0),
                null, null, null, true, 4, "distribution_hub", linkCapacity: 40f);
            graph.AddOrReplaceNode(producer);
            graph.AddOrReplaceNode(hub);
            graph.Connect("p1", "h1");

            Assert.AreEqual(1f, TransmissionLoss.ResolveDeliveryFactor("p1", graph), 0.001f);
        }

        private sealed class FakeConsumer : IEnergyConsumer
        {
            public string NodeId => "fake";
            public float GetDemand(Simulation.SimulationContext context) => 1f;
        }
    }
}
