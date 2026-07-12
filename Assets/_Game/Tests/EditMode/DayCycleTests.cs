using CleanEnergy.Buildings;
using CleanEnergy.Energy;
using CleanEnergy.Grid;
using CleanEnergy.Map;
using CleanEnergy.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class DayCycleTests
    {
        [Test]
        public void TickZero_IsMorning()
        {
            var day = new DayCycleService(48);
            day.SyncFromTickIndex(1); // first tick → TickInDay 0
            Assert.AreEqual(DayPhase.Morning, day.Phase);
            Assert.AreEqual(0f, day.DayNormalized, 0.001f);
        }

        [Test]
        public void HalfDay_IsEvening()
        {
            var day = new DayCycleService(48);
            // tickIndex 25 → zeroBased 24 → TickInDay 24 → 24/48 = 0.5 → Evening
            day.SyncFromTickIndex(25);
            Assert.AreEqual(0.5f, day.DayNormalized, 0.001f);
            Assert.AreEqual(DayPhase.Evening, day.Phase);
        }

        [Test]
        public void DemandMultiplier_EveningGreaterThanNoon()
        {
            Assert.Greater(
                DayCycleService.GetDemandMultiplier(DayPhase.Evening),
                DayCycleService.GetDemandMultiplier(DayPhase.Noon));
        }

        [Test]
        public void Solar_NightProductionIsZero()
        {
            var grid = CreateGrid(4);
            grid.SetSolarPotential(new GridCoordinate(1, 1), 1f);
            var settings = ScriptableObject.CreateInstance<MapGenerationSettings>();
            var def = CreateDef("small_solar", BuildingCategory.Energy, power: 10f, efficiency: 0.8f);
            var instance = new BuildingInstance("p1", def, new GridCoordinate(1, 1), 0, null);
            var producer = new ResourceProducerAdapter(instance, grid, settings);

            var night = new SimulationContext(1, 0.5f, SimulationSpeed.One, 0.8f, DayPhase.Night);
            Assert.AreEqual(0f, producer.GetAvailableProduction(night), 0.001f);
        }

        [Test]
        public void Hydro_UnchangedAcrossPhases()
        {
            var grid = CreateGrid(4);
            var coord = new GridCoordinate(1, 1);
            grid.SetWaterData(coord, 12f, Vector2Int.zero, false);
            var settings = ScriptableObject.CreateInstance<MapGenerationSettings>();
            // StreamAccumulationThreshold default should make potential = 1 when flow >= threshold
            var def = CreateDef("water_wheel", BuildingCategory.Energy, power: 10f, efficiency: 0.8f);
            var instance = new BuildingInstance("h1", def, coord, 0, null);
            var producer = new ResourceProducerAdapter(instance, grid, settings);

            var noon = new SimulationContext(1, 0.5f, SimulationSpeed.One, 0.3f, DayPhase.Noon);
            var night = new SimulationContext(2, 0.5f, SimulationSpeed.One, 0.8f, DayPhase.Night);
            var noonProd = producer.GetAvailableProduction(noon);
            var nightProd = producer.GetAvailableProduction(night);
            Assert.AreEqual(noonProd, nightProd, 0.001f);
            Assert.Greater(noonProd, 0f);
        }

        [Test]
        public void VillageDemand_ScalesWithPhase()
        {
            var def = ScriptableObject.CreateInstance<BuildingDefinition>();
            def.Configure(
                "village", "Village", "test",
                BuildingCategory.Settlement,
                100f, 0f, 20f, 0f, 0f, 0f,
                false, true, Color.white,
                demand: 10f);
            var instance = new BuildingInstance("v1", def, new GridCoordinate(0, 0), 0, null);
            var consumer = new VillageConsumerAdapter(instance);

            var evening = new SimulationContext(1, 0.5f, SimulationSpeed.One, 0.6f, DayPhase.Evening);
            var night = new SimulationContext(2, 0.5f, SimulationSpeed.One, 0.8f, DayPhase.Night);
            Assert.AreEqual(14.5f, consumer.GetDemand(evening), 0.001f);
            Assert.AreEqual(5.5f, consumer.GetDemand(night), 0.001f);
        }

        [Test]
        public void PhaseFromNormalized_Boundaries()
        {
            Assert.AreEqual(DayPhase.Morning, DayCycleService.PhaseFromNormalized(0f));
            Assert.AreEqual(DayPhase.Noon, DayCycleService.PhaseFromNormalized(0.25f));
            Assert.AreEqual(DayPhase.Evening, DayCycleService.PhaseFromNormalized(0.5f));
            Assert.AreEqual(DayPhase.Night, DayCycleService.PhaseFromNormalized(0.75f));
        }

        [Test]
        public void WindFactor_TableMatchesSpec()
        {
            Assert.AreEqual(0.85f, DayCycleService.GetWindFactor(DayPhase.Morning), 0.001f);
            Assert.AreEqual(0.55f, DayCycleService.GetWindFactor(DayPhase.Noon), 0.001f);
            Assert.AreEqual(1.15f, DayCycleService.GetWindFactor(DayPhase.Evening), 0.001f);
            Assert.AreEqual(1.35f, DayCycleService.GetWindFactor(DayPhase.Night), 0.001f);
        }

        [Test]
        public void Wind_NightGreaterThanNoon()
        {
            var grid = CreateGrid(4);
            grid.SetWindPotential(new GridCoordinate(1, 1), 1f);
            var settings = ScriptableObject.CreateInstance<MapGenerationSettings>();
            var def = CreateDef("small_wind", BuildingCategory.Energy, power: 10f, efficiency: 0.8f);
            var instance = new BuildingInstance("w1", def, new GridCoordinate(1, 1), 0, null);
            var producer = new ResourceProducerAdapter(instance, grid, settings);

            var noon = new SimulationContext(1, 0.5f, SimulationSpeed.One, 0.3f, DayPhase.Noon);
            var night = new SimulationContext(2, 0.5f, SimulationSpeed.One, 0.8f, DayPhase.Night);
            var noonProd = producer.GetAvailableProduction(noon);
            var nightProd = producer.GetAvailableProduction(night);

            Assert.AreEqual(10f * 0.8f * 0.55f, noonProd, 0.001f);
            Assert.AreEqual(10f * 0.8f * 1.35f, nightProd, 0.001f);
            Assert.Greater(nightProd, noonProd);
        }

        [Test]
        public void Solar_StillZeroAtNight()
        {
            var grid = CreateGrid(4);
            grid.SetSolarPotential(new GridCoordinate(1, 1), 1f);
            var settings = ScriptableObject.CreateInstance<MapGenerationSettings>();
            var def = CreateDef("small_solar", BuildingCategory.Energy, power: 10f, efficiency: 0.8f);
            var instance = new BuildingInstance("s1", def, new GridCoordinate(1, 1), 0, null);
            var producer = new ResourceProducerAdapter(instance, grid, settings);

            var night = new SimulationContext(1, 0.5f, SimulationSpeed.One, 0.8f, DayPhase.Night);
            Assert.AreEqual(0f, producer.GetAvailableProduction(night), 0.001f);
            Assert.AreEqual(0f, new SimulationContext(1, 0.5f, SimulationSpeed.One, 0.8f, DayPhase.Night).DaylightFactor, 0.001f);
            Assert.AreEqual(1.35f, new SimulationContext(1, 0.5f, SimulationSpeed.One, 0.8f, DayPhase.Night).WindFactor, 0.001f);
        }

        private static GridService CreateGrid(int size)
        {
            var grid = new GridService();
            grid.Create(size, size, 1f, Vector3.zero);
            return grid;
        }

        private static BuildingDefinition CreateDef(
            string id,
            BuildingCategory category,
            float power,
            float efficiency)
        {
            var def = ScriptableObject.CreateInstance<BuildingDefinition>();
            def.Configure(
                id, id, id, category,
                100f, power, 30f, 0f, 0f, 0f,
                false, true, Color.white,
                buildingEfficiency: efficiency);
            return def;
        }
    }
}
