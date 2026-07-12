using CleanEnergy.Art;
using NUnit.Framework;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class NaturePlainsTests
    {
        [Test]
        public void PlainsDensity_IsEightPercent()
        {
            Assert.AreEqual(0.08f, NatureSpawnMath.PlainsDensity, 0.0001f);
        }

        [Test]
        public void Roll_RespectsDensityZero()
        {
            Assert.IsFalse(NatureSpawnMath.Roll("s", 0, 0, 0f));
        }

        [Test]
        public void Roll_AlwaysTrue_AtDensityOne()
        {
            Assert.IsTrue(NatureSpawnMath.Roll("s", 0, 0, 1f));
        }
    }
}
