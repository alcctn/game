using CleanEnergy.Buildings;
using CleanEnergy.Energy;
using CleanEnergy.Grid;
using CleanEnergy.Placement;
using NUnit.Framework;
using UnityEngine;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class WindWakeTests
    {
        [Test]
        public void ClampFactor_AppliesPenaltyAndFloor()
        {
            Assert.AreEqual(1f, WindWakeFactor.ClampFactor(0), 0.001f);
            Assert.AreEqual(0.88f, WindWakeFactor.ClampFactor(1), 0.001f);
            Assert.AreEqual(0.4f, WindWakeFactor.ClampFactor(10), 0.001f);
        }

        [Test]
        public void Compute_IsolatedTurbine_NoPenalty()
        {
            var def = ScriptableObject.CreateInstance<BuildingDefinition>();
            def.Configure(
                "small_wind", "Small Wind", "", BuildingCategory.Energy,
                150f, 14f, 28f, 0f, 0f, 0.4f, false, true, Color.white,
                sameTypeSpacing: 3);

            var occupancy = new GridOccupancyService();
            occupancy.TryOccupy(new BuildingInstance("a", def, new GridCoordinate(5, 5), 0, null));

            var wake = WindWakeFactor.Compute(def, new GridCoordinate(5, 5), occupancy, "a");
            Assert.AreEqual(1f, wake, 0.001f);
        }

        [Test]
        public void Compute_NeighborWithinChebyshev_Penalizes()
        {
            var def = ScriptableObject.CreateInstance<BuildingDefinition>();
            def.Configure(
                "small_wind", "Small Wind", "", BuildingCategory.Energy,
                150f, 14f, 28f, 0f, 0f, 0.4f, false, true, Color.white,
                sameTypeSpacing: 3);

            var occupancy = new GridOccupancyService();
            occupancy.TryOccupy(new BuildingInstance("a", def, new GridCoordinate(0, 0), 0, null));
            occupancy.TryOccupy(new BuildingInstance("b", def, new GridCoordinate(2, 2), 0, null));

            var wake = WindWakeFactor.Compute(def, new GridCoordinate(0, 0), occupancy, "a");
            Assert.AreEqual(0.88f, wake, 0.001f);
        }

        [Test]
        public void Compute_NonWind_AlwaysOne()
        {
            var def = ScriptableObject.CreateInstance<BuildingDefinition>();
            def.Configure(
                "small_solar", "Solar", "", BuildingCategory.Energy,
                120f, 12f, 20f, 0f, 0.45f, 0f, false, true, Color.white);

            Assert.AreEqual(1f, WindWakeFactor.Compute(def, new GridCoordinate(0, 0), null), 0.001f);
        }
    }
}
