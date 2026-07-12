using CleanEnergy.Buildings;
using CleanEnergy.Energy;
using CleanEnergy.Grid;
using CleanEnergy.Map;
using CleanEnergy.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class PlacementYieldTests
    {
        [Test]
        public void Estimate_MatchesAdapterProduction()
        {
            var grid = CreateGrid(4);
            grid.SetSolarPotential(new GridCoordinate(1, 1), 1f);
            var settings = ScriptableObject.CreateInstance<MapGenerationSettings>();
            var def = ScriptableObject.CreateInstance<BuildingDefinition>();
            def.Configure(
                "small_solar", "Solar", "", BuildingCategory.Energy,
                120f, 10f, 20f, 0f, 0.45f, 0f, false, true, Color.white,
                buildingEfficiency: 0.8f);

            var instance = new BuildingInstance("s1", def, new GridCoordinate(1, 1), 0, null);
            var producer = new ResourceProducerAdapter(instance, grid, settings);
            var noon = new SimulationContext(1, 0.5f, SimulationSpeed.One, 0.3f, DayPhase.Noon);

            var fromAdapter = producer.GetAvailableProduction(noon);
            var fromEstimate = ProductionEstimate.Estimate(
                def, new GridCoordinate(1, 1), grid, settings, noon);

            Assert.AreEqual(fromAdapter, fromEstimate, 0.001f);
            Assert.Greater(fromEstimate, 0f);
        }

        [Test]
        public void Estimate_NonProducer_IsZero()
        {
            var def = ScriptableObject.CreateInstance<BuildingDefinition>();
            def.Configure(
                "village", "Village", "", BuildingCategory.Settlement,
                200f, 0f, 20f, 0f, 0f, 0f, false, true, Color.white,
                demand: 10f);
            var noon = new SimulationContext(1, 0.5f, SimulationSpeed.One, 0.3f, DayPhase.Noon);
            Assert.AreEqual(
                0f,
                ProductionEstimate.Estimate(def, new GridCoordinate(0, 0), CreateGrid(2), null, noon),
                0.001f);
        }

        private static GridService CreateGrid(int size)
        {
            var grid = new GridService();
            grid.Create(size, size, 1f, Vector3.zero);
            return grid;
        }
    }
}
