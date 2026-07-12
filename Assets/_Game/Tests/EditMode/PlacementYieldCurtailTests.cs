using CleanEnergy.Buildings;
using CleanEnergy.Energy;
using CleanEnergy.Grid;
using CleanEnergy.Placement;
using NUnit.Framework;
using UnityEngine;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class PlacementYieldCurtailTests
    {
        [Test]
        public void ResolveForPlacement_WithoutLoad_IsZero()
        {
            var solar = CreateSolar();
            var hub = ScriptableObject.CreateInstance<BuildingDefinition>();
            hub.Configure(
                "distribution_hub", "Hub", "", BuildingCategory.Network,
                120f, 0f, 30f, 0f, 0f, 0f, false, true, Color.white,
                hub: true, hubLinkCapacity: 120f);

            var occupancy = new GridOccupancyService();
            occupancy.TryOccupy(new BuildingInstance("h1", hub, new GridCoordinate(0, 0), 0, null));

            Assert.AreEqual(
                0f,
                NetworkLoadFactor.ResolveForPlacement(new GridCoordinate(1, 0), solar, occupancy),
                0.001f);
        }

        [Test]
        public void ResolveForPlacement_WithVillageInRange_IsOne()
        {
            var solar = CreateSolar(linkRange: 4);
            var village = ScriptableObject.CreateInstance<BuildingDefinition>();
            village.Configure(
                "village", "Village", "", BuildingCategory.Settlement,
                200f, 0f, 20f, 0f, 0f, 0f, false, true, Color.white, demand: 10f);

            var occupancy = new GridOccupancyService();
            occupancy.TryOccupy(new BuildingInstance("v1", village, new GridCoordinate(0, 0), 0, null));

            Assert.AreEqual(
                1f,
                NetworkLoadFactor.ResolveForPlacement(new GridCoordinate(2, 0), solar, occupancy),
                0.001f);
            Assert.AreEqual(
                0f,
                NetworkLoadFactor.ResolveForPlacement(new GridCoordinate(5, 0), solar, occupancy),
                0.001f);
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
    }
}
