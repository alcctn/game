using System.Collections.Generic;
using CleanEnergy.Buildings;
using CleanEnergy.Economy;
using CleanEnergy.Grid;
using CleanEnergy.Maintenance;
using NUnit.Framework;
using UnityEngine;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class MultiRepairTests
    {
        [Test]
        public void RepairSelected_AtomicallyRepairsOnlySelectedProducers()
        {
            var solarDef = ScriptableObject.CreateInstance<BuildingDefinition>();
            solarDef.Configure(
                "small_solar", "Solar", "", BuildingCategory.Energy,
                120f, 12f, 20f, 0f, 0.45f, 0f, false, true, Color.white, upkeepCost: 5f);

            var a = new BuildingInstance("s1", solarDef, new GridCoordinate(0, 0), 0, null);
            a.MaintenanceLevel = 0.5f;
            var b = new BuildingInstance("s2", solarDef, new GridCoordinate(1, 0), 0, null);
            b.MaintenanceLevel = 0.6f;
            var c = new BuildingInstance("s3", solarDef, new GridCoordinate(2, 0), 0, null);
            c.MaintenanceLevel = 0.4f;

            var occupied = new Dictionary<GridCoordinate, BuildingInstance>
            {
                [a.Coordinate] = a,
                [b.Coordinate] = b,
                [c.Coordinate] = c
            };
            var wallet = new Wallet(200f);

            Assert.IsTrue(MaintenanceService.TryRepairSelectedProducers(
                new[] { a.Coordinate, b.Coordinate },
                occupied,
                wallet,
                out var count,
                out var cost,
                out _));
            Assert.AreEqual(2, count);
            Assert.AreEqual(1f, a.MaintenanceLevel, 0.001f);
            Assert.AreEqual(1f, b.MaintenanceLevel, 0.001f);
            Assert.AreEqual(0.4f, c.MaintenanceLevel, 0.001f);
            Assert.AreEqual(200f - cost, wallet.Money, 0.001f);
        }

        [Test]
        public void RepairSelected_FailsWithoutSpending_WhenBroke()
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

            Assert.IsFalse(MaintenanceService.TryRepairSelectedProducers(
                new[] { a.Coordinate },
                occupied,
                wallet,
                out _,
                out _,
                out var fail));
            Assert.AreEqual(0.5f, a.MaintenanceLevel, 0.001f);
            Assert.AreEqual(5f, wallet.Money, 0.001f);
            Assert.IsFalse(string.IsNullOrEmpty(fail));
        }

        [Test]
        public void RepairSelected_IgnoresNonProducersAndFull()
        {
            var solarDef = ScriptableObject.CreateInstance<BuildingDefinition>();
            solarDef.Configure(
                "small_solar", "Solar", "", BuildingCategory.Energy,
                120f, 12f, 20f, 0f, 0.45f, 0f, false, true, Color.white, upkeepCost: 5f);
            var hubDef = ScriptableObject.CreateInstance<BuildingDefinition>();
            hubDef.Configure(
                "power_line", "Line", "", BuildingCategory.Network,
                40f, 0f, 35f, 0f, 0f, 0f, false, true, Color.white,
                hub: true, hubLinkCapacity: 40f);

            var producer = new BuildingInstance("s1", solarDef, new GridCoordinate(0, 0), 0, null);
            producer.MaintenanceLevel = 0.5f;
            var full = new BuildingInstance("s2", solarDef, new GridCoordinate(1, 0), 0, null);
            full.MaintenanceLevel = 1f;
            var hub = new BuildingInstance("h1", hubDef, new GridCoordinate(2, 0), 0, null);

            var occupied = new Dictionary<GridCoordinate, BuildingInstance>
            {
                [producer.Coordinate] = producer,
                [full.Coordinate] = full,
                [hub.Coordinate] = hub
            };
            var wallet = new Wallet(200f);

            Assert.IsTrue(MaintenanceService.TryRepairSelectedProducers(
                new[] { producer.Coordinate, full.Coordinate, hub.Coordinate },
                occupied,
                wallet,
                out var count,
                out _,
                out _));
            Assert.AreEqual(1, count);
            Assert.AreEqual(1f, producer.MaintenanceLevel, 0.001f);
        }
    }
}
