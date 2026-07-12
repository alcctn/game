using CleanEnergy.Art;
using NUnit.Framework;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class NatureHillsTests
    {
        [Test]
        public void HillsDensity_IsFivePercent()
        {
            Assert.AreEqual(0.05f, NatureSpawnMath.HillsDensity, 0.0001f);
        }
    }
}
