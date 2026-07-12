using CleanEnergy.Art;
using CleanEnergy.Grid;
using CleanEnergy.Map;
using UnityEngine;

namespace CleanEnergy.TerrainGeneration
{
    /// <summary>
    /// Applies a height map to a Unity Terrain, aligned with the logical grid origin.
    /// </summary>
    public sealed class TerrainBuilder
    {
        private const int DefaultAlphamapResolution = 128;

        private UnityEngine.Terrain _terrain;
        private TerrainArtCatalog _artCatalog;

        public UnityEngine.Terrain CurrentTerrain => _terrain;

        public void SetArtCatalog(TerrainArtCatalog catalog)
        {
            _artCatalog = catalog;
        }

        public UnityEngine.Terrain BuildOrUpdate(MapGenerationSettings settings, float[,] heightMap, Transform parent)
        {
            if (settings == null || heightMap == null)
            {
                Debug.LogError("[Terrain] Settings and height map are required.");
                return null;
            }

            var resolution = NextPowerOfTwoPlusOne(Mathf.Max(settings.GridWidth, settings.GridHeight));
            var terrainData = _terrain != null ? _terrain.terrainData : new TerrainData();
            terrainData.heightmapResolution = resolution;
            terrainData.size = new Vector3(settings.TerrainWorldSize, settings.MaxHeight, settings.TerrainWorldSize);

            var heights = SampleToTerrainHeights(heightMap, resolution);
            terrainData.SetHeights(0, 0, heights);
            EnsureTerrainLayers(terrainData, _artCatalog);

            if (_terrain == null)
            {
                var go = UnityEngine.Terrain.CreateTerrainGameObject(terrainData);
                go.name = "GeneratedTerrain";
                if (parent != null)
                {
                    go.transform.SetParent(parent, false);
                }

                go.transform.position = Vector3.zero;
                _terrain = go.GetComponent<UnityEngine.Terrain>();
            }
            else
            {
                _terrain.terrainData = terrainData;
                _terrain.transform.position = Vector3.zero;
                _terrain.Flush();
            }

            ApplyUrpTerrainMaterial(_terrain);

            return _terrain;
        }

        /// <summary>
        /// Paints splat weights from grid biomes (call after biome generation).
        /// </summary>
        public void ApplyBiomeAlphamap(GridService grid, MapGenerationSettings settings)
        {
            if (_terrain == null || _terrain.terrainData == null || grid == null || !grid.IsInitialized || settings == null)
            {
                return;
            }

            ApplyBiomeAlphamap(_terrain.terrainData, grid, settings);
            _terrain.Flush();
        }

        public static void ApplyBiomeAlphamap(
            TerrainData terrainData,
            GridService grid,
            MapGenerationSettings settings)
        {
            if (terrainData == null || grid == null || !grid.IsInitialized || settings == null)
            {
                return;
            }

            if (terrainData.alphamapLayers < TerrainBiomeSplatMath.LayerCount)
            {
                return;
            }

            var res = terrainData.alphamapResolution;
            if (res < 16)
            {
                terrainData.alphamapResolution = DefaultAlphamapResolution;
                res = terrainData.alphamapResolution;
            }

            var maps = new float[res, res, terrainData.alphamapLayers];
            var weights = new float[TerrainBiomeSplatMath.LayerCount];
            var worldSize = settings.TerrainWorldSize;
            var denom = Mathf.Max(1, res - 1);

            for (var z = 0; z < res; z++)
            {
                for (var x = 0; x < res; x++)
                {
                    var worldX = (x / (float)denom) * worldSize;
                    var worldZ = (z / (float)denom) * worldSize;
                    var biome = BiomeType.Plains;
                    if (grid.TryWorldToGrid(new Vector3(worldX, 0f, worldZ), out var coord)
                        && grid.TryGetCell(coord, out var cell))
                    {
                        biome = cell.Biome;
                    }

                    TerrainBiomeSplatMath.WriteWeights(weights, biome);
                    for (var layer = 0; layer < TerrainBiomeSplatMath.LayerCount; layer++)
                    {
                        maps[z, x, layer] = weights[layer];
                    }
                }
            }

            terrainData.SetAlphamaps(0, 0, maps);
        }

