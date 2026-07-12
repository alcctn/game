using CleanEnergy.Buildings;
using CleanEnergy.DebugTools;
using CleanEnergy.Grid;
using CleanEnergy.Placement;
using NUnit.Framework;
using UnityEngine;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class MultiDemolishUndoTests
    {
        [Test]
        public void TryDemolishMany_StoresGroupUndo()
        {
            var go = new GameObject("MultiDemo");
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

                Assert.IsTrue(placement.TryDemolishMany(
                    new[] { new GridCoordinate(0, 0), new GridCoordinate(1, 0) },
                    out var refund));
                Assert.AreEqual(40f, refund, 0.001f);
                Assert.AreEqual(2, placement.DemolishUndoCount);
                Assert.IsTrue(placement.HasDemolishUndo);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void Place_ClearsDemolishUndoGroup()
        {
            var go = new GameObject("MultiClear");
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
                placement.TryDemolish(new GridCoordinate(0, 0), out _);
                Assert.IsTrue(placement.HasDemolishUndo);
                placement.ClearDemolishUndo();
                Assert.IsFalse(placement.HasDemolishUndo);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void MultiSelect_ToggleRespectsMaxEight()
        {
            Assert.IsTrue(MapDebugOverlay.ShouldToggleMultiSelect(true));
            Assert.IsFalse(MapDebugOverlay.ShouldToggleMultiSelect(false));

            var go = new GameObject("MultiSelectMax");
            try
            {
                var overlay = go.AddComponent<MapDebugOverlay>();
                for (var i = 0; i < MapDebugOverlay.MaxMultiSelection; i++)
                {
                    Assert.IsTrue(overlay.ToggleMultiSelect(new GridCoordinate(i, 0)));
                }

                Assert.IsFalse(overlay.ToggleMultiSelect(new GridCoordinate(99, 0)));
                Assert.AreEqual(MapDebugOverlay.MaxMultiSelection, overlay.MultiSelectedCells.Count);
                Assert.IsTrue(overlay.ToggleMultiSelect(new GridCoordinate(0, 0)));
                Assert.AreEqual(MapDebugOverlay.MaxMultiSelection - 1, overlay.MultiSelectedCells.Count);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }
}
