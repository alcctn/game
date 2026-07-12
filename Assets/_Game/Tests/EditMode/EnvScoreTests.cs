using System.Collections.Generic;
using CleanEnergy.Buildings;
using CleanEnergy.DebugTools;
using CleanEnergy.Economy;
using CleanEnergy.Grid;
using CleanEnergy.Placement;
using NUnit.Framework;
using UnityEngine;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class EnvScoreTests
    {
        [Test]
        public void Score_IsZeroToOne_FromProducerDensity()
        {
            var solar = CreateProducer("small_solar");
            var occupancy = new GridOccupancyService();
            occupancy.TryOccupy(new BuildingInstance("s1", solar, new GridCoordinate(5, 5), 0, null));

            var score = EnvironmentalImpact.ScoreAt(new GridCoordinate(5, 5), occupancy);
            Assert.AreEqual(0.25f, score, 0.001f);
            Assert.GreaterOrEqual(score, 0f);
            Assert.LessOrEqual(score, 1f);
        }

        [Test]
        public void HighDensity_AppliesUpkeepMultiplier()
        {
            Assert.AreEqual(1f, EnvironmentalImpact.UpkeepMultiplierForDensity(0.5f), 0.001f);
            Assert.AreEqual(1f, EnvironmentalImpact.UpkeepMultiplierForDensity(0.6f), 0.001f);
            Assert.AreEqual(
                EnvironmentalImpact.HighDensityUpkeepMultiplier,
                EnvironmentalImpact.UpkeepMultiplierForDensity(0.61f),
                0.001f);
        }

        [Test]
        public void UpkeepService_RaisesProducerCost_WhenDensityHigh()
        {
            var solar = CreateProducer("small_solar", upkeep: 10f);
            var occupied = new Dictionary<GridCoordinate, BuildingInstance>();
            // 3 producers in Moore neighborhood of (1,1) → density 0.75 > 0.6
            var a = new BuildingInstance("a", solar, new GridCoordinate(1, 1), 0, null);
            var b = new BuildingInstance("b", solar, new GridCoordinate(1, 2), 0, null);
            var c = new BuildingInstance("c", solar, new GridCoordinate(2, 1), 0, null);
            occupied[a.Coordinate] = a;
            occupied[b.Coordinate] = b;
            occupied[c.Coordinate] = c;

            var service = new UpkeepService();
            var total = service.CalculateTotal(occupied);
            // Each gets ×1.15 (all see 3 producers in neighborhood)
            Assert.AreEqual(10f * 1.15f * 3f, total, 0.01f);
        }

        [Test]
        public void UpkeepService_NoPenalty_BelowThreshold()
        {
            var solar = CreateProducer("small_solar", upkeep: 10f);
            var occupied = new Dictionary<GridCoordinate, BuildingInstance>
            {
                { new GridCoordinate(0, 0), new BuildingInstance("a", solar, new GridCoordinate(0, 0), 0, null) }
            };

            var service = new UpkeepService();
            Assert.AreEqual(10f, service.CalculateTotal(occupied), 0.001f);
        }

        private static BuildingDefinition CreateProducer(string id, float upkeep = 1f)
        {
            var def = ScriptableObject.CreateInstance<BuildingDefinition>();
            def.Configure(
                id, id, "", BuildingCategory.Energy,
                100f, 10f, 20f, 0f, 0.45f, 0f, false, true, Color.white,
                upkeepCost: upkeep);
            return def;
        }
    }
}
