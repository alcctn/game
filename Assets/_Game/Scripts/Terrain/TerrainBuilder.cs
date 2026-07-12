using CleanEnergy.Map;
using UnityEngine;

namespace CleanEnergy.TerrainGeneration
{
    /// <summary>
    /// Applies a height map to a Unity Terrain, aligned with the logical grid origin.
    /// </summary>
    public sealed class TerrainBuilder
    {
        private UnityEngine.Terrain _terrain;

        public UnityEngine.Terrain CurrentTerrain => _terrain;

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
                shader = Shader.Find("Universal Render Pipeline/Lit");
            }

            if (shader == null)
            {
                return;
            }

            var mat = terrain.materialTemplate;
            if (mat == null || mat.shader != shader)
            {
                mat = new Material(shader) { name = "GeneratedTerrain_URP" };
            }

            var grass = new Color(0.32f, 0.52f, 0.26f, 1f);
            if (mat.HasProperty("_Color"))
            {
                mat.SetColor("_Color", grass);
            }

            if (mat.HasProperty("_BaseColor"))
            {
                mat.SetColor("_BaseColor", grass);
            }

            if (mat.HasProperty("_DiffuseColor"))
            {
                mat.SetColor("_DiffuseColor", grass);
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
