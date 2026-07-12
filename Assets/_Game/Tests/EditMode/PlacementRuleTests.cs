using System.Collections.Generic;
using CleanEnergy.Buildings;
using CleanEnergy.Economy;
using CleanEnergy.Grid;
using CleanEnergy.Placement;
using NUnit.Framework;
using UnityEngine;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class PlacementRuleTests
    {
        [Test]
        public void OccupiedCell_FailsOccupancyRule()
        {
            var grid = CreateFlatGrid(4);
            var occupancy = new GridOccupancyService();
            var definition = CreateBuilding("small_solar", solarMin: 0.1f);
            var coordinate = new GridCoordinate(1, 1);
            var fake = new BuildingInstance("busy", definition, coordinate, 0, null);
            occupancy.TryOccupy(fake);

            var reasons = new List<string>();
            var passed = new GridOccupancyRule().Evaluate(
                new PlacementContext(definition, coordinate, grid, occupancy, new Wallet(1000f)),
                reasons);

            Assert.IsFalse(passed);
            Assert.IsNotEmpty(reasons);
        }

        [Test]
        public void FlatBuildableHighSolar_PassesSolarRules()
        {
            var grid = CreateFlatGrid(4);
            grid.SetSolarPotential(new GridCoordinate(2, 2), 0.8f);
            grid.SetBuildable(new GridCoordinate(2, 2), true);
            var definition = CreateBuilding("small_solar", solarMin: 0.45f, maxSlope: 30f);
            var validator = new PlacementValidator();
            var result = validator.Validate(
                definition,
                new GridCoordinate(2, 2),
                grid,
                new GridOccupancyService(),
                new Wallet(1000f));

            Assert.IsTrue(result.IsValid, string.Join("; ", result.FailureReasons));
        }

        [Test]
        public void WaterWheel_WithoutAdjacentWater_Fails()
        {
            var grid = CreateFlatGrid(4);
            var definition = CreateBuilding("water_wheel", waterMin: 5f, adjacentWater: true, requireBuildable: false);
            var validator = new PlacementValidator();
            var result = validator.Validate(
                definition,
                new GridCoordinate(1, 1),
                grid,
                new GridOccupancyService(),
                new Wallet(1000f));

            Assert.IsFalse(result.IsValid);
            Assert.That(string.Join(" ", result.FailureReasons), Does.Contain("adjacent").IgnoreCase);
        }

        [Test]
        public void InsufficientMoney_FailsAffordability()
        {
            var grid = CreateFlatGrid(4);
            grid.SetSolarPotential(new GridCoordinate(0, 0), 1f);
            var definition = CreateBuilding("small_solar", cost: 500f, solarMin: 0.1f);
            var validator = new PlacementValidator();
            var result = validator.Validate(
                definition,
                new GridCoordinate(0, 0),
                grid,
                new GridOccupancyService(),
                new Wallet(10f));

            Assert.IsFalse(result.IsValid);
            Assert.That(string.Join(" ", result.FailureReasons), Does.Contain("money").IgnoreCase);
        }

        [Test]
        public void SuccessfulOccupy_SetsLookup()
        {
            var definition = CreateBuilding("small_wind", windMin: 0f);
            var occupancy = new GridOccupancyService();
            var coordinate = new GridCoordinate(3, 3);
            var instance = new BuildingInstance("wind_1", definition, coordinate, 0, null);

            Assert.IsTrue(occupancy.TryOccupy(instance));
            Assert.IsTrue(occupancy.IsOccupied(coordinate));
            Assert.IsTrue(occupancy.TryGet(coordinate, out var found));
            Assert.AreEqual("wind_1", found.InstanceId);
        }

        [Test]
        public void Validator_WritesOccupyingIdViaGridService()
        {
            var grid = CreateFlatGrid(4);
            grid.SetSolarPotential(new GridCoordinate(1, 1), 1f);
            var coordinate = new GridCoordinate(1, 1);
            grid.SetOccupyingBuildingId(coordinate, "solar_9");
            Assert.AreEqual("solar_9", grid.GetCell(coordinate).OccupyingBuildingId);
        }

        private static GridService CreateFlatGrid(int size)
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
                    grid.SetWindPotential(c, 0.5f);
                    grid.SetBuildable(c, true);
                }
            }

            return grid;
        }

        private static BuildingDefinition CreateBuilding(
            string id,
            float cost = 100f,
            float maxSlope = 30f,
            float waterMin = 0f,
            float solarMin = 0f,
            float windMin = 0f,
            bool adjacentWater = false,
            bool requireBuildable = true)
        {
            var def = ScriptableObject.CreateInstance<BuildingDefinition>();
            def.Configure(
                id, id, id, BuildingCategory.Energy,
                cost, 10f, maxSlope, waterMin, solarMin, windMin,
                adjacentWater, requireBuildable, Color.white);
            return def;
        }
    }
}
