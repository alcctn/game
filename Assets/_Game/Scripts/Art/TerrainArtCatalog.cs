using UnityEngine;

namespace CleanEnergy.Art
{
    /// <summary>
    /// Pure Poly terrain layers + water material for matte ground and river/lake visuals.
    /// </summary>
    [CreateAssetMenu(fileName = "TerrainArtCatalog", menuName = "Clean Energy/Terrain Art Catalog")]
    public sealed class TerrainArtCatalog : ScriptableObject
    {
        public const string DefaultAssetPath = "Assets/_Game/Data/Art/TerrainArtCatalog.asset";
        public const string LayerFolder =
            "Assets/Pure Poly/Free Low Poly Nature Pack/Terrain/Terrain Layers";
        public const string WaterMaterialPath =
            "Assets/Pure Poly/Free Low Poly Nature Pack/Materials/PP_Water.mat";

        public const int GrassIndex = 0;
        public const int RiverBedIndex = 1;
        public const int LakeShoreIndex = 2;
        public const int LayerCount = 3;

        [SerializeField] private TerrainLayer grass;
        [SerializeField] private TerrainLayer riverBed;
        [SerializeField] private TerrainLayer lakeShore;
        [SerializeField] private Material waterMaterial;

        public TerrainLayer Grass => grass;
        public TerrainLayer RiverBed => riverBed;
        public TerrainLayer LakeShore => lakeShore;
        public Material WaterMaterial => waterMaterial;

        public void Configure(
            TerrainLayer grassLayer,
            TerrainLayer riverBedLayer,
            TerrainLayer lakeShoreLayer,
            Material water)
        {
            grass = grassLayer;
            riverBed = riverBedLayer;
            lakeShore = lakeShoreLayer;
            waterMaterial = water;
        }

        public bool HasTerrainLayers()
        {
            return grass != null && riverBed != null && lakeShore != null;
        }

        public TerrainLayer[] BuildLayerArray()
        {
            return new[] { grass, riverBed, lakeShore };
        }

        public void EnforceMatteLayers()
        {
            EnforceMatte(grass);
            EnforceMatte(riverBed);
            EnforceMatte(lakeShore);
        }

        private static void EnforceMatte(TerrainLayer layer)
        {
            if (layer == null)
            {
                return;
            }

            layer.metallic = 0f;
            layer.smoothness = 0f;
            layer.specular = Color.black;
        }
    }
}
