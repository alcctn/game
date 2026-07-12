using System.Collections.Generic;
using CleanEnergy.Buildings;
using CleanEnergy.Economy;
using CleanEnergy.Grid;
using NUnit.Framework;
using UnityEngine;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class UpkeepEconomyTests
    {
        [Test]
        public void CalculateTotal_SumsMaintenanceCost()
        {
            var service = new UpkeepService();
            // Spaced far apart so env density upkeep (S92) does not apply.
            var occupied = new Dictionary<GridCoordinate, BuildingInstance>
            {
                { new GridCoordinate(0, 0), CreateBuilding("a", 1f, new GridCoordinate(0, 0)) },
                { new GridCoordinate(5, 0), CreateBuilding("b", 1f, new GridCoordinate(5, 0)) },
                { new GridCoordinate(10, 0), CreateBuilding("c", 1f, new GridCoordinate(10, 0)) }
            };

            Assert.AreEqual(3f, service.CalculateTotal(occupied), 0.001f);
        }

        [Test]
        public void ProcessTick_DeductsFromWallet()
        {
            var service = new UpkeepService();
            var wallet = new Wallet(100f);
            var occupied = new Dictionary<GridCoordinate, BuildingInstance>
            {
                { new GridCoordinate(0, 0), CreateBuilding("a", 1f, new GridCoordinate(0, 0)) },
                { new GridCoordinate(5, 0), CreateBuilding("b", 1f, new GridCoordinate(5, 0)) },
                { new GridCoordinate(10, 0), CreateBuilding("c", 1f, new GridCoordinate(10, 0)) }
            };

            service.ProcessTick(occupied, wallet);

            Assert.AreEqual(3f, service.LastUpkeepTotal, 0.001f);
            Assert.AreEqual(97f, wallet.Money, 0.001f);
            Assert.IsFalse(service.CouldNotAffordFullUpkeep);
        }

        [Test]
        public void ProcessTick_ClampsToZero_WhenBroke()
        {
            var service = new UpkeepService();
            var wallet = new Wallet(2f);
            var occupied = new Dictionary<GridCoordinate, BuildingInstance>
            {
                { new GridCoordinate(0, 0), CreateBuilding("a", 2f, new GridCoordinate(0, 0)) },
                { new GridCoordinate(1, 0), CreateBuilding("b", 3f, new GridCoordinate(1, 0)) }
            };

            service.ProcessTick(occupied, wallet);

            Assert.AreEqual(5f, service.LastUpkeepTotal, 0.001f);
            Assert.AreEqual(0f, wallet.Money, 0.001f);
            Assert.IsTrue(service.CouldNotAffordFullUpkeep);
        }

        private static BuildingInstance CreateBuilding(string id, float upkeep, GridCoordinate coordinate)
        {
            var def = ScriptableObject.CreateInstance<BuildingDefinition>();
            def.Configure(
                id, id, id,
                BuildingCategory.Energy,
                100f, 10f, 30f, 0f, 0f, 0f,
                false, true, Color.white,
                upkeepCost: upkeep);
            return new BuildingInstance(id + "_1", def, coordinate, 0, null);
        }
    }
}
