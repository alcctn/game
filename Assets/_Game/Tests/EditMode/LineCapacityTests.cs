using System.Collections.Generic;
using CleanEnergy.Energy;
using CleanEnergy.Grid;
using CleanEnergy.Simulation;
using NUnit.Framework;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class LineCapacityTests
    {
        [Test]
        public void CapacityZero_Unlimited()
        {
            var component = BuildComponent(
                production: 100f,
                demand: 50f,
                hubCapacity: 0f);

            var result = new EnergyBalanceCalculator().Calculate(component, default);

            Assert.AreEqual(100f, result.Production, 0.001f);
            Assert.AreEqual(100f, result.DeliveredProduction, 0.001f);
            Assert.IsFalse(result.IsCongested);
            Assert.AreEqual(0f, result.LinkCapacity, 0.001f);
            Assert.IsFalse(result.HasShortage);
        }

        [Test]
        public void LowCapacity_CapsDeliveredProduction()
        {
            var component = BuildComponent(
                production: 100f,
                demand: 50f,
                hubCapacity: 40f);

            var result = new EnergyBalanceCalculator().Calculate(component, default);

            Assert.AreEqual(100f, result.Production, 0.001f);
            Assert.AreEqual(40f, result.DeliveredProduction, 0.001f);
            Assert.IsTrue(result.IsCongested);
            Assert.AreEqual(40f, result.LinkCapacity, 0.001f);
            Assert.AreEqual(10f, result.Shortage, 0.001f);
        }

        [Test]
        public void HighCapacity_NoCongestion()
        {
            var component = BuildComponent(
                production: 100f,
                demand: 50f,
                hubCapacity: 120f);

            var result = new EnergyBalanceCalculator().Calculate(component, default);

            Assert.AreEqual(100f, result.DeliveredProduction, 0.001f);
            Assert.IsFalse(result.IsCongested);
            Assert.AreEqual(120f, result.LinkCapacity, 0.001f);
            Assert.IsFalse(result.HasShortage);
        }

        [Test]
        public void TwoHubs_SumCapacity()
        {
            var prod = new EnergyNetworkNode(
                "prod", new GridCoordinate(0, 0),
                new FakeProducer("prod", 100f), null, null, false, 4);
            var cons = new EnergyNetworkNode(
                "cons", new GridCoordinate(0, 0),
                null, new FakeConsumer("cons", 50f), null, false, 4);
            var hubA = new EnergyNetworkNode(
                "hubA", new GridCoordinate(1, 0),
                null, null, null, true, 5, linkCapacity: 40f);
            var hubB = new EnergyNetworkNode(
                "hubB", new GridCoordinate(2, 0),
                null, null, null, true, 5, linkCapacity: 40f);

            var component = new EnergyNetworkComponent(
                0, new List<EnergyNetworkNode> { prod, cons, hubA, hubB });

            Assert.AreEqual(80f, EnergyBalanceCalculator.ResolveComponentCapacity(component), 0.001f);

            var result = new EnergyBalanceCalculator().Calculate(component, default);
            Assert.AreEqual(80f, result.DeliveredProduction, 0.001f);
            Assert.IsTrue(result.IsCongested);
            Assert.AreEqual(80f, result.LinkCapacity, 0.001f);
        }

        private static EnergyNetworkComponent BuildComponent(
            float production,
            float demand,
            float hubCapacity)
        {
            var prod = new EnergyNetworkNode(
                "prod", new GridCoordinate(0, 0),
                new FakeProducer("prod", production), null, null, false, 4);
            var cons = new EnergyNetworkNode(
                "cons", new GridCoordinate(0, 0),
                null, new FakeConsumer("cons", demand), null, false, 4);
            var hub = new EnergyNetworkNode(
                "hub", new GridCoordinate(2, 0),
                null, null, null, true, 5, linkCapacity: hubCapacity);
            return new EnergyNetworkComponent(0, new List<EnergyNetworkNode> { prod, cons, hub });
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
