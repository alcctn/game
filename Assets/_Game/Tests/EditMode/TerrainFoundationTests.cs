using CleanEnergy.Grid;
using CleanEnergy.Map;
using CleanEnergy.TerrainGeneration;
using NUnit.Framework;
using UnityEngine;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class HeightMapGeneratorTests
    {
        [Test]
        public void SameSeed_ProducesIdenticalHeightMap()
        {
            var settings = CreateSettings("12345");
            var generator = new HeightMapGenerator();
            var a = generator.Generate(settings);
            var b = generator.Generate(settings);

            Assert.AreEqual(a.GetLength(0), b.GetLength(0));
            Assert.AreEqual(a.GetLength(1), b.GetLength(1));
            for (var x = 0; x < a.GetLength(0); x++)
            {
                for (var y = 0; y < a.GetLength(1); y++)
                {
                    Assert.AreEqual(a[x, y], b[x, y], 1e-6f);
                }
            }
        }

        [Test]
        public void DifferentSeed_ProducesDifferentHeightMap()
        {
            var generator = new HeightMapGenerator();
            var a = generator.Generate(CreateSettings("111"));
            var b = generator.Generate(CreateSettings("222"));

            var identical = true;
            for (var x = 0; x < a.GetLength(0) && identical; x++)
            {
                for (var y = 0; y < a.GetLength(1); y++)
                {
                    if (!Mathf.Approximately(a[x, y], b[x, y]))
                    {
                        identical = false;
                        break;
                    }
                }
            }

            Assert.IsFalse(identical);
        }

        [Test]
        public void HeightValues_AreNormalizedToZeroOne()
        {
            var map = new HeightMapGenerator().Generate(CreateSettings("normalize"));
            for (var x = 0; x < map.GetLength(0); x++)
            {
                for (var y = 0; y < map.GetLength(1); y++)
                {
                    Assert.GreaterOrEqual(map[x, y], 0f);
                    Assert.LessOrEqual(map[x, y], 1f);
                    Assert.IsFalse(float.IsNaN(map[x, y]));
                }
            }
        }

        private static MapGenerationSettings CreateSettings(string seed)
        {
            var settings = ScriptableObject.CreateInstance<MapGenerationSettings>();
            settings.SetSeed(seed);
            return settings;
        }
    }

    public sealed class GridServiceTests
    {
        [Test]
        public void WorldAndGrid_RoundTripIsConsistent()
        {
            var grid = new GridService();
            grid.Create(64, 64, 4f, Vector3.zero);

            var coordinate = new GridCoordinate(10, 20);
            var world = grid.GridToWorld(coordinate);
            Assert.IsTrue(grid.TryWorldToGrid(world, out var back));
            Assert.AreEqual(coordinate, back);
        }

        [Test]
        public void OutOfBounds_TryGetReturnsFalse()
        {
            var grid = new GridService();
            grid.Create(8, 8, 1f, Vector3.zero);
            Assert.IsFalse(grid.TryGetCell(new GridCoordinate(-1, 0), out _));
            Assert.IsFalse(grid.TryGetCell(new GridCoordinate(8, 0), out _));
        }
    }

    public sealed class SlopeCalculatorTests
    {
        [Test]
        public void FlatHeightMap_YieldsNearZeroSlope()
        {
            const int size = 8;
            var heightMap = new float[size, size];
            for (var x = 0; x < size; x++)
            {
                for (var y = 0; y < size; y++)
                {
                    heightMap[x, y] = 0.5f;
                }
            }

            var grid = new GridService();
            grid.Create(size, size, 1f, Vector3.zero);
            new SlopeCalculator().Calculate(heightMap, grid, 40f, 1f, 30f);

            for (var x = 0; x < size; x++)
            {
                for (var y = 0; y < size; y++)
                {
                    Assert.Less(grid.GetCell(new GridCoordinate(x, y)).Slope, 0.01f);
                }
            }
        }

        [Test]
        public void SteepHeightMap_YieldsHighSlope()
        {
            const int size = 8;
            var heightMap = new float[size, size];
            for (var x = 0; x < size; x++)
            {
                for (var y = 0; y < size; y++)
                {
                    heightMap[x, y] = x / (float)(size - 1);
                }
            }

            var slope = SlopeCalculator.CalculateCellSlopeDegrees(heightMap, 3, 3, size, size, 40f, 1f);
            Assert.Greater(slope, 20f);
            Assert.IsFalse(float.IsNaN(slope));
        }
    }

    public sealed class WaterFlowCalculatorTests
    {
        [Test]
        public void SlopedMap_RoutesFlowDownhillAndAccumulates()
        {
            const int size = 8;
            var heightMap = new float[size, size];
            for (var x = 0; x < size; x++)
            {
                for (var y = 0; y < size; y++)
                {
                    // High elevation at large Y so flow drains toward Y=0.
                    heightMap[x, y] = y / (float)(size - 1);
                }
            }

            var settings = ScriptableObject.CreateInstance<MapGenerationSettings>();
            settings.SetSeed("water");
            settings.SetWaterThresholds(4f, 20f);
            var grid = TestMapFactory.CreateFilledGrid(heightMap, settings);

            new WaterFlowCalculator().Calculate(heightMap, grid, settings);

            var top = grid.GetCell(new GridCoordinate(3, size - 1));
            var bottom = grid.GetCell(new GridCoordinate(3, 0));
            Assert.AreEqual(new Vector2Int(0, -1), top.FlowDirection);
            Assert.Greater(bottom.WaterFlow, top.WaterFlow);
            Assert.IsTrue(bottom.IsWater);
        }

        [Test]
        public void SameHeightMap_ProducesDeterministicWater()
        {
            var settings = ScriptableObject.CreateInstance<MapGenerationSettings>();
            settings.SetSeed("det-water");
            var heightMap = new HeightMapGenerator().Generate(settings);

            var gridA = TestMapFactory.CreateFilledGrid(heightMap, settings);
            var gridB = TestMapFactory.CreateFilledGrid(heightMap, settings);
            var calculator = new WaterFlowCalculator();
            calculator.Calculate(heightMap, gridA, settings);
            calculator.Calculate(heightMap, gridB, settings);

            for (var x = 0; x < settings.GridWidth; x++)
            {
                for (var y = 0; y < settings.GridHeight; y++)
                {
                    var a = gridA.GetCell(new GridCoordinate(x, y));
                    var b = gridB.GetCell(new GridCoordinate(x, y));
                    Assert.AreEqual(a.WaterFlow, b.WaterFlow, 1e-5f);
                    Assert.AreEqual(a.IsWater, b.IsWater);
                    Assert.AreEqual(a.FlowDirection, b.FlowDirection);
                }
            }
        }
    }

    public sealed class ResourceLayerTests
    {
        [Test]
        public void SolarAndWind_AreClampedAndFinite()
        {
            var settings = ScriptableObject.CreateInstance<MapGenerationSettings>();
            settings.SetSeed("resources");
            var heightMap = new HeightMapGenerator().Generate(settings);
            var grid = TestMapFactory.CreateFilledGrid(heightMap, settings);

            new SlopeCalculator().Calculate(heightMap, grid, settings.MaxHeight, settings.CellSize, settings.MaxBuildableSlopeDegrees);
            new WaterFlowCalculator().Calculate(heightMap, grid, settings);
            new SolarPotentialCalculator().Calculate(grid, settings);
            new WindPotentialCalculator().Calculate(heightMap, grid, settings);
            new BiomeGenerator().Calculate(grid, settings);
            new BuildabilityCalculator().Calculate(grid, settings);

            for (var x = 0; x < settings.GridWidth; x++)
            {
                for (var y = 0; y < settings.GridHeight; y++)
                {
                    var cell = grid.GetCell(new GridCoordinate(x, y));
                    Assert.GreaterOrEqual(cell.SolarPotential, 0f);
                    Assert.LessOrEqual(cell.SolarPotential, 1f);
                    Assert.IsFalse(float.IsNaN(cell.SolarPotential));
                    Assert.GreaterOrEqual(cell.WindPotential, 0f);
                    Assert.LessOrEqual(cell.WindPotential, 1f);
                    Assert.IsFalse(float.IsNaN(cell.WindPotential));
                    if (cell.IsWater)
                    {
                        Assert.IsFalse(cell.IsBuildable);
                    }
                }
            }
        }

        [Test]
        public void WaterCells_AreNotBuildable()
        {
            const int size = 4;
            var heightMap = new float[size, size];
            for (var x = 0; x < size; x++)
            {
                for (var y = 0; y < size; y++)
                {
                    heightMap[x, y] = y / (float)(size - 1);
                }
            }

            var settings = ScriptableObject.CreateInstance<MapGenerationSettings>();
            settings.SetWaterThresholds(2f, 10f);
            var grid = TestMapFactory.CreateFilledGrid(heightMap, settings);
            new SlopeCalculator().Calculate(heightMap, grid, 40f, 1f, 30f);
            new WaterFlowCalculator().Calculate(heightMap, grid, settings);
            new BuildabilityCalculator().Calculate(grid, settings);

            var waterFound = false;
            for (var x = 0; x < size; x++)
            {
                for (var y = 0; y < size; y++)
                {
                    var cell = grid.GetCell(new GridCoordinate(x, y));
                    if (!cell.IsWater)
                    {
                        continue;
                    }

                    waterFound = true;
                    Assert.IsFalse(cell.IsBuildable);
                }
            }

            Assert.IsTrue(waterFound);
        }
    }

    internal static class TestMapFactory
    {
        public static GridService CreateFilledGrid(float[,] heightMap, MapGenerationSettings settings)
        {
            var width = heightMap.GetLength(0);
            var height = heightMap.GetLength(1);
            var grid = new GridService();
            var cellSize = settings != null && settings.GridWidth == width
                ? settings.CellSize
                : 1f;
            var maxHeight = settings != null ? settings.MaxHeight : 40f;
            grid.Create(width, height, cellSize, Vector3.zero);
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    grid.SetElevation(new GridCoordinate(x, y), heightMap[x, y] * maxHeight);
                }
            }

            return grid;
        }
    }
}