        /// <summary>
        /// Assigns Pure Poly matte layers when catalog is available; otherwise a solid green fallback.
        /// </summary>
        public static void EnsureTerrainLayers(TerrainData terrainData, TerrainArtCatalog catalog = null)
        {
            if (terrainData == null)
            {
                return;
            }

            if (catalog != null && catalog.HasTerrainLayers())
            {
                catalog.EnforceMatteLayers();
                terrainData.terrainLayers = catalog.BuildLayerArray();
                if (terrainData.alphamapResolution < 16)
                {
                    terrainData.alphamapResolution = DefaultAlphamapResolution;
                }

                return;
            }

            var existing = terrainData.terrainLayers;
            if (existing != null && existing.Length > 0 && existing[0] != null)
            {
                EnforceMatteOnLayers(existing);
                return;
            }

            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false)
            {
                name = "TerrainGrassTex",
                wrapMode = TextureWrapMode.Repeat,
                filterMode = FilterMode.Bilinear
            };
            var pixels = new Color[16];
            var grass = new Color(0.34f, 0.55f, 0.27f, 1f);
            for (var i = 0; i < pixels.Length; i++)
            {
                pixels[i] = grass;
            }

            tex.SetPixels(pixels);
            tex.Apply(false, true);

            var layer = new TerrainLayer
            {
                diffuseTexture = tex,
                tileSize = new Vector2(8f, 8f),
                tileOffset = Vector2.zero,
                metallic = 0f,
                smoothness = 0f,
                specular = Color.black
            };
            terrainData.terrainLayers = new[] { layer };
        }

        /// <summary>
        /// Built-in Terrain material is invisible/pink under URP; assign Terrain Lit when available.
        /// </summary>
        public static void ApplyUrpTerrainMaterial(UnityEngine.Terrain terrain)
        {
            if (terrain == null)
            {
                return;
            }

            var shader = Shader.Find("Universal Render Pipeline/Terrain/Lit");
            if (shader == null)
            {
                // Do NOT fall back to Lit — it does not draw Terrain geometry.
                return;
            }

            var mat = terrain.materialTemplate;
            if (mat == null || mat.shader != shader)
            {
                mat = new Material(shader) { name = "GeneratedTerrain_URP" };
            }

            var white = Color.white;
            if (mat.HasProperty("_Color"))
            {
                mat.SetColor("_Color", white);
            }

            if (mat.HasProperty("_BaseColor"))
            {
                mat.SetColor("_BaseColor", white);
            }

            if (mat.HasProperty("_DiffuseColor"))
            {
                mat.SetColor("_DiffuseColor", white);
            }

            if (mat.HasProperty("_Smoothness"))
            {
                mat.SetFloat("_Smoothness", 0f);
            }

            if (mat.HasProperty("_Metallic"))
            {
                mat.SetFloat("_Metallic", 0f);
            }

            if (mat.HasProperty("_SpecColor"))
            {
                mat.SetColor("_SpecColor", Color.black);
            }

            terrain.materialTemplate = mat;
        }

        public void Clear()
        {
            if (_terrain == null)
            {
                return;
            }

            var go = _terrain.gameObject;
            _terrain = null;
            if (Application.isPlaying)
            {
                Object.Destroy(go);
            }
            else
            {
                Object.DestroyImmediate(go);
            }
        }

        private static void EnforceMatteOnLayers(TerrainLayer[] layers)
        {
            if (layers == null)
            {
                return;
            }

            for (var i = 0; i < layers.Length; i++)
            {
                var layer = layers[i];
                if (layer == null)
                {
                    continue;
                }

                layer.metallic = 0f;
                layer.smoothness = 0f;
                layer.specular = Color.black;
            }
        }

        private static float[,] SampleToTerrainHeights(float[,] heightMap, int resolution)
        {
            var mapWidth = heightMap.GetLength(0);
            var mapHeight = heightMap.GetLength(1);
            var heights = new float[resolution, resolution];

            for (var ty = 0; ty < resolution; ty++)
            {
                for (var tx = 0; tx < resolution; tx++)
                {
                    var u = resolution == 1 ? 0f : tx / (float)(resolution - 1);
                    var v = resolution == 1 ? 0f : ty / (float)(resolution - 1);
                    var gx = u * (mapWidth - 1);
                    var gy = v * (mapHeight - 1);
                    heights[ty, tx] = BilinearSample(heightMap, gx, gy);
                }
            }

            return heights;
        }

        private static float BilinearSample(float[,] map, float x, float y)
        {
            var width = map.GetLength(0);
            var height = map.GetLength(1);
            var x0 = Mathf.Clamp(Mathf.FloorToInt(x), 0, width - 1);
            var y0 = Mathf.Clamp(Mathf.FloorToInt(y), 0, height - 1);
            var x1 = Mathf.Min(x0 + 1, width - 1);
            var y1 = Mathf.Min(y0 + 1, height - 1);
            var tx = x - x0;
            var ty = y - y0;
            var a = Mathf.Lerp(map[x0, y0], map[x1, y0], tx);
            var b = Mathf.Lerp(map[x0, y1], map[x1, y1], tx);
            return Mathf.Lerp(a, b, ty);
        }

        private static int NextPowerOfTwoPlusOne(int value)
        {
            var power = 1;
            while (power + 1 < value)
            {
                power <<= 1;
            }

            return power + 1;
        }
    }
}
