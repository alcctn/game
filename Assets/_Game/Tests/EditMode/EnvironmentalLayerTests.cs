using CleanEnergy.Buildings;
using CleanEnergy.DebugTools;
using CleanEnergy.Grid;
using CleanEnergy.Placement;
using NUnit.Framework;
using UnityEngine;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class EnvironmentalLayerTests
    {
        [Test]
        public void Score_ScalesWithNearbyProducers()
        {
            var solar = ScriptableObject.CreateInstance<BuildingDefinition>();
            solar.Configure(
                "small_solar", "Solar", "", BuildingCategory.Energy,
                120f, 10f, 20f, 0f, 0.45f, 0f, false, true, Color.white);
            var occupancy = new GridOccupancyService();
            occupancy.TryOccupy(new BuildingInstance("s1", solar, new GridCoordinate(1, 1), 0, null));

            var score = EnvironmentalImpact.ScoreAt(new GridCoordinate(1, 1), occupancy);
            Assert.Greater(score, 0f);
            Assert.LessOrEqual(score, 1f);
        }

        [Test]
        public void Hotkey_F10_MapsEnvironmental()
        {
            Assert.IsTrue(DebugViewHotkeys.TryMapKey(KeyCode.F10, out var mode));
            Assert.AreEqual(DebugViewMode.Environmental, mode);
        }
    }
}
