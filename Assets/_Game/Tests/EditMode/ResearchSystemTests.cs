using CleanEnergy.Buildings;
using CleanEnergy.Energy;
using CleanEnergy.Grid;
using CleanEnergy.Map;
using CleanEnergy.Placement;
using CleanEnergy.Research;
using CleanEnergy.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class ResearchSystemTests
    {
        [Test]
        public void Start_WaterWheelUnlocked_SolarAndWindLocked()
        {
            var service = CreateService();
            Assert.IsTrue(service.IsBuildingUnlocked("water_wheel"));
            Assert.IsTrue(service.IsBuildingUnlocked("village"));
            Assert.IsFalse(service.IsBuildingUnlocked("small_solar"));
            Assert.IsFalse(service.IsBuildingUnlocked("small_wind"));
            Assert.IsFalse(service.IsBuildingUnlocked("small_hydro"));
        }

        [Test]
        public void Unlock_Fails_WhenNotEnoughRp()
        {
            var service = CreateService();
            Assert.IsFalse(service.TryUnlock("solar_basic"));
            Assert.IsFalse(service.IsBuildingUnlocked("small_solar"));
        }

        [Test]
        public void Unlock_Fails_WhenPrerequisiteMissing()
        {
            var service = CreateService();
            service.Wallet.Add(100f);
            Assert.IsFalse(service.TryUnlock("solar_eff"));
            Assert.AreEqual(0f, service.GetEfficiencyBonus("small_solar"), 0.001f);
        }

        [Test]
        public void Unlock_Solar_MakesBuildingPlaceable()
        {
            var service = CreateService();
            service.Wallet.Add(15f);
            Assert.IsTrue(service.TryUnlock("solar_basic"));
            Assert.IsTrue(service.IsBuildingUnlocked("small_solar"));

            var grid = CreateGrid(4);
            grid.SetSolarPotential(new GridCoordinate(1, 1), 0.8f);
            var def = CreateSolarDef();
            var occupancy = new GridOccupancyService();
            var hub = ScriptableObject.CreateInstance<BuildingDefinition>();
            hub.Configure(
                "distribution_hub", "Hub", "", BuildingCategory.Network,
                120f, 0f, 30f, 0f, 0f, 0f, false, true, Color.white,
                linkRange: 8, hub: true, hubLinkCapacity: 120f);
            occupancy.TryOccupy(new BuildingInstance("h1", hub, new GridCoordinate(0, 0), 0, null));
            var validator = new PlacementValidator();
            var result = validator.Validate(
                def,
                new GridCoordinate(1, 1),
                grid,
                occupancy,
                new CleanEnergy.Economy.Wallet(1000f),
                service);
            Assert.IsTrue(result.IsValid);
        }

        [Test]
        public void CoverageTick_AddsResearchPoints()
        {
            var service = CreateService();
            var tracker = new ResearchProgressTracker(service);
            var balance = new EnergyBalanceResult(10f, 10f, 0f, 0f, 0f);
            Assert.AreEqual(1f, balance.CoverageRatio, 0.001f);

            tracker.OnBalanceTick(balance, activeProducerTypeCount: 1);
            Assert.AreEqual(1f, service.Wallet.Points, 0.001f);
        }

        [Test]
        public void DiversityBonus_GrantedOnce()
        {
            var service = CreateService();
            var tracker = new ResearchProgressTracker(service);
            var balance = new EnergyBalanceResult(10f, 10f, 0f, 0f, 0f);

            tracker.OnBalanceTick(balance, 2);
            tracker.OnBalanceTick(balance, 2);
            // 1+10 first tick, +1 second tick = 12
            Assert.AreEqual(12f, service.Wallet.Points, 0.001f);
        }

        [Test]
        public void EfficiencyBonus_IncreasesProduction()
        {
            var service = CreateService();
            service.Wallet.Add(100f);
            Assert.IsTrue(service.TryUnlock("solar_basic"));
            Assert.IsTrue(service.TryUnlock("solar_eff"));
            Assert.AreEqual(0.1f, service.GetEfficiencyBonus("small_solar"), 0.001f);

            var grid = CreateGrid(4);
            grid.SetSolarPotential(new GridCoordinate(1, 1), 0.5f);
            var settings = ScriptableObject.CreateInstance<MapGenerationSettings>();
            var def = CreateSolarDef();
            var instance = new BuildingInstance("p1", def, new GridCoordinate(1, 1), 0, null);
            var producer = new ResourceProducerAdapter(
                instance, grid, settings, service.GetEfficiencyBonus);

            var production = producer.GetAvailableProduction(
                new SimulationContext(1, 0.5f, SimulationSpeed.One));
            // 10 * 0.5 * (0.8 + 0.1) = 4.5
            Assert.AreEqual(4.5f, production, 0.001f);
        }

        private static ResearchService CreateService()
        {
            return new ResearchService(ResearchService.CreateRuntimeDefaultTree());
        }

        private static GridService CreateGrid(int size)
        {
            var grid = new GridService();
            grid.Create(size, size, 1f, Vector3.zero);
            for (var x = 0; x < size; x++)
            {
                for (var y = 0; y < size; y++)
                {
                    var c = new GridCoordinate(x, y);
                    grid.SetElevation(c, 1f);
                    grid.SetSlope(c, 0f, true);
                    grid.SetSolarPotential(c, 0.5f);
                    grid.SetBuildable(c, true);
                }
            }

            return grid;
        }

        private static BuildingDefinition CreateSolarDef()
        {
            var def = ScriptableObject.CreateInstance<BuildingDefinition>();
            def.Configure(
                "small_solar", "Small Solar", "test",
                BuildingCategory.Energy,
                100f, 10f, 30f, 0f, 0f, 0f,
                false, true, Color.white,
                buildingEfficiency: 0.8f);
            return def;
        }
    }
}
