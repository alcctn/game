using CleanEnergy.Buildings;
using CleanEnergy.Energy;
using CleanEnergy.Grid;
using CleanEnergy.Placement;
using NUnit.Framework;
using UnityEngine;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class PlacementLossPreviewTests
    {
        [Test]
        public void PlacementDelivery_ScalesWithHopsToVillage()
        {
            var village = ScriptableObject.CreateInstance<BuildingDefinition>();
            village.Configure(
                "village", "Village", "", BuildingCategory.Settlement,
                200f, 0f, 20f, 0f, 0f, 0f, false, true, Color.white, demand: 10f);
            var occupancy = new GridOccupancyService();
            occupancy.TryOccupy(new BuildingInstance("v1", village, new GridCoordinate(0, 0), 0, null));

            var near = TransmissionLoss.ResolveDeliveryFactorForPlacement(
                new GridCoordinate(1, 0), occupancy);
            var far = TransmissionLoss.ResolveDeliveryFactorForPlacement(
                new GridCoordinate(10, 0), occupancy);
            Assert.AreEqual(1f - 0.05f * 1f, near, 0.001f);
            Assert.AreEqual(1f - 0.05f * 10f, far, 0.001f);
            Assert.Greater(near, far);
        }

        [Test]
        public void PlacementDelivery_HubAlone_IsOne()
        {
            var hub = ScriptableObject.CreateInstance<BuildingDefinition>();
            hub.Configure(
                "distribution_hub", "Hub", "", BuildingCategory.Network,
                120f, 0f, 30f, 0f, 0f, 0f, false, true, Color.white,
                hub: true, hubLinkCapacity: 120f);
            var occupancy = new GridOccupancyService();
            occupancy.TryOccupy(new BuildingInstance("h1", hub, new GridCoordinate(0, 0), 0, null));
            Assert.AreEqual(
                1f,
                TransmissionLoss.ResolveDeliveryFactorForPlacement(new GridCoordinate(5, 0), occupancy),
                0.001f);
        }
    }
}
