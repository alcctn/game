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

        public float CellSize => terrainWorldSize / Mathf.Max(1, gridWidth);

        public void SetSeed(string newSeed)
        {
            seed = newSeed ?? string.Empty;
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

            error = null;
            return true;
        }
    }
}
