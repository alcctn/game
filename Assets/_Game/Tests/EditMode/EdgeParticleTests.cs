using CleanEnergy.DebugTools;
using NUnit.Framework;

namespace CleanEnergy.Tests.EditMode
{
    public sealed class EdgeParticleTests
    {
        [Test]
        public void ClampEdgeCount_CapsAtSixtyFour()
        {
            Assert.AreEqual(0, EdgeParticleMath.ClampEdgeCount(-3));
            Assert.AreEqual(10, EdgeParticleMath.ClampEdgeCount(10));
            Assert.AreEqual(EdgeParticleMath.MaxEdges, EdgeParticleMath.ClampEdgeCount(100));
        }

        [Test]
        public void ParticlesForUtilization_CapsAtEight()
        {
            Assert.AreEqual(0, EdgeParticleMath.ParticlesForUtilization(0f));
            Assert.AreEqual(EdgeParticleMath.MaxParticlesPerEdge,
                EdgeParticleMath.ParticlesForUtilization(1f, 2f));
            Assert.AreEqual(4, EdgeParticleMath.ParticlesForUtilization(0.5f, 1f));
        }
    }
}
