using CleanEnergy.Art;
using NUnit.Framework;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class NatureWaterTests
    {
        [Test]
        public void LakeAndBridgeCaps_AreLocked()
        {
            Assert.AreEqual(32, NatureSpawnMath.MaxLakeInstances);
            Assert.AreEqual(8, NatureSpawnMath.MaxBridgeInstances);
        }

        [Test]
        public void AllocateBudget_ReservesLakeAndBridge()
        {
            NatureSpawnMath.AllocateBudget(
                512,
                out var plains,
                out var forest,
                out var hills,
                out var lake,
                out var bridge);
            Assert.LessOrEqual(lake, NatureSpawnMath.MaxLakeInstances);
            Assert.LessOrEqual(bridge, NatureSpawnMath.MaxBridgeInstances);
            Assert.AreEqual(512, plains + forest + hills + lake + bridge);
        }
    }
}
