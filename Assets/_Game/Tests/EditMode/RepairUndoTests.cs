using System.Collections.Generic;
using CleanEnergy.Buildings;
using CleanEnergy.Economy;
using CleanEnergy.Grid;
using CleanEnergy.Maintenance;
using NUnit.Framework;
using UnityEngine;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class RepairUndoTests
    {
        private static BuildingDefinition CreateSolar()
        {
            var def = ScriptableObject.CreateInstance<BuildingDefinition>();
            def.Configure(
                "small_solar", "Solar", "", BuildingCategory.Energy,
                120f, 12f, 20f, 0f, 0.45f, 0f, false, true, Color.white, upkeepCost: 5f);
            return def;
        }

        [Test]
        public void ManualRepair_Undo_RefundsAndRestoresLevel()
        {
            var def = CreateSolar();
            var building = new BuildingInstance("s1", def, new GridCoordinate(0, 0), 0, null);
            building.MaintenanceLevel = 0.55f;
            var occupied = new Dictionary<GridCoordinate, BuildingInstance>
            {
                [building.Coordinate] = building
            };
            var wallet = new Wallet(200f);
            var undo = new RepairUndoService();

            Assert.IsTrue(MaintenanceService.TryManualRepair(building, wallet, undo, out _));
            Assert.AreEqual(1f, building.MaintenanceLevel, 0.001f);
            Assert.IsTrue(undo.HasRepairUndo);

            var moneyAfterRepair = wallet.Money;
            Assert.IsTrue(undo.TryUndoLast(occupied, wallet));
            Assert.AreEqual(0.55f, building.MaintenanceLevel, 0.001f);
            Assert.AreEqual(moneyAfterRepair + MaintenanceService.ManualRepairCost(building), wallet.Money, 0.001f);
            Assert.IsFalse(undo.HasRepairUndo);
        }

        [Test]
        public void MultiRepair_Undo_RestoresBatch()
        {
            var def = CreateSolar();
            var a = new BuildingInstance("a", def, new GridCoordinate(0, 0), 0, null) { MaintenanceLevel = 0.4f };
            var b = new BuildingInstance("b", def, new GridCoordinate(1, 0), 0, null) { MaintenanceLevel = 0.6f };
            var occupied = new Dictionary<GridCoordinate, BuildingInstance>
            {
                [a.Coordinate] = a,
                [b.Coordinate] = b
            };
            var wallet = new Wallet(500f);
            var undo = new RepairUndoService();

            Assert.IsTrue(MaintenanceService.TryRepairSelectedProducers(
                new[] { a.Coordinate, b.Coordinate },
                occupied,
                wallet,
                undo,
                out var count,
                out var cost,
                out _));
            Assert.AreEqual(2, count);
            Assert.AreEqual(1, undo.RepairUndoStackDepth);
            Assert.AreEqual(2, undo.RepairUndoCount);

            Assert.IsTrue(undo.TryUndoLast(occupied, wallet));
            Assert.AreEqual(0.4f, a.MaintenanceLevel, 0.001f);
            Assert.AreEqual(0.6f, b.MaintenanceLevel, 0.001f);
            Assert.AreEqual(500f, wallet.Money, 0.001f);
            Assert.Greater(cost, 0f);
        }

        [Test]
        public void GlobalRepair_Undo_PushesSeparateFromDemolishStack()
        {
            var def = CreateSolar();
            var a = new BuildingInstance("a", def, new GridCoordinate(0, 0), 0, null) { MaintenanceLevel = 0.5f };
            var occupied = new Dictionary<GridCoordinate, BuildingInstance>
            {
                [a.Coordinate] = a
            };
            var wallet = new Wallet(500f);
            var undo = new RepairUndoService();

            Assert.IsTrue(MaintenanceService.TryGlobalRepairAllProducers(
                occupied, wallet, undo, out _, out _, out _));
            Assert.IsTrue(undo.HasRepairUndo);
            Assert.AreEqual(1, undo.RepairUndoStackDepth);
        }

        [Test]
        public void RepairUndo_TrimsToThreeBatches()
        {
            var def = CreateSolar();
            var wallet = new Wallet(2000f);
            var undo = new RepairUndoService();

            for (var i = 0; i < 4; i++)
            {
                var building = new BuildingInstance($"s{i}", def, new GridCoordinate(i, 0), 0, null);
                building.MaintenanceLevel = 0.5f;
                var occupied = new Dictionary<GridCoordinate, BuildingInstance>
                {
                    [building.Coordinate] = building
                };
                Assert.IsTrue(MaintenanceService.TryManualRepair(building, wallet, undo, out _));
                Assert.IsTrue(occupied.ContainsKey(building.Coordinate));
            }

            Assert.AreEqual(RepairUndoService.MaxRepairUndoGroups, undo.RepairUndoStackDepth);
            Assert.AreEqual(3, undo.RepairUndoStackDepth);
            Assert.AreEqual("s3", undo.RepairUndoGroup[0].InstanceId);
        }

        [Test]
        public void RepairAndDemolish_StacksRemainIndependent()
        {
            var go = new GameObject("RepairDemolishSeparate");
            try
            {
                var placement = go.AddComponent<Placement.PlacementController>();
                var line = ScriptableObject.CreateInstance<BuildingDefinition>();
                line.Configure(
                    "power_line", "Line", "", BuildingCategory.Network,
                    40f, 0f, 35f, 0f, 0f, 0f, false, true, Color.white,
                    hub: true, hubLinkCapacity: 40f);
                placement.Configure(null, go.transform, new[] { line }, 200f);
                placement.Occupancy.TryOccupy(
                    new BuildingInstance("line", line, new GridCoordinate(0, 0), 0, null));
                Assert.IsTrue(placement.TryDemolish(new GridCoordinate(0, 0), out _));
                Assert.IsTrue(placement.HasDemolishUndo);

                var solar = CreateSolar();
                var producer = new BuildingInstance("p1", solar, new GridCoordinate(1, 0), 0, null);
                producer.MaintenanceLevel = 0.5f;
                var occupied = new Dictionary<GridCoordinate, BuildingInstance>
                {
                    [producer.Coordinate] = producer
                };
                var undo = new RepairUndoService();
                Assert.IsTrue(MaintenanceService.TryManualRepair(producer, placement.Wallet, undo, out _));

                Assert.IsTrue(placement.HasDemolishUndo);
                Assert.IsTrue(undo.HasRepairUndo);
                Assert.AreEqual(1, placement.DemolishUndoStackDepth);
                Assert.AreEqual(1, undo.RepairUndoStackDepth);

                Assert.IsTrue(undo.TryUndoLast(occupied, placement.Wallet));
                Assert.IsTrue(placement.HasDemolishUndo);
                Assert.IsFalse(undo.HasRepairUndo);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }
}
