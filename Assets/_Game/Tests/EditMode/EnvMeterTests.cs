using System.Collections.Generic;
using CleanEnergy.Buildings;
using CleanEnergy.DebugTools;
using CleanEnergy.Grid;
using CleanEnergy.Placement;
using NUnit.Framework;
using UnityEngine;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class EnvMeterTests
    {
        [Test]
        public void MeanDensity_AveragesProducerScores()
        {
            var solar = CreateProducer("small_solar");
            var occupied = new Dictionary<GridCoordinate, BuildingInstance>
            {
                { new GridCoordinate(0, 0), new BuildingInstance("a", solar, new GridCoordinate(0, 0), 0, null) }
            };

            Assert.AreEqual(0.25f, EnvironmentalImpact.MeanDensity(occupied), 0.001f);
            Assert.AreEqual(0, EnvironmentalImpact.CountHighDensitySites(occupied));
            Assert.AreEqual("Env density 0.25", EnvironmentalImpact.FormatHudMeter(occupied));
        }

        [Test]
        public void HighDensitySites_PreferCountLabel()
        {
            var solar = CreateProducer("small_solar");
            var occupied = new Dictionary<GridCoordinate, BuildingInstance>();
            var a = new BuildingInstance("a", solar, new GridCoordinate(1, 1), 0, null);
            var b = new BuildingInstance("b", solar, new GridCoordinate(1, 2), 0, null);
            var c = new BuildingInstance("c", solar, new GridCoordinate(2, 1), 0, null);
            occupied[a.Coordinate] = a;
            occupied[b.Coordinate] = b;
            occupied[c.Coordinate] = c;

            Assert.AreEqual(3, EnvironmentalImpact.CountHighDensitySites(occupied));
            Assert.AreEqual("High density sites: 3", EnvironmentalImpact.FormatHudMeter(occupied));
        }

        [Test]
        public void EmptyOccupied_ReturnsEmptyMeter()
        {
            Assert.AreEqual(0f, EnvironmentalImpact.MeanDensity(null), 0.001f);
            Assert.AreEqual(string.Empty, EnvironmentalImpact.FormatHudMeter(null));
            Assert.AreEqual(
                string.Empty,
                EnvironmentalImpact.FormatHudMeter(new Dictionary<GridCoordinate, BuildingInstance>()));
        }

        private static BuildingDefinition CreateProducer(string id)
        {
            var def = ScriptableObject.CreateInstance<BuildingDefinition>();
            def.Configure(
                id, id, "", BuildingCategory.Energy,
                100f, 10f, 20f, 0f, 0.45f, 0f, false, true, Color.white);
            return def;
        }
    }
}
