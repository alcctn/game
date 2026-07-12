using UnityEngine;

namespace CleanEnergy.DebugTools
{
    /// <summary>
    /// Clamp helpers for network edge particle counts.
    /// </summary>
    public static class EdgeParticleMath
    {
        public const int MaxEdges = 64;
        public const int MaxParticlesPerEdge = 8;

        public static int ClampEdgeCount(int edgeCount)
        {
            return Mathf.Clamp(edgeCount, 0, MaxEdges);
        }

        public static int ParticlesForUtilization(float utilization, float speed = 1f)
        {
            var scaled = Mathf.Clamp01(utilization) * Mathf.Max(0f, speed) * MaxParticlesPerEdge;
            var count = Mathf.CeilToInt(scaled);
            return Mathf.Clamp(count, 0, MaxParticlesPerEdge);
        }
    }
}
