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
        public void PlacementDelivery_ScalesWithPathHopsViaHub()
        {
            var village = ScriptableObject.CreateInstance<BuildingDefinition>();
            village.Configure(
                "village", "Village", "", BuildingCategory.Settlement,
                200f, 0f, 20f, 0f, 0f, 0f, false, true, Color.white, demand: 10f);
            var hub = ScriptableObject.CreateInstance<BuildingDefinition>();
            hub.Configure(
                "distribution_hub", "Hub", "", BuildingCategory.Network,
                120f, 0f, 30f, 0f, 0f, 0f, false, true, Color.white,
                hub: true, hubLinkCapacity: 120f, linkRange: 20);

            var occupancy = new GridOccupancyService();
            occupancy.TryOccupy(new BuildingInstance("v1", village, new GridCoordinate(0, 0), 0, null));
            occupancy.TryOccupy(new BuildingInstance("h1", hub, new GridCoordinate(5, 0), 0, null));

            var near = TransmissionLoss.ResolveDeliveryFactorForPlacement(
                new GridCoordinate(6, 0), occupancy);
            var far = TransmissionLoss.ResolveDeliveryFactorForPlacement(
                new GridCoordinate(10, 0), occupancy);
            // Both connect through the same hub → 2 path hops (ghost–hub–village)
            Assert.AreEqual(1f - TransmissionLoss.LossPerHop * 2f, near, 0.001f);
            Assert.AreEqual(1f - TransmissionLoss.LossPerHop * 2f, far, 0.001f);
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
