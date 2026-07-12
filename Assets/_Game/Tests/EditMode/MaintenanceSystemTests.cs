using System.Collections.Generic;
using CleanEnergy.Buildings;
using CleanEnergy.Energy;
using CleanEnergy.Grid;
using CleanEnergy.Maintenance;
using CleanEnergy.Map;
using CleanEnergy.Save;
using CleanEnergy.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class MaintenanceSystemTests
    {
        [Test]
        public void WithoutDepot_ProducerDecaysTowardFloor()
        {
            var service = new MaintenanceService();
            var producer = CreateProducerInstance("water_wheel", new GridCoordinate(0, 0), 1f);
            var occupied = new Dictionary<GridCoordinate, BuildingInstance>
            {
                { producer.Coordinate, producer }
            };

            for (var i = 0; i < 20; i++)
            {
                service.ProcessTick(occupied);
            }

            Assert.Less(producer.MaintenanceLevel, 1f);
            Assert.GreaterOrEqual(producer.MaintenanceLevel, MaintenanceService.MinLevel - 0.0001f);
        }

        [Test]
        public void WithDepotInRange_RestoresMaintenance()
        {
            var service = new MaintenanceService();
            var producer = CreateProducerInstance("water_wheel", new GridCoordinate(0, 0), 0.5f);
            var depot = CreateDepotInstance(new GridCoordinate(2, 0));
            var occupied = new Dictionary<GridCoordinate, BuildingInstance>
            {
                { producer.Coordinate, producer },
                { depot.Coordinate, depot }
            };

            for (var i = 0; i < 10; i++)
            {
                service.ProcessTick(occupied);
            }

            Assert.Greater(producer.MaintenanceLevel, 0.5f);
            Assert.LessOrEqual(producer.MaintenanceLevel, 1f);
        }

        [Test]
        public void Production_ScalesWithMaintenanceLevel()
        {
            var grid = new GridService();
            grid.Create(4, 4, 1f, Vector3.zero);
            grid.SetSolarPotential(new GridCoordinate(1, 1), 0.5f);
            var settings = ScriptableObject.CreateInstance<MapGenerationSettings>();
            var def = ScriptableObject.CreateInstance<BuildingDefinition>();
            def.Configure(
                "small_solar", "solar", "t",
                BuildingCategory.Energy,
                100f, 10f, 30f, 0f, 0f, 0f,
                false, true, Color.white,
                buildingEfficiency: 0.8f);
            var instance = new BuildingInstance("p1", def, new GridCoordinate(1, 1), 0, null)
            {
                MaintenanceLevel = 0.5f
            };
            var producer = new ResourceProducerAdapter(instance, grid, settings);
            var noon = new SimulationContext(1, 0.5f, SimulationSpeed.One, 0.3f, DayPhase.Noon);
            // 10 * 0.5 * 0.8 * 0.5 = 2.0
            Assert.AreEqual(2f, producer.GetAvailableProduction(noon), 0.001f);
        }

        [Test]
        public void SaveData_RoundTripsMaintenanceLevel()
        {
            var original = new BuildingSaveData
            {
                definitionId = "water_wheel",
                x = 1,
                y = 2,
                maintenanceLevel = 0.73f
            };
            var wrapper = new GameSaveData
            {
                buildings = new[] { original }
            };
            var service = new SaveGameService();
            var loaded = service.FromJson(service.ToJson(wrapper));
            Assert.AreEqual(0.73f, loaded.buildings[0].maintenanceLevel, 0.001f);
        }

        [Test]
        public void LowMaintenance_Counted()
        {
            var service = new MaintenanceService();
            var producer = CreateProducerInstance("small_wind", new GridCoordinate(0, 0), 0.65f);
            var occupied = new Dictionary<GridCoordinate, BuildingInstance>
            {
                { producer.Coordinate, producer }
            };
            service.ProcessTick(occupied);
            Assert.AreEqual(1, service.LowMaintenanceCount);
        }

        private static BuildingInstance CreateProducerInstance(
            string id,
            GridCoordinate coordinate,
            float maintenance)
        {
            var def = ScriptableObject.CreateInstance<BuildingDefinition>();
            def.Configure(
                id, id, id,
                BuildingCategory.Energy,
                100f, 10f, 30f, 0f, 0f, 0f,
                false, true, Color.white);
            return new BuildingInstance(id + "_1", def, coordinate, 0, null)
            {
                MaintenanceLevel = maintenance
            };
        }

        private static BuildingInstance CreateDepotInstance(GridCoordinate coordinate)
        {
            var def = ScriptableObject.CreateInstance<BuildingDefinition>();
            def.Configure(
                MaintenanceService.DepotBuildingId, "Depot", "d",
                BuildingCategory.Service,
                160f, 0f, 30f, 0f, 0f, 0f,
                false, true, Color.white,
                linkRange: 5);
            return new BuildingInstance("depot_1", def, coordinate, 0, null);
        }
    }
}
