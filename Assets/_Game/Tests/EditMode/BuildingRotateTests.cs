using CleanEnergy.Buildings;
using CleanEnergy.Placement;
using NUnit.Framework;
using UnityEngine;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class BuildingRotateTests
    {
        [Test]
        public void CycleRotation_WrapsZeroToThree()
        {
            var go = new GameObject("Placement");
            try
            {
                var controller = go.AddComponent<PlacementController>();
                var def = ScriptableObject.CreateInstance<BuildingDefinition>();
                def.Configure(
                    "small_solar", "Solar", "t",
                    BuildingCategory.Energy,
                    100f, 10f, 30f, 0f, 0f, 0f,
                    false, true, Color.white);
                controller.SelectBuilding(def);
                Assert.AreEqual(0, controller.PlacementRotation);

                controller.CycleRotation();
                Assert.AreEqual(1, controller.PlacementRotation);
                controller.CycleRotation();
                Assert.AreEqual(2, controller.PlacementRotation);
                controller.CycleRotation();
                Assert.AreEqual(3, controller.PlacementRotation);
                controller.CycleRotation();
                Assert.AreEqual(0, controller.PlacementRotation);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void CancelPlacement_ResetsRotation()
        {
            var go = new GameObject("Placement");
            try
            {
                var controller = go.AddComponent<PlacementController>();
                var def = ScriptableObject.CreateInstance<BuildingDefinition>();
                def.Configure(
                    "small_wind", "Wind", "t",
                    BuildingCategory.Energy,
                    100f, 10f, 30f, 0f, 0f, 0f,
                    false, true, Color.white);
                controller.SelectBuilding(def);
                controller.CycleRotation();
                controller.CycleRotation();
                Assert.AreEqual(2, controller.PlacementRotation);

                controller.CancelPlacement();
                Assert.AreEqual(0, controller.PlacementRotation);
                Assert.IsFalse(controller.IsPlacementActive);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }
}
