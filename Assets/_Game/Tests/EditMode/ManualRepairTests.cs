using CleanEnergy.Buildings;
using CleanEnergy.Economy;
using CleanEnergy.Grid;
using CleanEnergy.Maintenance;
using NUnit.Framework;
using UnityEngine;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class ManualRepairTests
    {
        [Test]
        public void ManualRepairCost_UsesMaintenanceTimesFiveFloor25()
        {
            var def = ScriptableObject.CreateInstance<BuildingDefinition>();
            def.Configure(
                "small_solar", "Solar", "", BuildingCategory.Energy,
                120f, 12f, 20f, 0f, 0.45f, 0f, false, true, Color.white,
                upkeepCost: 4f);
            var instance = new BuildingInstance("s1", def, new GridCoordinate(0, 0), 0, null);
            Assert.AreEqual(25f, MaintenanceService.ManualRepairCost(instance), 0.001f);
        }

        [Test]
        public void TryManualRepair_RestoresLevelAndSpends()
        {
            var def = ScriptableObject.CreateInstance<BuildingDefinition>();
            def.Configure(
                "small_solar", "Solar", "", BuildingCategory.Energy,
                120f, 12f, 20f, 0f, 0.45f, 0f, false, true, Color.white,
                upkeepCost: 10f);
            var instance = new BuildingInstance("s1", def, new GridCoordinate(0, 0), 0, null);
            instance.MaintenanceLevel = 0.5f;
            var wallet = new Wallet(100f);
            Assert.IsTrue(MaintenanceService.TryManualRepair(instance, wallet, out _));
            Assert.AreEqual(1f, instance.MaintenanceLevel, 0.001f);
            Assert.AreEqual(50f, wallet.Money, 0.001f);
        }
    }
}
