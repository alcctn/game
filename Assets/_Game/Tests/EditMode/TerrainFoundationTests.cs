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
}
