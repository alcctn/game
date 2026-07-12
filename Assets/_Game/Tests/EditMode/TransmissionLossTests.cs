using CleanEnergy.Energy;
using CleanEnergy.Grid;
using NUnit.Framework;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class TransmissionLossTests
    {
        [Test]
        public void DeliveryFactor_ScalesWithManhattanHops()
        {
            var producer = new EnergyNetworkNode(
                "p1", new GridCoordinate(0, 0),
                producer: null, consumer: null, storage: null,
                isHub: false, connectionRange: 4, buildingTypeId: "small_solar");
            var village = new EnergyNetworkNode(
                "v1", new GridCoordinate(4, 0),
                producer: null, consumer: new FakeConsumer(), storage: null,
                isHub: false, connectionRange: 4, buildingTypeId: "village");

            var component = new EnergyNetworkComponent(
                0, new System.Collections.Generic.List<EnergyNetworkNode> { producer, village });

            var factor = TransmissionLoss.ResolveDeliveryFactor(producer.Coordinate, component);
            Assert.AreEqual(1f - 0.05f * 4f, factor, 0.001f);
        }

        [Test]
        public void DeliveryFactor_ClampsAtMinimum()
        {
            var producer = new EnergyNetworkNode(
                "p1", new GridCoordinate(0, 0),
                null, null, null, false, 4, "small_solar");
            var village = new EnergyNetworkNode(
                "v1", new GridCoordinate(40, 0),
                null, new FakeConsumer(), null, false, 4, "village");
            var component = new EnergyNetworkComponent(
                0, new System.Collections.Generic.List<EnergyNetworkNode> { producer, village });

            Assert.AreEqual(
                TransmissionLoss.MinDeliveryFactor,
                TransmissionLoss.ResolveDeliveryFactor(producer.Coordinate, component),
                0.001f);
        }

        [Test]
        public void DeliveryFactor_NoLoad_IsOne()
        {
            var producer = new EnergyNetworkNode(
                "p1", new GridCoordinate(0, 0),
                null, null, null, false, 4, "small_solar");
            var hub = new EnergyNetworkNode(
                "h1", new GridCoordinate(1, 0),
                null, null, null, true, 4, "distribution_hub", linkCapacity: 40f);
            var component = new EnergyNetworkComponent(
                0, new System.Collections.Generic.List<EnergyNetworkNode> { producer, hub });

            Assert.AreEqual(
                1f,
                TransmissionLoss.ResolveDeliveryFactor(producer.Coordinate, component),
                0.001f);
        }

        private sealed class FakeConsumer : IEnergyConsumer
        {
            public string NodeId => "fake";
            public float GetDemand(Simulation.SimulationContext context) => 1f;
        }
    }
}
