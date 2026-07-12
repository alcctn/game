using UnityEngine;

namespace CleanEnergy.Art
{
    /// <summary>
    /// Deterministic cell hashing and density budget for nature scatter.
    /// </summary>
    public static class NatureSpawnMath
    {
        public const int MaxInstances = 512;
        public const float PlainsDensity = 0.08f;
        public const float ForestDensity = 0.12f;
        public const float HillsDensity = 0.05f;
        public const int MaxLakeInstances = 32;
        public const int MaxBridgeInstances = 8;

        /// <summary>Stable int hash from seed + cell.</summary>
        public static int Hash(string seed, int x, int y)
        {
            unchecked
            {
                var h = seed != null ? seed.GetHashCode() : 0;
                h = (h * 397) ^ x;
                h = (h * 397) ^ (y * 73856093);
                return h;
            }
        }

        /// <summary>Unit float in [0,1) from hash.</summary>
        public static float UnitFloat(int hash)
        {
            return (hash & 0x7FFFFFFF) / (float)int.MaxValue;
        }

        public static bool Roll(string seed, int x, int y, float density)
        {
            if (density <= 0f)
            {
                return false;
            }

            return UnitFloat(Hash(seed, x, y)) < Mathf.Clamp01(density);
        }

        /// <summary>
        /// Splits the global instance budget across biome shares (lake/bridge reserved separately).
        /// </summary>
        public static void AllocateBudget(
            int maxTotal,
            out int plainsCap,
            out int forestCap,
            out int hillsCap,
            out int lakeCap,
            out int bridgeCap)
        {
            var total = Mathf.Max(0, maxTotal);
            lakeCap = Mathf.Min(MaxLakeInstances, total / 16);
            bridgeCap = Mathf.Min(MaxBridgeInstances, total / 64);
            var remaining = Mathf.Max(0, total - lakeCap - bridgeCap);
            // Shares ~ plains 35%, forest 45%, hills 20%
            plainsCap = remaining * 35 / 100;
            forestCap = remaining * 45 / 100;
            hillsCap = remaining - plainsCap - forestCap;
        }
    }
}
