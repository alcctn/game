using CleanEnergy.Map;

namespace CleanEnergy.TerrainGeneration
{
    /// <summary>
    /// Pure helpers mapping biomes to terrain splat layer weights.
    /// </summary>
    public static class TerrainBiomeSplatMath
    {
        public const int GrassIndex = 0;
        public const int RiverBedIndex = 1;
        public const int LakeShoreIndex = 2;
        public const int LayerCount = 3;

        public static int LayerIndexForBiome(BiomeType biome)
        {
            switch (biome)
            {
                case BiomeType.River:
                    return RiverBedIndex;
                case BiomeType.Lake:
                    return LakeShoreIndex;
                default:
                    return GrassIndex;
            }
        }

        /// <summary>
        /// Writes normalized weights into a length-<see cref="LayerCount"/> buffer.
        /// </summary>
        public static void WriteWeights(float[] weights, BiomeType biome)
        {
            if (weights == null || weights.Length < LayerCount)
            {
                return;
            }

            for (var i = 0; i < LayerCount; i++)
            {
                weights[i] = 0f;
            }

            weights[LayerIndexForBiome(biome)] = 1f;
        }
    }
}
