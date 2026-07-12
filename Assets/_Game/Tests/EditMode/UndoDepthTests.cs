using CleanEnergy.Buildings;
using CleanEnergy.Grid;
using CleanEnergy.Placement;
using NUnit.Framework;
using UnityEngine;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class UndoDepthTests
    {
        [Test]
        public void DemolishStack_TrimsToThree_LatestOnTop()
        {
            var go = new GameObject("UndoDepth");
            try
            {
                var placement = go.AddComponent<PlacementController>();
                var def = ScriptableObject.CreateInstance<BuildingDefinition>();
                def.Configure(
                    "power_line", "Line", "", BuildingCategory.Network,
                    40f, 0f, 35f, 0f, 0f, 0f, false, true, Color.white,
                    hub: true, hubLinkCapacity: 40f);
                placement.Configure(null, go.transform, new[] { def }, 500f);

                for (var i = 0; i < 4; i++)
                {
                    placement.Occupancy.TryOccupy(
                        new BuildingInstance($"b{i}", def, new GridCoordinate(i, 0), 0, null));
                    Assert.IsTrue(placement.TryDemolish(new GridCoordinate(i, 0), out _));
                }

                Assert.AreEqual(PlacementController.MaxDemolishUndoGroups, placement.DemolishUndoStackDepth);
                Assert.AreEqual(3, placement.DemolishUndoStackDepth);
                Assert.AreEqual(1, placement.DemolishUndoCount);
                Assert.AreEqual(3, placement.DemolishUndoGroup[0].Coordinate.X);

                // Without map, undo fails but LIFO top remains the latest group.
                Assert.IsFalse(placement.TryUndoLastDemolish());
                Assert.AreEqual(3, placement.DemolishUndoStackDepth);
                Assert.AreEqual(3, placement.DemolishUndoGroup[0].Coordinate.X);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void SecondDemolish_PushesSecondStackEntry()
        {
            var go = new GameObject("UndoPush");
            try
            {
                var placement = go.AddComponent<PlacementController>();
                var def = ScriptableObject.CreateInstance<BuildingDefinition>();
                def.Configure(
                    "power_line", "Line", "", BuildingCategory.Network,
                    40f, 0f, 35f, 0f, 0f, 0f, false, true, Color.white,
                    hub: true, hubLinkCapacity: 40f);
                placement.Configure(null, go.transform, new[] { def }, 200f);
                placement.Occupancy.TryOccupy(
                    new BuildingInstance("a", def, new GridCoordinate(0, 0), 0, null));
                placement.Occupancy.TryOccupy(
                    new BuildingInstance("b", def, new GridCoordinate(1, 0), 0, null));
                Assert.IsTrue(placement.TryDemolish(new GridCoordinate(0, 0), out _));
                Assert.AreEqual(1, placement.DemolishUndoStackDepth);
                Assert.IsTrue(placement.TryDemolish(new GridCoordinate(1, 0), out _));
                Assert.AreEqual(2, placement.DemolishUndoStackDepth);
                Assert.AreEqual(1, placement.DemolishUndoGroup[0].Coordinate.X);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void ClearDemolishUndo_ClearsEntireStack()
        {
            var go = new GameObject("UndoClearStack");
            try
            {
                var placement = go.AddComponent<PlacementController>();
                var def = ScriptableObject.CreateInstance<BuildingDefinition>();
                def.Configure(
                    "power_line", "Line", "", BuildingCategory.Network,
                    40f, 0f, 35f, 0f, 0f, 0f, false, true, Color.white,
                    hub: true, hubLinkCapacity: 40f);
                placement.Configure(null, go.transform, new[] { def }, 200f);
                placement.Occupancy.TryOccupy(
                    new BuildingInstance("a", def, new GridCoordinate(0, 0), 0, null));
                placement.Occupancy.TryOccupy(
                    new BuildingInstance("b", def, new GridCoordinate(1, 0), 0, null));
                placement.TryDemolish(new GridCoordinate(0, 0), out _);
                placement.TryDemolish(new GridCoordinate(1, 0), out _);
                Assert.AreEqual(2, placement.DemolishUndoStackDepth);

                placement.ClearDemolishUndo();
                Assert.IsFalse(placement.HasDemolishUndo);
                Assert.AreEqual(0, placement.DemolishUndoStackDepth);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }
}
