using CleanEnergy.Buildings;
using CleanEnergy.Energy;
using CleanEnergy.Grid;
using CleanEnergy.Map;
using CleanEnergy.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class ProductionBreakdownTests
    {
        [Test]
        public void BreakDown_MatchesEstimateProduct()
        {
            var grid = new GridService();
            grid.Create(4, 4, 1f, Vector3.zero);
            grid.SetSolarPotential(new GridCoordinate(1, 1), 1f);
            var settings = ScriptableObject.CreateInstance<MapGenerationSettings>();
            var def = ScriptableObject.CreateInstance<BuildingDefinition>();
            def.Configure(
                "small_solar", "Solar", "", BuildingCategory.Energy,
                120f, 10f, 20f, 0f, 0.45f, 0f, false, true, Color.white,
                buildingEfficiency: 0.8f);
            var noon = new SimulationContext(1, 0.5f, SimulationSpeed.One, 0.3f, DayPhase.Noon);
            var breakdown = ProductionEstimate.BreakDown(
                def, new GridCoordinate(1, 1), grid, settings, noon);
            var expected = def.InstalledPower
                           * breakdown.ResourcePotential
                           * breakdown.PhaseFactor
                           * breakdown.Efficiency
                           * breakdown.Maintenance
                           * breakdown.WakeFactor;
            Assert.AreEqual(expected, breakdown.Production, 0.001f);
            Assert.Greater(breakdown.Production, 0f);
        }
    }
}
