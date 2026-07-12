using CleanEnergy.Art;
using NUnit.Framework;
using UnityEngine;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class NatureForestTests
    {
        [Test]
        public void ForestDensity_IsTwelvePercent()
        {
            Assert.AreEqual(0.12f, NatureSpawnMath.ForestDensity, 0.0001f);
        }

        [Test]
        public void DisableColliders_TurnsOffMeshColliders()
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            try
            {
                Assert.IsTrue(go.GetComponent<Collider>().enabled);
                NatureVisualSpawner.DisableColliders(go);
                Assert.IsFalse(go.GetComponent<Collider>().enabled);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }
}
