using CleanEnergy.Buildings;
using CleanEnergy.Grid;
using CleanEnergy.Placement;
using NUnit.Framework;
using UnityEngine;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class LineCostDistanceTests
    {
        [Test]
        public void PowerLine_ScalesWithManhattanDistance()
        {
            var line = ScriptableObject.CreateInstance<BuildingDefinition>();
            line.Configure(
                "power_line", "Line", "", BuildingCategory.Network,
                40f, 0f, 35f, 0f, 0f, 0f, false, true, Color.white,
                hub: true, hubLinkCapacity: 40f);
            var hub = ScriptableObject.CreateInstance<BuildingDefinition>();
            hub.Configure(
                "distribution_hub", "Hub", "", BuildingCategory.Network,
                120f, 0f, 30f, 0f, 0f, 0f, false, true, Color.white,
                hub: true, hubLinkCapacity: 120f);

            var occupancy = new GridOccupancyService();
            occupancy.TryOccupy(new BuildingInstance("h1", hub, new GridCoordinate(0, 0), 0, null));

            var near = PowerLinePlacementCost.ComputeEffectiveCost(line, new GridCoordinate(1, 0), occupancy);
            var far = PowerLinePlacementCost.ComputeEffectiveCost(line, new GridCoordinate(10, 0), occupancy);
            Assert.AreEqual(40f * (1f + 0.15f * 1f), near, 0.001f);
            Assert.AreEqual(40f * (1f + 0.15f * 10f), far, 0.001f);
            Assert.Greater(far, near);
        }

        [Test]
        public void NonPowerLine_UsesFlatCost()
        {
            var hub = ScriptableObject.CreateInstance<BuildingDefinition>();
            hub.Configure(
                "distribution_hub", "Hub", "", BuildingCategory.Network,
                120f, 0f, 30f, 0f, 0f, 0f, false, true, Color.white,
                hub: true, hubLinkCapacity: 120f);
            Assert.AreEqual(
                120f,
                PowerLinePlacementCost.ComputeEffectiveCost(hub, new GridCoordinate(5, 5), null),
                0.001f);
        }
    }
}
