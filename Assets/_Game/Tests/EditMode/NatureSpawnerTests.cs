using CleanEnergy.Art;
using NUnit.Framework;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class NatureSpawnerTests
    {
        [Test]
        public void MaxInstances_DefaultIs512()
        {
            Assert.AreEqual(512, NatureSpawnMath.MaxInstances);
        }

        [Test]
        public void Hash_IsDeterministic_AndVariesByCell()
        {
            var a = NatureSpawnMath.Hash("seed", 1, 2);
            var b = NatureSpawnMath.Hash("seed", 1, 2);
            var c = NatureSpawnMath.Hash("seed", 2, 1);
            Assert.AreEqual(a, b);
            Assert.AreNotEqual(a, c);
        }

        [Test]
        public void UnitFloat_IsInZeroOne()
        {
            var u = NatureSpawnMath.UnitFloat(NatureSpawnMath.Hash("x", 3, 4));
            Assert.GreaterOrEqual(u, 0f);
            Assert.Less(u, 1f);
        }

        [Test]
        public void NatureRootName_IsLocked()
        {
            Assert.AreEqual("NatureRoot", NatureVisualSpawner.NatureRootName);
        }
    }
}
