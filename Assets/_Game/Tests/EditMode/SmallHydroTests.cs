using CleanEnergy.Buildings;
using CleanEnergy.Economy;
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
    public sealed class SmallHydroTests
    {
        [Test]
        public void LowWaterFlow_FailsPlacement()
        {
            var grid = CreateShoreGrid(flow: 10f);
            var def = CreateHydroDef();
            var result = new PlacementValidator().Validate(
                def,
                new GridCoordinate(1, 1),
                grid,
                new GridOccupancyService(),
                new Wallet(1000f),
                UnlockedResearch());

            Assert.IsFalse(result.IsValid);
            Assert.That(string.Join(" ", result.FailureReasons), Does.Contain("Water flow").IgnoreCase);
        }

        [Test]
        public void HighWaterFlow_PassesPlacement()
        {
            var grid = CreateShoreGrid(flow: 25f);
            var def = CreateHydroDef();
            var occupancy = SeedHubNear(new GridCoordinate(0, 0));
            var result = new PlacementValidator().Validate(
                def,
                new GridCoordinate(1, 1),
                grid,
                occupancy,
                new Wallet(1000f),
                UnlockedResearch());

            Assert.IsTrue(result.IsValid, string.Join("; ", result.FailureReasons));
        }

        [Test]
        public void Production_ScalesWithHydroPotential()
        {
            var grid = CreateShoreGrid(flow: 12f);
            var settings = ScriptableObject.CreateInstance<MapGenerationSettings>();
            var def = CreateHydroDef();
            var instance = new BuildingInstance("h1", def, new GridCoordinate(1, 1), 0, null);
            var producer = new ResourceProducerAdapter(instance, grid, settings);

            // threshold default 12 → potential 1.0; 18 * 1.0 * 0.85 = 15.3
            var production = producer.GetAvailableProduction(
                new SimulationContext(1, 0.5f, SimulationSpeed.One));
            Assert.AreEqual(15.3f, production, 0.05f);
        }

        [Test]
        public void Locked_UntilHydroTurbineUnlocked()
        {
            var service = new ResearchService(ResearchService.CreateRuntimeDefaultTree());
            Assert.IsFalse(service.IsBuildingUnlocked("small_hydro"));

            var grid = CreateShoreGrid(flow: 25f);
            var def = CreateHydroDef();
            var locked = new PlacementValidator().Validate(
                def,
                new GridCoordinate(1, 1),
                grid,
                new GridOccupancyService(),
                new Wallet(1000f),
                service);
            Assert.IsFalse(locked.IsValid);
            Assert.That(string.Join(" ", locked.FailureReasons), Does.Contain("Technology").IgnoreCase);

            service.Wallet.Add(30f);
            Assert.IsTrue(service.TryUnlock("hydro_turbine"));
            Assert.IsTrue(service.IsBuildingUnlocked("small_hydro"));

            var unlocked = new PlacementValidator().Validate(
                def,
                new GridCoordinate(1, 1),
                grid,
                SeedHubNear(new GridCoordinate(0, 0)),
                new Wallet(1000f),
                service);
            Assert.IsTrue(unlocked.IsValid, string.Join("; ", unlocked.FailureReasons));
        }

        private static GridOccupancyService SeedHubNear(GridCoordinate at)
        {
            var occupancy = new GridOccupancyService();
            var hub = ScriptableObject.CreateInstance<BuildingDefinition>();
            hub.Configure(
                "distribution_hub", "Hub", "", BuildingCategory.Network,
                120f, 0f, 30f, 0f, 0f, 0f, false, true, Color.white,
                linkRange: 8, hub: true, hubLinkCapacity: 120f);
            occupancy.TryOccupy(new BuildingInstance("seed_hub", hub, at, 0, null));
            return occupancy;
        }

        private static GridService CreateShoreGrid(float flow)
        {
            var grid = new GridService();
            grid.Create(4, 4, 1f, Vector3.zero);
            for (var x = 0; x < 4; x++)
            {
                for (var y = 0; y < 4; y++)
                {
                    var c = new GridCoordinate(x, y);
                    grid.SetElevation(c, 1f);
                    grid.SetSlope(c, 0f, true);
                    grid.SetBuildable(c, true);
                    grid.SetWaterData(c, 0f, Vector2Int.zero, false);
                }
            }

            // Shore cell (1,1) next to water (2,1) carrying flow.
            grid.SetWaterData(new GridCoordinate(2, 1), flow, Vector2Int.left, true);
            grid.SetBuildable(new GridCoordinate(2, 1), false);
            grid.SetWaterData(new GridCoordinate(1, 1), flow, Vector2Int.zero, false);
            return grid;
        }

        private static BuildingDefinition CreateHydroDef()
        {
            var def = ScriptableObject.CreateInstance<BuildingDefinition>();
            def.Configure(
                "small_hydro", "Small Hydro", "t",
                BuildingCategory.Energy,
                220f, 18f, 35f, 20f, 0f, 0f,
                true, false, Color.blue,
                buildingEfficiency: 0.85f);
            return def;
        }

        private static ResearchService UnlockedResearch()
        {
            var service = new ResearchService(ResearchService.CreateRuntimeDefaultTree());
            service.Wallet.Add(30f);
            service.TryUnlock("hydro_turbine");
            return service;
        }
    }
}
