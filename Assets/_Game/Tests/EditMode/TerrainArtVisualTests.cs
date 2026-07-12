using CleanEnergy.Art;
using CleanEnergy.Map;
using CleanEnergy.TerrainGeneration;
using NUnit.Framework;
using UnityEngine;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class TerrainArtVisualTests
    {
        [Test]
        public void LayerIndexForBiome_MapsRiverLakeAndLand()
        {
            Assert.AreEqual(TerrainBiomeSplatMath.RiverBedIndex, TerrainBiomeSplatMath.LayerIndexForBiome(BiomeType.River));
            Assert.AreEqual(TerrainBiomeSplatMath.LakeShoreIndex, TerrainBiomeSplatMath.LayerIndexForBiome(BiomeType.Lake));
            Assert.AreEqual(TerrainBiomeSplatMath.GrassIndex, TerrainBiomeSplatMath.LayerIndexForBiome(BiomeType.Plains));
            Assert.AreEqual(TerrainBiomeSplatMath.GrassIndex, TerrainBiomeSplatMath.LayerIndexForBiome(BiomeType.Hills));
            Assert.AreEqual(TerrainBiomeSplatMath.GrassIndex, TerrainBiomeSplatMath.LayerIndexForBiome(BiomeType.Forest));
            Assert.AreEqual(TerrainBiomeSplatMath.GrassIndex, TerrainBiomeSplatMath.LayerIndexForBiome(BiomeType.Ridge));
        }

        [Test]
        public void WriteWeights_SetsSingleLayerToOne()
        {
            var weights = new float[TerrainBiomeSplatMath.LayerCount];
            TerrainBiomeSplatMath.WriteWeights(weights, BiomeType.River);
            Assert.AreEqual(0f, weights[0], 1e-5f);
            Assert.AreEqual(1f, weights[1], 1e-5f);
            Assert.AreEqual(0f, weights[2], 1e-5f);

            TerrainBiomeSplatMath.WriteWeights(weights, BiomeType.Lake);
            Assert.AreEqual(0f, weights[0], 1e-5f);
            Assert.AreEqual(0f, weights[1], 1e-5f);
            Assert.AreEqual(1f, weights[2], 1e-5f);
        }

        [Test]
        public void CountWaterCells_NullOrUninitialized_ReturnsZero()
        {
            Assert.AreEqual(0, WaterSurfaceVisual.CountWaterCells(null));
            Assert.AreEqual(0, WaterSurfaceVisual.CountWaterCells(new CleanEnergy.Grid.GridService()));
        }

        [Test]
        public void ApplyUrpTerrainMaterial_Null_IsNoOp()
        {
            Assert.DoesNotThrow(() => TerrainBuilder.ApplyUrpTerrainMaterial(null));
        }

        [Test]
        public void TerrainArtCatalog_HasTerrainLayers_RequiresAllThree()
        {
            var catalog = ScriptableObject.CreateInstance<TerrainArtCatalog>();
            Assert.IsFalse(catalog.HasTerrainLayers());

            var a = new TerrainLayer();
            var b = new TerrainLayer();
            var c = new TerrainLayer();
            catalog.Configure(a, b, c, null);
            Assert.IsTrue(catalog.HasTerrainLayers());
            Object.DestroyImmediate(catalog);
        }
    }
}
