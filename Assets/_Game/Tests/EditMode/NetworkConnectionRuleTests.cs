using System.Collections.Generic;
using CleanEnergy.Buildings;
using CleanEnergy.Grid;
using CleanEnergy.Placement;
using NUnit.Framework;
using UnityEngine;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class NetworkConnectionRuleTests
    {
        [Test]
        public void Producer_OnEmptyMap_Fails()
        {
            var solar = CreateSolar();
            var rule = new NetworkConnectionRule();
            var reasons = new List<string>();
            var context = new PlacementContext(
                solar, new GridCoordinate(1, 1), CreateGrid(4), new GridOccupancyService(), null);
            Assert.IsFalse(rule.Evaluate(context, reasons));
            Assert.AreEqual(NetworkConnectionRule.FailReason, reasons[0]);
        }

        [Test]
        public void Hub_OnEmptyMap_Passes()
        {
            var hub = ScriptableObject.CreateInstance<BuildingDefinition>();
            hub.Configure(
                "distribution_hub", "Hub", "", BuildingCategory.Network,
                120f, 0f, 30f, 0f, 0f, 0f, false, true, Color.white,
                hub: true, hubLinkCapacity: 120f);
            var rule = new NetworkConnectionRule();
            var reasons = new List<string>();
            var context = new PlacementContext(
                hub, new GridCoordinate(0, 0), CreateGrid(4), new GridOccupancyService(), null);
            Assert.IsTrue(rule.Evaluate(context, reasons));
        }

        [Test]
        public void Village_OnEmptyMap_Passes()
        {
            var village = ScriptableObject.CreateInstance<BuildingDefinition>();
            village.Configure(
                "village", "Village", "", BuildingCategory.Settlement,
                200f, 0f, 20f, 0f, 0f, 0f, false, true, Color.white, demand: 10f);
            var rule = new NetworkConnectionRule();
            var reasons = new List<string>();
            var context = new PlacementContext(
                village, new GridCoordinate(0, 0), CreateGrid(4), new GridOccupancyService(), null);
            Assert.IsTrue(rule.Evaluate(context, reasons));
        }

        [Test]
        public void Producer_WithinRangeOfHub_Passes()
        {
            var hub = ScriptableObject.CreateInstance<BuildingDefinition>();
            hub.Configure(
                "distribution_hub", "Hub", "", BuildingCategory.Network,
                120f, 0f, 30f, 0f, 0f, 0f, false, true, Color.white,
                linkRange: 4, hub: true, hubLinkCapacity: 120f);
            var solar = CreateSolar(linkRange: 4);
            var occupancy = new GridOccupancyService();
            occupancy.TryOccupy(new BuildingInstance("h1", hub, new GridCoordinate(0, 0), 0, null));

            var rule = new NetworkConnectionRule();
            var reasons = new List<string>();
            var near = new PlacementContext(
                solar, new GridCoordinate(3, 0), CreateGrid(8), occupancy, null);
            Assert.IsTrue(rule.Evaluate(near, reasons));

            reasons.Clear();
            var far = new PlacementContext(
                solar, new GridCoordinate(5, 0), CreateGrid(8), occupancy, null);
            Assert.IsFalse(rule.Evaluate(far, reasons));
        }

        private static BuildingDefinition CreateSolar(int linkRange = 4)
        {
            var solar = ScriptableObject.CreateInstance<BuildingDefinition>();
            solar.Configure(
                "small_solar", "Solar", "", BuildingCategory.Energy,
                120f, 10f, 20f, 0f, 0.45f, 0f, false, true, Color.white,
                linkRange: linkRange);
            return solar;
        }

        private static GridService CreateGrid(int size)
        {
            var grid = new GridService();
            grid.Create(size, size, 1f, Vector3.zero);
            return grid;
        }
    }
}
