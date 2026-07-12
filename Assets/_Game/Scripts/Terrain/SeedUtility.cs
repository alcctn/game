using CleanEnergy.Map;
using UnityEngine;

namespace CleanEnergy.TerrainGeneration
{
    /// <summary>
    /// Shared seeded helpers for resource-layer generators.
    /// </summary>
    public static class SeedUtility
    {
        public static Vector2 HashToOffset(string seed)
        {
            unchecked
            {
                var hash = 23;
                var value = seed ?? string.Empty;
                for (var i = 0; i < value.Length; i++)
                {
                    hash = hash * 31 + value[i];
                }

                return new Vector2((hash & 0xFF) * 0.1f, ((hash >> 8) & 0xFF) * 0.1f);
            }
        }

        public static bool IsForested(int x, int y, MapGenerationSettings settings, Vector2 seedOffset)
        {
            var sampleX = (x + seedOffset.x) * settings.ForestNoiseScale;
            var sampleY = (y + seedOffset.y) * settings.ForestNoiseScale;
            return DeterministicNoise.Perlin(sampleX, sampleY) >= settings.ForestThreshold;
        }
    }
}
