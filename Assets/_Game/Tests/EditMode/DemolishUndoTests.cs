using CleanEnergy.Buildings;
using CleanEnergy.Grid;
using CleanEnergy.Placement;
using NUnit.Framework;
using UnityEngine;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class DemolishUndoTests
    {
        [Test]
        public void TryDemolish_StoresUndoSnapshot()
        {
            var go = new GameObject("DemolishUndoSnap");
            try
            {
                var placement = go.AddComponent<PlacementController>();
                var def = ScriptableObject.CreateInstance<BuildingDefinition>();
                def.Configure(
                    "power_line", "Line", "", BuildingCategory.Network,
                    40f, 0f, 35f, 0f, 0f, 0f, false, true, Color.white,
                    hub: true, hubLinkCapacity: 40f);
                placement.Configure(null, go.transform, new[] { def }, 100f);

                var instance = new BuildingInstance(
                    "p1", def, new GridCoordinate(2, 2), 1, null)
                {
                    StoredEnergy = 3f,
                    MaintenanceLevel = 0.8f
                };
                Assert.IsTrue(placement.Occupancy.TryOccupy(instance));
                Assert.IsTrue(placement.TryDemolish(new GridCoordinate(2, 2), out var refund));
                Assert.AreEqual(20f, refund, 0.001f);
                Assert.IsTrue(placement.HasDemolishUndo);
                Assert.AreEqual(120f, placement.Wallet.Money, 0.001f);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void ClearDemolishUndo_ClearsPending()
        {
            var go = new GameObject("DemolishUndoClear");
            try
            {
                var placement = go.AddComponent<PlacementController>();
                var def = ScriptableObject.CreateInstance<BuildingDefinition>();
                def.Configure(
                    "power_line", "Line", "", BuildingCategory.Network,
                    40f, 0f, 35f, 0f, 0f, 0f, false, true, Color.white,
                    hub: true, hubLinkCapacity: 40f);
                placement.Configure(null, go.transform, new[] { def }, 100f);
                placement.Occupancy.TryOccupy(
                    new BuildingInstance("p1", def, new GridCoordinate(1, 1), 0, null));
                placement.TryDemolish(new GridCoordinate(1, 1), out _);
                Assert.IsTrue(placement.HasDemolishUndo);
                placement.ClearDemolishUndo();
                Assert.IsFalse(placement.HasDemolishUndo);
                Assert.IsFalse(placement.TryUndoLastDemolish());
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void TryUndo_WithoutMap_FailsAndRestoresRefundSpend()
        {
            var go = new GameObject("DemolishUndoFail");
            try
            {
                var placement = go.AddComponent<PlacementController>();
                var def = ScriptableObject.CreateInstance<BuildingDefinition>();
                def.Configure(
                    "power_line", "Line", "", BuildingCategory.Network,
                    40f, 0f, 35f, 0f, 0f, 0f, false, true, Color.white,
                    hub: true, hubLinkCapacity: 40f);
                placement.Configure(null, go.transform, new[] { def }, 100f);
                placement.Occupancy.TryOccupy(
                    new BuildingInstance("p1", def, new GridCoordinate(1, 1), 0, null));
                placement.TryDemolish(new GridCoordinate(1, 1), out _);
                var money = placement.Wallet.Money;
                Assert.IsFalse(placement.TryUndoLastDemolish());
                Assert.AreEqual(money, placement.Wallet.Money, 0.001f);
                Assert.IsTrue(placement.HasDemolishUndo);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void SecondDemolish_PushesOntoStack()
        {
            var go = new GameObject("DemolishUndoOverwrite");
            try
            {
                var placement = go.AddComponent<PlacementController>();
                var def = ScriptableObject.CreateInstance<BuildingDefinition>();
                def.Configure(
                    "power_line", "Line", "", BuildingCategory.Network,
                    40f, 0f, 35f, 0f, 0f, 0f, false, true, Color.white,
                    hub: true, hubLinkCapacity: 40f);
                placement.Configure(null, go.transform, new[] { def }, 100f);
                placement.Occupancy.TryOccupy(
                    new BuildingInstance("a", def, new GridCoordinate(0, 0), 0, null));
                placement.Occupancy.TryOccupy(
                    new BuildingInstance("b", def, new GridCoordinate(1, 0), 0, null));
                Assert.IsTrue(placement.TryDemolish(new GridCoordinate(0, 0), out _));
                Assert.IsTrue(placement.TryDemolish(new GridCoordinate(1, 0), out _));
                Assert.IsTrue(placement.HasDemolishUndo);
                Assert.AreEqual(2, placement.DemolishUndoStackDepth);
                Assert.IsFalse(placement.Occupancy.IsOccupied(new GridCoordinate(0, 0)));
                Assert.IsFalse(placement.Occupancy.IsOccupied(new GridCoordinate(1, 0)));
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }
}
