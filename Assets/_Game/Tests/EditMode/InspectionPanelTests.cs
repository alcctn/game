using CleanEnergy.Buildings;
using CleanEnergy.Energy;
using CleanEnergy.Grid;
using CleanEnergy.UI;
using NUnit.Framework;
using UnityEngine;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class InspectionPanelTests
    {
        [Test]
        public void EmptyBuilding_NoBuildingStatus()
        {
            var status = InspectionStatus.ResolveNetwork(null, new EnergyNetworkGraph());
            Assert.AreEqual(InspectionNetworkStatus.NoBuilding, status);
            Assert.AreEqual("No building", InspectionStatus.NetworkLabel(status));
        }

        [Test]
        public void ProducerAlone_Isolated()
        {
            var graph = new EnergyNetworkGraph();
            var building = CreateBuilding("water_wheel", new GridCoordinate(0, 0));
            graph.AddOrReplaceNode(new EnergyNetworkNode(
                building.InstanceId,
                building.Coordinate,
                new FakeProducer(building.InstanceId, 1f),
                null,
                null,
                false,
                4,
                building.Definition.Id));

            var status = InspectionStatus.ResolveNetwork(building, graph);
            Assert.AreEqual(InspectionNetworkStatus.Isolated, status);
        }

        [Test]
        public void ProducerWithConsumer_Connected()
        {
            var graph = new EnergyNetworkGraph();
            var producer = CreateBuilding("water_wheel", new GridCoordinate(0, 0));
            var consumer = CreateBuilding("village", new GridCoordinate(2, 0), BuildingCategory.Settlement, demand: 10f);
            graph.AddOrReplaceNode(new EnergyNetworkNode(
                producer.InstanceId,
                producer.Coordinate,
                new FakeProducer(producer.InstanceId, 1f),
                null,
                null,
                false,
                4,
                producer.Definition.Id));
            graph.AddOrReplaceNode(new EnergyNetworkNode(
                consumer.InstanceId,
                consumer.Coordinate,
                null,
                new FakeConsumer(consumer.InstanceId, 1f),
                null,
                false,
                4,
                consumer.Definition.Id));
            graph.Connect(producer.InstanceId, consumer.InstanceId);

            var status = InspectionStatus.ResolveNetwork(producer, graph);
            Assert.AreEqual(InspectionNetworkStatus.Connected, status);
            Assert.AreEqual("Connected", InspectionStatus.NetworkLabel(status));
        }

        [Test]
        public void BuildingMissingFromGraph_NotInNetwork()
        {
            var building = CreateBuilding("small_solar", new GridCoordinate(1, 1));
            var status = InspectionStatus.ResolveNetwork(building, new EnergyNetworkGraph());
            Assert.AreEqual(InspectionNetworkStatus.NotInNetwork, status);
        }

        [Test]
        public void EfficiencyHint_ForProducer()
        {
            var def = ScriptableObject.CreateInstance<BuildingDefinition>();
            def.Configure(
                "water_wheel", "Water Wheel", "t",
                BuildingCategory.Energy,
                80f, 8f, 25f, 8f, 0f, 0f,
                true, false, Color.blue,
                buildingEfficiency: 0.8f);
            var hint = InspectionStatus.FormatEfficiencyHint(def, 0.9f);
            Assert.That(hint, Does.Contain("8"));
            Assert.That(hint, Does.Contain("0.80"));
            Assert.That(hint, Does.Contain("0.90"));
        }

        private static BuildingInstance CreateBuilding(
            string id,
            GridCoordinate coordinate,
            BuildingCategory category = BuildingCategory.Energy,
            float demand = 0f)
        {
            var def = ScriptableObject.CreateInstance<BuildingDefinition>();
            def.Configure(
                id, id, id, category,
                100f, category == BuildingCategory.Energy ? 10f : 0f, 30f, 0f, 0f, 0f,
                false, true, Color.white,
                demand: demand);
            return new BuildingInstance(id + "_1", def, coordinate, 0, null);
        }

        private sealed class FakeProducer : IEnergyProducer
        {
            public string NodeId { get; }
            private readonly float _amount;
            public FakeProducer(string id, float amount) { NodeId = id; _amount = amount; }
            public float GetAvailableProduction(Simulation.SimulationContext context) => _amount;
        }

        private sealed class FakeConsumer : IEnergyConsumer
        {
            public string NodeId { get; }
            private readonly float _demand;
            public FakeConsumer(string id, float demand) { NodeId = id; _demand = demand; }
            public float GetDemand(Simulation.SimulationContext context) => _demand;
        }
    }
}
