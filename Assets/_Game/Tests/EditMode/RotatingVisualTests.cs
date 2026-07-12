using CleanEnergy.Buildings;
using CleanEnergy.Grid;
using CleanEnergy.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class RotatingVisualTests
    {
        [Test]
        public void ResolveRpm_WindIs40_HydroIs25()
        {
            Assert.AreEqual(40f, RotatingVisual.ResolveRpmForBuildingId("small_wind"), 0.001f);
            Assert.AreEqual(25f, RotatingVisual.ResolveRpmForBuildingId("water_wheel"), 0.001f);
            Assert.AreEqual(25f, RotatingVisual.ResolveRpmForBuildingId("small_hydro"), 0.001f);
            Assert.AreEqual(0f, RotatingVisual.ResolveRpmForBuildingId("village"), 0.001f);
        }

        [Test]
        public void BuildingFactory_AddsRotatingVisual_ForWindAndHydro()
        {
            AssertHasRotating("small_wind", 40f);
            AssertHasRotating("water_wheel", 25f);
            AssertHasRotating("small_hydro", 25f);
        }

        [Test]
        public void BuildingFactory_DoesNotAddRotatingVisual_ForVillage()
        {
            var def = ScriptableObject.CreateInstance<BuildingDefinition>();
            def.Configure(
                "village", "Village", "", BuildingCategory.Settlement,
                200f, 0f, 20f, 0f, 0f, 0f, false, true, Color.white, demand: 10f);

            var grid = CreateFlatGrid(4);
            var factory = new BuildingFactory();
            BuildingInstance instance = null;
            try
            {
                instance = factory.Create(def, new GridCoordinate(0, 0), grid, null);
                Assert.IsNull(instance.GameObject.GetComponent<RotatingVisual>());
            }
            finally
            {
                if (instance?.GameObject != null)
                {
                    Object.DestroyImmediate(instance.GameObject);
                }
            }
        }

        [Test]
        public void Update_DoesNotRotate_WhenPaused()
        {
            var clockGo = new GameObject("ClockPause");
            var visualGo = new GameObject("Spinner");
            try
            {
                var clock = clockGo.AddComponent<SimulationClock>();
                clock.SetSpeed(SimulationSpeed.Paused);
                var rotating = visualGo.AddComponent<RotatingVisual>();
                rotating.Configure(40f, clock);
                var before = visualGo.transform.eulerAngles.y;

                rotating.Tick(0.5f);

                Assert.AreEqual(before, visualGo.transform.eulerAngles.y, 0.01f);
            }
            finally
            {
                Object.DestroyImmediate(visualGo);
                Object.DestroyImmediate(clockGo);
            }
        }

        [Test]
        public void Tick_RotatesWhenNotPaused()
        {
            var clockGo = new GameObject("ClockRun");
            var visualGo = new GameObject("SpinnerRun");
            try
            {
                var clock = clockGo.AddComponent<SimulationClock>();
                clock.SetSpeed(SimulationSpeed.One);
                var rotating = visualGo.AddComponent<RotatingVisual>();
                rotating.Configure(60f, clock);
                var before = visualGo.transform.eulerAngles.y;

                rotating.Tick(0.5f);

                var delta = Mathf.DeltaAngle(before, visualGo.transform.eulerAngles.y);
                Assert.AreEqual(180f, delta, 0.5f);
            }
            finally
            {
                Object.DestroyImmediate(visualGo);
                Object.DestroyImmediate(clockGo);
            }
        }

        private static void AssertHasRotating(string id, float expectedRpm)
        {
            var def = ScriptableObject.CreateInstance<BuildingDefinition>();
            def.Configure(
                id, id, "", BuildingCategory.Energy,
                100f, 10f, 30f, 0f, 0f, 0f, false, true, Color.white);

            var grid = CreateFlatGrid(4);
            var factory = new BuildingFactory();
            BuildingInstance instance = null;
            try
            {
                instance = factory.Create(def, new GridCoordinate(0, 0), grid, null);
                var rotating = instance.GameObject.GetComponent<RotatingVisual>();
                Assert.IsNotNull(rotating);
                Assert.AreEqual(expectedRpm, rotating.Rpm, 0.001f);
            }
            finally
            {
                if (instance?.GameObject != null)
                {
                    Object.DestroyImmediate(instance.GameObject);
                }
            }
        }

        private static GridService CreateFlatGrid(int size)
        {
            var grid = new GridService();
            grid.Create(size, size, 1f, Vector3.zero);
            for (var x = 0; x < size; x++)
            {
                for (var y = 0; y < size; y++)
                {
                    grid.SetElevation(new GridCoordinate(x, y), 1f);
                }
            }

            return grid;
        }
    }
}
