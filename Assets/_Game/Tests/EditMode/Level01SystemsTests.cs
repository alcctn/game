using CleanEnergy.Buildings;
using CleanEnergy.Economy;
using CleanEnergy.Grid;
using CleanEnergy.Placement;
using CleanEnergy.Scenario;
using CleanEnergy.Settlements;
using CleanEnergy.UI;
using CleanEnergy.Workers;
using NUnit.Framework;
using UnityEngine;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class Level01SystemsTests
    {
        [Test]
        public void AutoConnectionCost_ScalesWithDistance()
        {
            var settlement = new ActiveSettlementService();
            settlement.Set(new GridCoordinate(10, 10), 10);
            var def = ScriptableObject.CreateInstance<BuildingDefinition>();
            def.Configure(
                "water_wheel", "WW", "", BuildingCategory.Energy,
                80f, 8f, 25f, 0f, 0f, 0f, true, false, Color.blue);
            var cost = AutoConnectionCost.Compute(def, new GridCoordinate(10, 15), settlement, 2f, true);
            Assert.AreEqual(10f, cost, 0.001f);
        }

        [Test]
        public void SettlementRadiusRule_RejectsOutside()
        {
            var settlement = new ActiveSettlementService();
            settlement.Set(new GridCoordinate(5, 5), 3);
            var rule = new SettlementRadiusRule(settlement);
            var def = ScriptableObject.CreateInstance<BuildingDefinition>();
            def.Configure(
                "water_wheel", "WW", "", BuildingCategory.Energy,
                80f, 8f, 25f, 0f, 0f, 0f, true, false, Color.blue);
            var grid = CreateGrid(16);
            var reasons = new System.Collections.Generic.List<string>();
            var ctx = new PlacementContext(
                def, new GridCoordinate(5, 10), grid, new GridOccupancyService(), new Wallet(500f),
                settlement: settlement);
            Assert.IsFalse(rule.Evaluate(ctx, reasons));
            Assert.That(reasons, Does.Contain(SettlementRadiusRule.FailReasonEn));
        }

        [Test]
        public void WorkerRequirementRule_RequiresEngineer()
        {
            var pool = new WorkerPool();
            var rule = new WorkerRequirementRule(pool);
            var def = ScriptableObject.CreateInstance<BuildingDefinition>();
            def.Configure(
                "water_wheel", "WW", "", BuildingCategory.Energy,
                80f, 8f, 25f, 0f, 0f, 0f, true, false, Color.blue);
            def.SetWorkerRequirements(1, 0);
            var reasons = new System.Collections.Generic.List<string>();
            var ctx = new PlacementContext(
                def, new GridCoordinate(0, 0), CreateGrid(4), new GridOccupancyService(), new Wallet(500f),
                workers: pool);
            Assert.IsFalse(rule.Evaluate(ctx, reasons));
            pool.TryAdd(WorkerType.Engineer);
            reasons.Clear();
            Assert.IsTrue(rule.Evaluate(ctx, reasons));
        }

        [Test]
        public void Level01_RestrictsBuildMenuToEnergy()
        {
            var level = LevelDefinition.CreateRuntimeDefault();
            Assert.IsTrue(level.RestrictBuildMenuToEnergy);
            Assert.IsTrue(BuildingPlacementUI.IsCategoryAllowed(BuildingCategory.Energy, true));
            Assert.IsFalse(BuildingPlacementUI.IsCategoryAllowed(BuildingCategory.Network, true));
            Assert.IsFalse(BuildingPlacementUI.IsCategoryAllowed(BuildingCategory.Storage, true));
            Assert.IsFalse(BuildingPlacementUI.IsCategoryAllowed(BuildingCategory.Settlement, true));
            Assert.IsFalse(BuildingPlacementUI.IsCategoryAllowed(BuildingCategory.Service, true));
        }

        [Test]
        public void AllowedBuildCategoryRule_BlocksNetworkWhenRestricted()
        {
            var level = LevelDefinition.CreateRuntimeDefault();
            var rule = new AllowedBuildCategoryRule();
            var network = ScriptableObject.CreateInstance<BuildingDefinition>();
            network.Configure(
                "power_line", "Power Line", "", BuildingCategory.Network,
                40f, 0f, 0f, 0f, 0f, 0f, false, false, Color.yellow);
            var reasons = new System.Collections.Generic.List<string>();
            var ctx = new PlacementContext(
                network, new GridCoordinate(0, 0), CreateGrid(4), new GridOccupancyService(), new Wallet(500f),
                level: level);
            Assert.IsFalse(rule.Evaluate(ctx, reasons));
            Assert.That(reasons, Does.Contain(AllowedBuildCategoryRule.FailReason));

            var energy = ScriptableObject.CreateInstance<BuildingDefinition>();
            energy.Configure(
                "water_wheel", "WW", "", BuildingCategory.Energy,
                80f, 8f, 25f, 0f, 0f, 0f, true, false, Color.blue);
            reasons.Clear();
            ctx = new PlacementContext(
                energy, new GridCoordinate(0, 0), CreateGrid(4), new GridOccupancyService(), new Wallet(500f),
                level: level);
            Assert.IsTrue(rule.Evaluate(ctx, reasons));
        }

        [Test]
        public void LevelProgress_WeightsAndRewards()
        {
            var level = LevelDefinition.CreateRuntimeDefault();
            var progress = new LevelProgressService(level);
            var wallet = new Wallet(0f);
            var pool = new WorkerPool();
            pool.TryAdd(WorkerType.Engineer);
            progress.Evaluate(pool, new GridOccupancyService(), 0f, wallet);
            Assert.IsTrue(progress.State.EngineerComplete);
            Assert.Greater(progress.State.ProgressPercent, 0f);
            Assert.AreEqual(0f, wallet.Money, 0.001f);

            pool.TryAdd(WorkerType.Technician);
            progress.Evaluate(pool, new GridOccupancyService(), 0f, wallet);
            Assert.IsTrue(progress.State.TechnicianComplete);
            Assert.AreEqual(level.RewardTechnicianCreated, wallet.Money, 0.001f);
        }

        [Test]
        public void FirstPacket_FitsStartingMoney()
        {
            var level = LevelDefinition.CreateRuntimeDefault();
            var first = level.EngineerHireCost + 80f + 7f * level.ConnectionCostPerCell;
            Assert.LessOrEqual(first, level.StartingMoney);
        }

        [Test]
        public void MaxSameTypeCountRule_BlocksSecondWind()
        {
            var def = ScriptableObject.CreateInstance<BuildingDefinition>();
            def.Configure(
                "small_wind", "Small Wind", "", BuildingCategory.Energy,
                100f, 14f, 28f, 0f, 0f, 0.25f, false, true, Color.yellow);
            def.SetMaxSameTypeCount(1);

            var occupancy = new GridOccupancyService();
            var placed = new BuildingInstance(
                "w1", def, new GridCoordinate(0, 0), 0, null);
            Assert.IsTrue(occupancy.TryOccupy(placed));

            var rule = new MaxSameTypeCountRule();
            var reasons = new System.Collections.Generic.List<string>();
            var ctx = new PlacementContext(
                def, new GridCoordinate(5, 5), CreateGrid(8), occupancy, new Wallet(500f));
            Assert.IsFalse(rule.Evaluate(ctx, reasons));
            Assert.That(reasons[0], Does.Contain("Only 1"));
        }

        [Test]
        public void TechnicianHire_RequiresWaterGate()
        {
            var level = LevelDefinition.CreateRuntimeDefault();
            var wallet = new Wallet(500f);
            var waterReady = false;
            var workers = new WorkerService();
            workers.Configure(level, wallet, () => waterReady);

            Assert.IsFalse(workers.CanHireTechnician());
            Assert.IsFalse(workers.TryHireTechnician());
            Assert.AreEqual(0, workers.Pool.TechnicianCount);

            waterReady = true;
            Assert.IsTrue(workers.TryHireTechnician());
            Assert.AreEqual(1, workers.Pool.TechnicianCount);
        }

        private static GridService CreateGrid(int size)
        {
            var grid = new GridService();
            grid.Create(size, size, 1f, Vector3.zero);
            return grid;
        }
    }
}
