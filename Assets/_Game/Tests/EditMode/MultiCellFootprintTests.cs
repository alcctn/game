using CleanEnergy.Buildings;
using CleanEnergy.Grid;
using CleanEnergy.Placement;
using NUnit.Framework;
using UnityEngine;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class MultiCellFootprintTests
    {
        [Test]
        public void GetFootprintSize_SwapsAxesOnOddRotation()
        {
            var def = ScriptableObject.CreateInstance<BuildingDefinition>();
            def.Configure(
                "small_solar", "Solar", "", BuildingCategory.Energy,
                120f, 12f, 20f, 0f, 0.45f, 0f, false, true, Color.yellow,
                footprint: new Vector2Int(2, 1));

            Assert.AreEqual(new Vector2Int(2, 1), BuildingFootprint.GetFootprintSize(def, 0));
            Assert.AreEqual(new Vector2Int(1, 2), BuildingFootprint.GetFootprintSize(def, 1));
            Assert.AreEqual(new Vector2Int(2, 1), BuildingFootprint.GetFootprintSize(def, 2));
            Assert.AreEqual(new Vector2Int(1, 2), BuildingFootprint.GetFootprintSize(def, 3));
        }

        [Test]
        public void GetCells_ReturnsOffsetFootprintFromAnchor()
        {
            var cells = BuildingFootprint.GetCells(new GridCoordinate(3, 4), new Vector2Int(2, 1));
            Assert.AreEqual(2, cells.Count);
            Assert.AreEqual(new GridCoordinate(3, 4), cells[0]);
            Assert.AreEqual(new GridCoordinate(4, 4), cells[1]);
        }

        [Test]
        public void Solar_2x1_OccupiesTwoCells()
        {
            var def = ScriptableObject.CreateInstance<BuildingDefinition>();
            def.Configure(
                "small_solar", "Solar", "", BuildingCategory.Energy,
                120f, 12f, 20f, 0f, 0.45f, 0f, false, true, Color.yellow,
                footprint: new Vector2Int(2, 1));

            var occupancy = new GridOccupancyService();
            var anchor = new GridCoordinate(1, 1);
            var instance = new BuildingInstance("solar_1", def, anchor, 0, null);

            Assert.IsTrue(occupancy.TryOccupy(instance));
            Assert.IsTrue(occupancy.IsOccupied(new GridCoordinate(1, 1)));
            Assert.IsTrue(occupancy.IsOccupied(new GridCoordinate(2, 1)));
            Assert.IsTrue(occupancy.TryGet(new GridCoordinate(2, 1), out var found));
            Assert.AreEqual("solar_1", found.InstanceId);
        }

        [Test]
        public void Occupy_BlocksOverlappingFootprintCells()
        {
            var def = ScriptableObject.CreateInstance<BuildingDefinition>();
            def.Configure(
                "small_solar", "Solar", "", BuildingCategory.Energy,
                120f, 12f, 20f, 0f, 0.45f, 0f, false, true, Color.yellow,
                footprint: new Vector2Int(2, 1));

            var occupancy = new GridOccupancyService();
            Assert.IsTrue(occupancy.TryOccupy(
                new BuildingInstance("a", def, new GridCoordinate(0, 0), 0, null)));

            // Overlaps on cell (1,0)
            Assert.IsFalse(occupancy.TryOccupy(
                new BuildingInstance("b", def, new GridCoordinate(1, 0), 0, null)));

            // Clear first, rotated 1x2 should occupy (0,0) and (0,1)
            occupancy.Release(new GridCoordinate(0, 0));
            Assert.IsTrue(occupancy.TryOccupy(
                new BuildingInstance("c", def, new GridCoordinate(0, 0), 1, null)));
            Assert.IsTrue(occupancy.IsOccupied(new GridCoordinate(0, 1)));
            Assert.IsFalse(occupancy.IsOccupied(new GridCoordinate(1, 0)));
        }

        [Test]
        public void Release_ClearsEntireFootprint()
        {
            var def = ScriptableObject.CreateInstance<BuildingDefinition>();
            def.Configure(
                "small_solar", "Solar", "", BuildingCategory.Energy,
                120f, 12f, 20f, 0f, 0.45f, 0f, false, true, Color.yellow,
                footprint: new Vector2Int(2, 1));

            var occupancy = new GridOccupancyService();
            occupancy.TryOccupy(new BuildingInstance("s", def, new GridCoordinate(2, 2), 0, null));
            Assert.IsTrue(occupancy.Release(new GridCoordinate(3, 2)));
            Assert.IsFalse(occupancy.IsOccupied(new GridCoordinate(2, 2)));
            Assert.IsFalse(occupancy.IsOccupied(new GridCoordinate(3, 2)));
        }
    }
}
