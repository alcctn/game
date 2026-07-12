using CleanEnergy.Buildings;
using CleanEnergy.Grid;
using CleanEnergy.Placement;
using NUnit.Framework;
using UnityEngine;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class DemolishBuildingTests
    {
        [Test]
        public void TryDemolish_RefundsHalfAndClearsOccupancy()
        {
            var go = new GameObject("DemolishTest");
            try
            {
                var placement = go.AddComponent<PlacementController>();
                var def = ScriptableObject.CreateInstance<BuildingDefinition>();
                def.Configure(
                    "power_line", "Line", "", BuildingCategory.Network,
                    40f, 0f, 35f, 0f, 0f, 0f, false, true, Color.white,
                    hub: true, hubLinkCapacity: 40f);
                placement.Configure(null, go.transform, new[] { def }, 100f);

                var instance = new BuildingInstance("p1", def, new GridCoordinate(2, 2), 0, null);
                Assert.IsTrue(placement.Occupancy.TryOccupy(instance));
                Assert.IsTrue(placement.TryDemolish(new GridCoordinate(2, 2), out var refund));
                Assert.AreEqual(20f, refund, 0.001f);
                Assert.IsFalse(placement.Occupancy.IsOccupied(new GridCoordinate(2, 2)));
                Assert.AreEqual(120f, placement.Wallet.Money, 0.001f);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }
}
