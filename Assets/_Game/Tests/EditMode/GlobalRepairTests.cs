using System.Collections.Generic;
using CleanEnergy.Buildings;
using CleanEnergy.Economy;
using CleanEnergy.Grid;
using CleanEnergy.Maintenance;
using NUnit.Framework;
using UnityEngine;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class GlobalRepairTests
    {
        [Test]
        public void GlobalRepair_AtomicallyRepairsAllProducers()
        {
            var solarDef = ScriptableObject.CreateInstance<BuildingDefinition>();
            solarDef.Configure(
                "small_solar", "Solar", "", BuildingCategory.Energy,
                120f, 12f, 20f, 0f, 0.45f, 0f, false, true, Color.white, upkeepCost: 5f);

            var a = new BuildingInstance("s1", solarDef, new GridCoordinate(0, 0), 0, null);
            a.MaintenanceLevel = 0.5f;
            var b = new BuildingInstance("s2", solarDef, new GridCoordinate(20, 20), 0, null);
            b.MaintenanceLevel = 0.6f;
            var full = new BuildingInstance("s3", solarDef, new GridCoordinate(1, 1), 0, null);
            full.MaintenanceLevel = 1f;

            var occupied = new Dictionary<GridCoordinate, BuildingInstance>
            {
                [a.Coordinate] = a,
                [b.Coordinate] = b,
                [full.Coordinate] = full
            };
            var wallet = new Wallet(200f);

            Assert.IsTrue(MaintenanceService.TryGlobalRepairAllProducers(
                occupied, wallet, out var count, out var cost, out _));
            Assert.AreEqual(2, count);
            Assert.AreEqual(1f, a.MaintenanceLevel, 0.001f);
            Assert.AreEqual(1f, b.MaintenanceLevel, 0.001f);
            Assert.AreEqual(200f - cost, wallet.Money, 0.001f);
        }

        [Test]
        public void GlobalRepair_FailsWithoutSpending_WhenBroke()
        {
            var solarDef = ScriptableObject.CreateInstance<BuildingDefinition>();
            solarDef.Configure(
                "small_solar", "Solar", "", BuildingCategory.Energy,
                120f, 12f, 20f, 0f, 0.45f, 0f, false, true, Color.white, upkeepCost: 10f);

            var a = new BuildingInstance("s1", solarDef, new GridCoordinate(0, 0), 0, null);
            a.MaintenanceLevel = 0.5f;
            var occupied = new Dictionary<GridCoordinate, BuildingInstance>
            {
                [a.Coordinate] = a
            };
            var wallet = new Wallet(5f);

            Assert.IsFalse(MaintenanceService.TryGlobalRepairAllProducers(
                occupied, wallet, out _, out _, out var fail));
            Assert.AreEqual(0.5f, a.MaintenanceLevel, 0.001f);
            Assert.AreEqual(5f, wallet.Money, 0.001f);
            Assert.IsFalse(string.IsNullOrEmpty(fail));
        }
    }
}
