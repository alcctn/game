using CleanEnergy.Buildings;
using CleanEnergy.Economy;
using CleanEnergy.Grid;
using CleanEnergy.Placement;
using CleanEnergy.Scenario;
using CleanEnergy.Settlements;
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

        private static GridService CreateGrid(int size)
        {
            var grid = new GridService();
            grid.Create(size, size, 1f, Vector3.zero);
            return grid;
        }
    }
}
