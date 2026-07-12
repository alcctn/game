using System.Collections.Generic;
using CleanEnergy.Buildings;
using CleanEnergy.Economy;
using CleanEnergy.Grid;
using CleanEnergy.Maintenance;
using NUnit.Framework;
using UnityEngine;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class BulkRepairTests
    {
        [Test]
        public void BulkRepair_AtomicallyRepairsCoveredProducers()
        {
            var depotDef = ScriptableObject.CreateInstance<BuildingDefinition>();
            depotDef.Configure(
                "maintenance_depot", "Depot", "", BuildingCategory.Service,
                160f, 0f, 30f, 0f, 0f, 0f, false, true, Color.white, linkRange: 5);
            var solarDef = ScriptableObject.CreateInstance<BuildingDefinition>();
            solarDef.Configure(
                "small_solar", "Solar", "", BuildingCategory.Energy,
                120f, 12f, 20f, 0f, 0.45f, 0f, false, true, Color.white, upkeepCost: 5f);

            var depot = new BuildingInstance("d1", depotDef, new GridCoordinate(0, 0), 0, null);
            var near = new BuildingInstance("s1", solarDef, new GridCoordinate(1, 0), 0, null);
            near.MaintenanceLevel = 0.5f;
            var far = new BuildingInstance("s2", solarDef, new GridCoordinate(20, 20), 0, null);
            far.MaintenanceLevel = 0.5f;

            var occupied = new Dictionary<GridCoordinate, BuildingInstance>
            {
                [depot.Coordinate] = depot,
                [near.Coordinate] = near,
                [far.Coordinate] = far
            };
            var wallet = new Wallet(100f);

            Assert.IsTrue(MaintenanceService.TryBulkRepairInDepotRange(
                depot, occupied, wallet, out var count, out var cost, out _));
            Assert.AreEqual(1, count);
            Assert.AreEqual(1f, near.MaintenanceLevel, 0.001f);
            Assert.AreEqual(0.5f, far.MaintenanceLevel, 0.001f);
            Assert.AreEqual(100f - cost, wallet.Money, 0.001f);
        }

        [Test]
        public void BulkRepair_FailsWithoutSpending_WhenBroke()
        {
            var depotDef = ScriptableObject.CreateInstance<BuildingDefinition>();
            depotDef.Configure(
                "maintenance_depot", "Depot", "", BuildingCategory.Service,
                160f, 0f, 30f, 0f, 0f, 0f, false, true, Color.white, linkRange: 5);
            var solarDef = ScriptableObject.CreateInstance<BuildingDefinition>();
            solarDef.Configure(
                "small_solar", "Solar", "", BuildingCategory.Energy,
                120f, 12f, 20f, 0f, 0.45f, 0f, false, true, Color.white, upkeepCost: 10f);

            var depot = new BuildingInstance("d1", depotDef, new GridCoordinate(0, 0), 0, null);
            var near = new BuildingInstance("s1", solarDef, new GridCoordinate(1, 0), 0, null);
            near.MaintenanceLevel = 0.5f;
            var occupied = new Dictionary<GridCoordinate, BuildingInstance>
            {
                [depot.Coordinate] = depot,
                [near.Coordinate] = near
            };
            var wallet = new Wallet(10f);

            Assert.IsFalse(MaintenanceService.TryBulkRepairInDepotRange(
                depot, occupied, wallet, out _, out _, out var fail));
            Assert.AreEqual(0.5f, near.MaintenanceLevel, 0.001f);
            Assert.AreEqual(10f, wallet.Money, 0.001f);
            Assert.IsFalse(string.IsNullOrEmpty(fail));
        }
    }
}
