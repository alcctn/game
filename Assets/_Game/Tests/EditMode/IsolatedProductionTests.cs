using System.Collections.Generic;
using CleanEnergy.Energy;
using CleanEnergy.Grid;
using CleanEnergy.Simulation;
using NUnit.Framework;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class IsolatedProductionTests
    {
        [Test]
        public void ComponentHasLoad_RequiresConsumerOrStorage()
        {
            var producerOnly = new EnergyNetworkComponent(
                0,
                new List<EnergyNetworkNode>
                {
                    new EnergyNetworkNode("p", new GridCoordinate(0, 0), new FakeProducer(10f), null, null, false, 4)
                });
            Assert.IsFalse(NetworkLoadFactor.ComponentHasLoad(producerOnly));

            var withVillage = new EnergyNetworkComponent(
                1,
                new List<EnergyNetworkNode>
                {
                    new EnergyNetworkNode("p", new GridCoordinate(0, 0), new FakeProducer(10f), null, null, false, 4),
                    new EnergyNetworkNode("c", new GridCoordinate(1, 0), null, new FakeConsumer(5f), null, false, 4)
                });
            Assert.IsTrue(NetworkLoadFactor.ComponentHasLoad(withVillage));
        }

        [Test]
        public void Calculate_IsolatedProducer_YieldsZeroProduction()
        {
            var calculator = new EnergyBalanceCalculator();
            var component = new EnergyNetworkComponent(
                0,
                new List<EnergyNetworkNode>
                {
                    new EnergyNetworkNode("p", new GridCoordinate(0, 0), new FakeProducer(12f), null, null, false, 4)
                });
            var result = calculator.Calculate(component, new SimulationContext(1, 0.5f, SimulationSpeed.One));
            Assert.AreEqual(0f, result.Production, 0.001f);
            Assert.AreEqual(0f, result.SurplusSold, 0.001f);
        }

        [Test]
        public void Calculate_ConnectedProducer_KeepsProduction()
        {
            var calculator = new EnergyBalanceCalculator();
            var component = new EnergyNetworkComponent(
                0,
                new List<EnergyNetworkNode>
                {
                    new EnergyNetworkNode("p", new GridCoordinate(0, 0), new FakeProducer(12f), null, null, false, 4),
                    new EnergyNetworkNode("c", new GridCoordinate(1, 0), null, new FakeConsumer(5f), null, false, 4)
                });
            var result = calculator.Calculate(component, new SimulationContext(1, 0.5f, SimulationSpeed.One));
            Assert.AreEqual(12f, result.Production, 0.001f);
        }

        private sealed class FakeProducer : IEnergyProducer
        {
            private readonly float _amount;
            public FakeProducer(float amount) => _amount = amount;
            public string NodeId => "p";
            public float GetAvailableProduction(SimulationContext context) => _amount;
        }

        private sealed class FakeConsumer : IEnergyConsumer
        {
            private readonly float _demand;
            public FakeConsumer(float demand) => _demand = demand;
            public string NodeId => "c";
            public float GetDemand(SimulationContext context) => _demand;
        }
    }
}
