using CleanEnergy.Buildings;
using CleanEnergy.Energy;
using CleanEnergy.Grid;
using CleanEnergy.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class DistributionHubTests
    {
        [Test]
        public void LongRangeHub_ConnectsDistantNodes()
        {
            // Producer at 0, consumer at 8; hub at 4 with range 10 reaches both (dist 4).
            var graph = BuildWithHub(
                producerAt: new GridCoordinate(0, 0),
                consumerAt: new GridCoordinate(8, 0),
                hubAt: new GridCoordinate(4, 0),
                hubRange: 10);

            Assert.IsTrue(graph.AreConnected("prod", "cons"));
            Assert.AreEqual(1, graph.Components.Count);
        }

        [Test]
        public void ShortRangeHub_DoesNotConnectDistantNodes()
        {
            // Same layout; power_line-scale range 5 cannot reach both from center
            // (hub→prod = 4 ok, hub→cons = 4 ok actually...)
            // Place nodes farther: 0 and 12, hub at 6 → Manhattan 6 > 5.
            var graph = BuildWithHub(
                producerAt: new GridCoordinate(0, 0),
                consumerAt: new GridCoordinate(12, 0),
                hubAt: new GridCoordinate(6, 0),
                hubRange: 5);

            Assert.IsFalse(graph.AreConnected("prod", "cons"));
            Assert.Greater(graph.Components.Count, 1);
        }

        [Test]
        public void ShortRangeFails_LongRangeSucceeds_SameLayout()
        {
            var shortGraph = BuildWithHub(
                producerAt: new GridCoordinate(0, 0),
                consumerAt: new GridCoordinate(12, 0),
                hubAt: new GridCoordinate(6, 0),
                hubRange: 5);
            var longGraph = BuildWithHub(
                producerAt: new GridCoordinate(0, 0),
                consumerAt: new GridCoordinate(12, 0),
                hubAt: new GridCoordinate(6, 0),
                hubRange: 10);

            Assert.IsFalse(shortGraph.AreConnected("prod", "cons"));
            Assert.IsTrue(longGraph.AreConnected("prod", "cons"));
        }

        [Test]
        public void PowerLineId_WithoutHubFlag_IsNotHub()
        {
            var def = ScriptableObject.CreateInstance<BuildingDefinition>();
            def.Configure(
                "power_line", "Power Line", "t",
                BuildingCategory.Network,
                40f, 0f, 35f, 0f, 0f, 0f,
                false, true, Color.yellow,
                linkRange: 5, hub: false);

            Assert.IsFalse(EnergyNetworkService.IsHubDefinition(def));
        }

        [Test]
        public void DistributionHub_WithHubFlag_IsHub()
        {
            var def = ScriptableObject.CreateInstance<BuildingDefinition>();
            def.Configure(
                "distribution_hub", "Distribution Hub", "t",
                BuildingCategory.Network,
                120f, 0f, 30f, 0f, 0f, 0f,
                false, true, Color.cyan,
                linkRange: 10, hub: true);

            Assert.IsTrue(EnergyNetworkService.IsHubDefinition(def));
            Assert.AreEqual(10, def.ConnectionRange);
        }

        private static EnergyNetworkGraph BuildWithHub(
            GridCoordinate producerAt,
            GridCoordinate consumerAt,
            GridCoordinate hubAt,
            int hubRange)
        {
            var graph = new EnergyNetworkGraph();
            var prod = new EnergyNetworkNode(
                "prod", producerAt, new FakeProducer("prod", 1f), null, null, false, 4);
            var cons = new EnergyNetworkNode(
                "cons", consumerAt, null, new FakeConsumer("cons", 1f), null, false, 4);
            var hub = new EnergyNetworkNode(
                "hub", hubAt, null, null, null, true, hubRange);

            graph.AddOrReplaceNode(prod);
            graph.AddOrReplaceNode(cons);
            graph.AddOrReplaceNode(hub);

            LinkHubToEnergy(graph, hub, prod);
            LinkHubToEnergy(graph, hub, cons);
            return graph;
        }

        private static void LinkHubToEnergy(
            EnergyNetworkGraph graph,
            EnergyNetworkNode hub,
            EnergyNetworkNode energy)
        {
            var d = Mathf.Abs(hub.Coordinate.X - energy.Coordinate.X)
                    + Mathf.Abs(hub.Coordinate.Y - energy.Coordinate.Y);
            if (d <= hub.ConnectionRange)
            {
                graph.Connect(hub.Id, energy.Id);
            }
        }

        private sealed class FakeProducer : IEnergyProducer
        {
            public string NodeId { get; }
            private readonly float _amount;

            public FakeProducer(string id, float amount)
            {
                NodeId = id;
                _amount = amount;
            }

            public float GetAvailableProduction(SimulationContext context) => _amount;
        }

        private sealed class FakeConsumer : IEnergyConsumer
        {
            public string NodeId { get; }
            private readonly float _demand;

            public FakeConsumer(string id, float demand)
            {
                NodeId = id;
                _demand = demand;
            }

            public float GetDemand(SimulationContext context) => _demand;
        }
    }
}
