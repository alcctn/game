using UnityEngine;

namespace CleanEnergy.Map
{
    /// <summary>
    /// ScriptableObject configuration for seeded map generation.
    /// </summary>
    [CreateAssetMenu(fileName = "MapGenerationSettings", menuName = "Clean Energy/Map Generation Settings")]
    public sealed class MapGenerationSettings : ScriptableObject
    {
        [Header("Grid")]
        [SerializeField] private int gridWidth = 64;
        [SerializeField] private int gridHeight = 64;

        [Header("Terrain")]
        [SerializeField] private float terrainWorldSize = 256f;
        [SerializeField] private float maxHeight = 40f;
        [SerializeField] private float maxBuildableSlopeDegrees = 30f;

        [Header("Noise")]
        [SerializeField] private float noiseScale = 0.04f;
        [SerializeField] private int octaves = 4;
        [SerializeField] private float persistence = 0.5f;
        [SerializeField] private float lacunarity = 2f;
        [SerializeField] private string seed = "12345";

        [Header("Water")]
        [SerializeField] private float streamAccumulationThreshold = 12f;
        [SerializeField] private float lakeAccumulationThreshold = 40f;

        [Header("Solar")]
        [SerializeField] private float baseClimateSolar = 0.75f;
        [SerializeField] private float cloudFactor = 1f;
        [SerializeField] private float treeCoverFactorPlains = 1f;
        [SerializeField] private float treeCoverFactorForest = 0.7f;

        [Header("Wind")]
        [SerializeField] private float baseWind = 0.35f;
        [SerializeField] private float elevationWindBonus = 0.35f;
        [SerializeField] private float ridgeWindBonus = 0.25f;
        [SerializeField] private float obstacleWindPenalty = 0.15f;
        [SerializeField] private Vector2 prevailingWindDirection = new Vector2(1f, 0.25f);

        [Header("Biome")]
        [SerializeField] private float forestNoiseScale = 0.08f;
        [SerializeField] private float forestThreshold = 0.62f;
        [SerializeField] private float hillsSlopeDegrees = 12f;
        [SerializeField] private float ridgeSlopeDegrees = 22f;

        public int GridWidth => gridWidth;
        public int GridHeight => gridHeight;
        public float TerrainWorldSize => terrainWorldSize;
        public float MaxHeight => maxHeight;
        public float MaxBuildableSlopeDegrees => maxBuildableSlopeDegrees;
        public float NoiseScale => noiseScale;
        public int Octaves => octaves;
        public float Persistence => persistence;
        public float Lacunarity => lacunarity;
        public string Seed => seed;

        public float StreamAccumulationThreshold => streamAccumulationThreshold;
        public float LakeAccumulationThreshold => lakeAccumulationThreshold;
        public float BaseClimateSolar => baseClimateSolar;
        public float CloudFactor => cloudFactor;
        public float TreeCoverFactorPlains => treeCoverFactorPlains;
        public float TreeCoverFactorForest => treeCoverFactorForest;
        public float BaseWind => baseWind;
        public float ElevationWindBonus => elevationWindBonus;
        public float RidgeWindBonus => ridgeWindBonus;
        public float ObstacleWindPenalty => obstacleWindPenalty;
        public Vector2 PrevailingWindDirection => prevailingWindDirection;
        public float ForestNoiseScale => forestNoiseScale;
        public float ForestThreshold => forestThreshold;
        public float HillsSlopeDegrees => hillsSlopeDegrees;
        public float RidgeSlopeDegrees => ridgeSlopeDegrees;

        public float CellSize => terrainWorldSize / Mathf.Max(1, gridWidth);

        public void SetSeed(string newSeed)
        {
            seed = newSeed ?? string.Empty;
        }

        public void SetBaseClimateSolar(float value)
        {
            baseClimateSolar = Mathf.Max(0f, value);
        }

        /// <summary>
        /// Test/editor helper to override water classification thresholds.
        /// </summary>
        public void SetWaterThresholds(float streamThreshold, float lakeThreshold)
        {
            streamAccumulationThreshold = streamThreshold;
            lakeAccumulationThreshold = lakeThreshold;
        }

        public bool Validate(out string error)
        {
            if (gridWidth < 2 || gridHeight < 2)
            {
                error = "[Map] Grid dimensions must be at least 2x2.";
                return false;
            }

            if (terrainWorldSize <= 0f || maxHeight <= 0f)
            {
                error = "[Map] Terrain world size and max height must be positive.";
                return false;
            }

            if (noiseScale <= 0f)
            {
                error = "[Map] Noise scale must be positive.";
                return false;
            }

            if (octaves < 1)
            {
                error = "[Map] Octaves must be at least 1.";
                return false;
            }

            if (streamAccumulationThreshold <= 0f || lakeAccumulationThreshold < streamAccumulationThreshold)
            {
                error = "[Map] Water thresholds must be positive and lake >= stream.";
                return false;
            }

            if (baseClimateSolar < 0f || baseWind < 0f)
            {
                error = "[Map] Base solar and wind must be non-negative.";
                return false;
            }

            error = null;
            return true;
        }
    }
}
