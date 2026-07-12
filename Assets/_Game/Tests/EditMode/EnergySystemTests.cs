using System.Collections.Generic;
using CleanEnergy.Buildings;
using CleanEnergy.Energy;
using CleanEnergy.Grid;
using CleanEnergy.Map;
using CleanEnergy.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class EnergySystemTests
    {
        [Test]
        public void ProductionFormula_UsesPotentialAndEfficiency()
        {
            var grid = CreateGrid(4);
            grid.SetSolarPotential(new GridCoordinate(1, 1), 0.5f);
            var settings = ScriptableObject.CreateInstance<MapGenerationSettings>();
            var def = CreateDef("small_solar", BuildingCategory.Energy, power: 10f, efficiency: 0.8f, solarMin: 0f);
            var instance = new BuildingInstance("p1", def, new GridCoordinate(1, 1), 0, null);
            var producer = new ResourceProducerAdapter(instance, grid, settings);

            var production = producer.GetAvailableProduction(new SimulationContext(1, 0.5f, SimulationSpeed.One));
            Assert.AreEqual(4f, production, 0.001f); // 10 * 0.5 * 0.8
        }

        [Test]
        public void Balance_NoShortage_WhenProductionMeetsDemand()
        {
            var producer = new FakeProducer("prod", 12f);
            var consumer = new FakeConsumer("cons", 10f);
            var component = SingleComponent(Node("n1", producer, consumer, null));
            var result = new EnergyBalanceCalculator().Calculate(
                component,
                new SimulationContext(1, 0.5f, SimulationSpeed.One));

            Assert.IsFalse(result.HasShortage);
            Assert.AreEqual(12f, result.Production, 0.001f);
            Assert.AreEqual(10f, result.Demand, 0.001f);
            Assert.AreEqual(2f, result.SurplusSold, 0.001f);
        }

        [Test]
        public void Balance_Shortage_WhenDemandExceedsProductionAndEmptyBattery()
        {
            var producer = new FakeProducer("prod", 4f);
            var consumer = new FakeConsumer("cons", 10f);
            var battery = new FakeStorage("bat", capacity: 20f, stored: 0f, rate: 10f);
            var component = SingleComponent(
                Node("n1", producer, null, null),
                Node("n2", null, consumer, null),
                Node("n3", null, null, battery));

            // Need them in same component with shared list - SingleComponent already merges
            var result = new EnergyBalanceCalculator().Calculate(
                component,
                new SimulationContext(1, 0.5f, SimulationSpeed.One));

            Assert.IsTrue(result.HasShortage);
            Assert.AreEqual(6f, result.Shortage, 0.001f);
        }

        [Test]
        public void Surplus_ChargesBatteryUpToCapacity()
        {
            var producer = new FakeProducer("prod", 20f);
            var consumer = new FakeConsumer("cons", 5f);
            var battery = new FakeStorage("bat", capacity: 10f, stored: 0f, rate: 100f);
            var component = SingleComponent(
                Node("n1", producer, null, null),
                Node("n2", null, consumer, null),
                Node("n3", null, null, battery));

            var result = new EnergyBalanceCalculator().Calculate(
                component,
                new SimulationContext(1, 0.5f, SimulationSpeed.One));

            Assert.AreEqual(10f, battery.StoredEnergy, 0.001f);
            Assert.AreEqual(5f, result.SurplusSold, 0.001f); // 20-5-10
            Assert.IsFalse(result.HasShortage);
        }

        [Test]
        public void Graph_ConnectsThroughHub()
        {
            var graph = new EnergyNetworkGraph();
            var a = Node("a", new FakeProducer("a", 1f), null, null, new GridCoordinate(0, 0));
            var b = Node("b", null, new FakeConsumer("b", 1f), null, new GridCoordinate(3, 0));
            var hub = new EnergyNetworkNode("hub", new GridCoordinate(1, 0), null, null, null, true, 5);
            graph.AddOrReplaceNode(a);
            graph.AddOrReplaceNode(b);
            graph.AddOrReplaceNode(hub);
            graph.Connect("hub", "a");
            graph.Connect("hub", "b");

            Assert.IsTrue(graph.AreConnected("a", "b"));
            Assert.AreEqual(1, graph.Components.Count);
        }

        [Test]
        public void Graph_WithoutLink_SeparateComponents()
        {
            var graph = new EnergyNetworkGraph();
            graph.AddOrReplaceNode(Node("a", new FakeProducer("a", 1f), null, null, new GridCoordinate(0, 0)));
            graph.AddOrReplaceNode(Node("b", null, new FakeConsumer("b", 1f), null, new GridCoordinate(3, 0)));

            Assert.IsFalse(graph.AreConnected("a", "b"));
            Assert.AreEqual(2, graph.Components.Count);
        }

        private static EnergyNetworkComponent SingleComponent(params EnergyNetworkNode[] nodes)
        {
            return new EnergyNetworkComponent(0, new List<EnergyNetworkNode>(nodes));
        }

        private static EnergyNetworkNode Node(
            string id,
            IEnergyProducer producer,
            IEnergyConsumer consumer,
            IEnergyStorage storage,
            GridCoordinate? coordinate = null)
        {
            return new EnergyNetworkNode(
                id,
                coordinate ?? new GridCoordinate(0, 0),
                producer,
                consumer,
                storage,
                false,
                4);
        }

        private static GridService CreateGrid(int size)
        {
            var grid = new GridService();
            grid.Create(size, size, 1f, Vector3.zero);
            return grid;
        }

        private static BuildingDefinition CreateDef(
            string id,
            BuildingCategory category,
            float power = 0f,
            float efficiency = 0.8f,
            float solarMin = 0f)
        {
            var def = ScriptableObject.CreateInstance<BuildingDefinition>();
            def.Configure(
                id, id, id, category,
                100f, power, 30f, 0f, solarMin, 0f,
                false, true, Color.white,
                buildingEfficiency: efficiency);
            return def;
        }

        private sealed class FakeProducer : IEnergyProducer
        {
            private readonly float _amount;
            public string NodeId { get; }
            public FakeProducer(string id, float amount) { NodeId = id; _amount = amount; }
            public float GetAvailableProduction(SimulationContext context) => _amount;
        }

        private sealed class FakeConsumer : IEnergyConsumer
        {
            private readonly float _demand;
            public string NodeId { get; }
            public FakeConsumer(string id, float demand) { NodeId = id; _demand = demand; }
            public float GetDemand(SimulationContext context) => _demand;
        }

        private sealed class FakeStorage : IEnergyStorage
        {
            public string NodeId { get; }
            public float StoredEnergy { get; private set; }
            public float Capacity { get; }
            public float MaxChargeRate { get; }
            public float MaxDischargeRate { get; }

            public FakeStorage(string id, float capacity, float stored, float rate)
            {
                NodeId = id;
                Capacity = capacity;
                StoredEnergy = stored;
                MaxChargeRate = rate;
                MaxDischargeRate = rate;
            }

            public float Charge(float amount)
            {
                var accepted = Mathf.Min(amount, Capacity - StoredEnergy, MaxChargeRate);
                StoredEnergy += accepted;
                return accepted;
            }

            public float Discharge(float amount)
            {
                var provided = Mathf.Min(amount, StoredEnergy, MaxDischargeRate);
                StoredEnergy -= provided;
                return provided;
            }
        }
    }
}
