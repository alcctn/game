using CleanEnergy.TerrainGeneration;
using NUnit.Framework;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class TerrainUrpMaterialTests
    {
        [Test]
        public void ApplyUrpTerrainMaterial_Null_IsNoOp()
        {
            Assert.DoesNotThrow(() => TerrainBuilder.ApplyUrpTerrainMaterial(null));
        }
    }
}
